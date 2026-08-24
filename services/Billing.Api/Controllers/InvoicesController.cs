using Billing.Api.Clients;
using Billing.Api.Data;
using Billing.Api.Dtos;
using Billing.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace Billing.Api.Controllers;

[ApiController]
[Route("api/invoices")]
public class InvoicesController : ControllerBase
{
    private readonly BillingDbContext _context;
    private readonly InventoryClient _inventoryClient;

    public InvoicesController(BillingDbContext context, InventoryClient inventoryClient)
    {
        _context = context;
        _inventoryClient = inventoryClient;
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceResponse>> Create(
        CreateInvoiceRequest request)
    {
        var groupedItems = request.Items
            .GroupBy(item => item.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                Quantity = group.Sum(item => item.Quantity)
            })
            .ToList();

        try
        {
            foreach (var item in groupedItems)
            {
                var product = await _inventoryClient
                    .GetProductByIdAsync(item.ProductId);

                if (product is null)
                {
                    return BadRequest(new
                    {
                        message = "Um dos produtos informados não existe.",
                        productId = item.ProductId
                    });
                }
            }
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    message =
                        "O serviço de estoque está temporariamente indisponível. Tente novamente."
                });
        }

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            Status = InvoiceStatus.Open,
            CreatedAt = DateTime.UtcNow,

            Items = groupedItems
                .Select(item => new InvoiceItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                })
                .ToList()
        };

        _context.Invoices.Add(invoice);

        await _context.SaveChangesAsync();

        var response = MapToResponse(invoice);

        return CreatedAtAction(
            nameof(GetById),
            new { id = invoice.Id },
            response
        );
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InvoiceResponse>>> GetAll()
    {
        var invoices = await _context.Invoices
            .AsNoTracking()
            .Include(invoice => invoice.Items)
            .OrderByDescending(invoice => invoice.Number)
            .ToListAsync();

        return Ok(invoices.Select(MapToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InvoiceResponse>> GetById(Guid id)
    {
        var invoice = await _context.Invoices
            .AsNoTracking()
            .Include(invoice => invoice.Items)
            .FirstOrDefaultAsync(invoice => invoice.Id == id);

        if (invoice is null)
        {
            return NotFound(new
            {
                message = "Nota fiscal não encontrada."
            });
        }

        return Ok(MapToResponse(invoice));
    }

    private static InvoiceResponse MapToResponse(Invoice invoice)
    {
        return new InvoiceResponse
        {
            Id = invoice.Id,
            Number = invoice.Number,
            Status = invoice.Status.ToString(),
            CreatedAt = invoice.CreatedAt,

            Items = invoice.Items
                .Select(item => new InvoiceItemResponse
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                })
                .ToList()
        };
    }

    [HttpPost("{id:guid}/print")]
    public async Task<ActionResult<InvoiceResponse>> Print(Guid id)
    {
        var invoice = await _context.Invoices
            .Include(invoice => invoice.Items)
            .FirstOrDefaultAsync(invoice => invoice.Id == id);

        if (invoice is null)
        {
            return NotFound(new
            {
                message = "Nota fiscal não encontrada."
            });
        }

        if (invoice.Status != InvoiceStatus.Open)
        {
            return Conflict(new
            {
                message = "Somente notas abertas podem ser impressas."
            });
        }

        var stockRequest = new InventoryDecreaseStockRequest
        {
            Items = invoice.Items
                .Select(item => new InventoryDecreaseStockItemRequest
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                })
                .ToList()
        };

        try
        {
            var inventoryResponse = await _inventoryClient
                .DecreaseStockAsync(stockRequest);

            if (!inventoryResponse.IsSuccessStatusCode)
            {
                var error = await inventoryResponse.Content
                    .ReadFromJsonAsync<InventoryErrorResponse>();

                return StatusCode(
                    (int)inventoryResponse.StatusCode,
                    new
                    {
                        message = error?.Message
                            ?? "Não foi possível atualizar o estoque."
                    }
                );
            }
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    message =
                        "O serviço de estoque está temporariamente indisponível. Tente novamente."
                }
            );
        }

        invoice.Status = InvoiceStatus.Closed;

        await _context.SaveChangesAsync();

        return Ok(MapToResponse(invoice));
    }
}
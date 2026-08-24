using System.Net.Http.Json;
using Billing.Api.Clients;
using Billing.Api.Data;
using Billing.Api.Dtos;
using Billing.Api.Exceptions;
using Billing.Api.Models;
using Billing.Api.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Billing.Api.Services;

public class InvoiceService : IInvoiceService
{
    private readonly BillingDbContext _context;
    private readonly IInventoryClient _inventoryClient;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(
        BillingDbContext context,
        IInventoryClient inventoryClient,
        ILogger<InvoiceService> logger)
    {
        _context = context;
        _inventoryClient = inventoryClient;
        _logger = logger;
    }

    public async Task<InvoiceResponse> CreateAsync(
        CreateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
        "Creating invoice with {ItemCount} item(s).",
        request.Items.Count);

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
                    .GetProductByIdAsync(item.ProductId, cancellationToken);

                if (product is null)
                {
                    throw new ProductNotFoundException(
                        item.ProductId);
                }
            }
        }
        catch (HttpRequestException)
        {
            throw new InventoryUnavailableException();
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

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
        "Invoice {InvoiceId} created with number {InvoiceNumber}.",
        invoice.Id,
        invoice.Number);

        return invoice.ToResponse();
    }

    public async Task<IReadOnlyCollection<InvoiceResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var invoices = await _context.Invoices
            .AsNoTracking()
            .Include(invoice => invoice.Items)
            .OrderByDescending(invoice => invoice.Number)
            .ToListAsync(cancellationToken);

        return invoices
            .Select(invoice => invoice.ToResponse())
            .ToList();
    }

    public async Task<InvoiceResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .AsNoTracking()
            .Include(invoice => invoice.Items)
            .FirstOrDefaultAsync(invoice => invoice.Id == id, cancellationToken: cancellationToken);

        if (invoice is null)
        {
            throw new InvoiceNotFoundException(id);
        }

        return invoice.ToResponse();
    }

    public async Task<InvoiceResponse> PrintAsync(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
        "Starting print operation for invoice {InvoiceId}.",
        id);

        var invoice = await _context.Invoices
            .Include(invoice => invoice.Items)
            .FirstOrDefaultAsync(invoice => invoice.Id == id, cancellationToken: cancellationToken);

        if (invoice is null)
        {
            throw new InvoiceNotFoundException(id);
        }

        if (invoice.Status != InvoiceStatus.Open)
        {
            throw new InvoiceNotOpenException();
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
            using var inventoryResponse =
                await _inventoryClient.DecreaseStockAsync(
                    stockRequest, cancellationToken);

            if (!inventoryResponse.IsSuccessStatusCode)
            {
                var error = await inventoryResponse.Content
                    .ReadFromJsonAsync<InventoryErrorResponse>(cancellationToken: cancellationToken);

                throw new InventoryOperationException(
                    (int)inventoryResponse.StatusCode,
                    error?.Detail
                        ?? "Não foi possível atualizar o estoque."
                );
            }
        }
        catch (HttpRequestException)
        {
            throw new InventoryUnavailableException();
        }

        invoice.Status = InvoiceStatus.Closed;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
        "Invoice {InvoiceId} successfully closed after stock update.",
        invoice.Id);

        return invoice.ToResponse();
    }
}
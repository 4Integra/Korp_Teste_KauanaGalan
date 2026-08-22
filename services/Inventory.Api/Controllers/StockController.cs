using Inventory.Api.Data;
using Inventory.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/stock")]
public class StockController : ControllerBase
{
    private readonly InventoryDbContext _context;

    public StockController(InventoryDbContext context)
    {
        _context = context;
    }

    [HttpPost("decrease")]
    public async Task<IActionResult> Decrease(
    DecreaseStockRequest request)
    {
        var requestedItems = request.Items
            .GroupBy(item => item.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                Quantity = group.Sum(item => item.Quantity)
            })
            .ToList();

        var productIds = requestedItems
            .Select(item => item.ProductId)
            .ToList();

        var products = await _context.Products
            .Where(product => productIds.Contains(product.Id))
            .ToListAsync();

        if (products.Count != productIds.Count)
        {
            var foundIds = products
                .Select(product => product.Id)
                .ToHashSet();

            var missingIds = productIds
                .Where(id => !foundIds.Contains(id))
                .ToList();

            return NotFound(new
            {
                message = "Um ou mais produtos não foram encontrados.",
                productIds = missingIds
            });
        }

        foreach (var requestedItem in requestedItems)
        {
            var product = products
                .First(product =>
                    product.Id == requestedItem.ProductId);

            if (product.StockQuantity < requestedItem.Quantity)
            {
                return Conflict(new
                {
                    message = "Estoque insuficiente.",
                    productId = product.Id,
                    productCode = product.Code,
                    availableQuantity = product.StockQuantity,
                    requestedQuantity = requestedItem.Quantity
                });
            }
        }

        foreach (var requestedItem in requestedItems)
        {
            var product = products
                .First(product =>
                    product.Id == requestedItem.ProductId);

            product.StockQuantity -= requestedItem.Quantity;
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Estoque atualizado com sucesso.",
            items = products.Select(product => new
            {
                product.Id,
                product.Code,
                product.StockQuantity
            })
        });
    }
}

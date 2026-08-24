using Inventory.Api.Data;
using Inventory.Api.Dtos;
using Inventory.Api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Services;

public class StockService : IStockService
{
    private readonly InventoryDbContext _context;
    private readonly ILogger<StockService> _logger;

    public StockService(
        InventoryDbContext context,
        ILogger<StockService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DecreaseStockResponse> DecreaseAsync(
        DecreaseStockRequest request,
        CancellationToken cancellationToken)
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
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
        {
            var foundIds = products
                .Select(product => product.Id)
                .ToHashSet();

            var missingIds = productIds
                .Where(id => !foundIds.Contains(id))
                .ToList();

            throw new ProductsNotFoundException(missingIds);
        }

        foreach (var requestedItem in requestedItems)
        {
            var product = products.First(
                product =>
                    product.Id == requestedItem.ProductId);

            if (product.StockQuantity < requestedItem.Quantity)
            {
                throw new InsufficientStockException(
                    product.Id,
                    product.Code,
                    product.StockQuantity,
                    requestedItem.Quantity);
            }
        }

        foreach (var requestedItem in requestedItems)
        {
            var product = products.First(
                product =>
                    product.Id == requestedItem.ProductId);

            product.StockQuantity -= requestedItem.Quantity;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Stock successfully decreased for {ProductCount} product(s).",
            products.Count);

        return new DecreaseStockResponse
        {
            Message = "Estoque atualizado com sucesso.",

            Items = products
                .Select(product => new DecreaseStockItemResponse
                {
                    ProductId = product.Id,
                    Code = product.Code,
                    StockQuantity = product.StockQuantity
                })
                .ToList()
        };
    }

    public async Task<IncreaseStockResponse> IncreaseAsync(
    IncreaseStockRequest request,
    CancellationToken cancellationToken)
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
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
        {
            var foundIds = products
                .Select(product => product.Id)
                .ToHashSet();

            var missingIds = productIds
                .Where(id => !foundIds.Contains(id))
                .ToList();

            throw new ProductsNotFoundException(missingIds);
        }

        foreach (var requestedItem in requestedItems)
        {
            var product = products.First(
                product =>
                    product.Id == requestedItem.ProductId);

            product.StockQuantity += requestedItem.Quantity;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Stock successfully increased for {ProductCount} product(s).",
            products.Count);

        return new IncreaseStockResponse
        {
            Message = "Estoque atualizado com sucesso.",

            Items = products
                .Select(product => new IncreaseStockItemResponse
                {
                    ProductId = product.Id,
                    Code = product.Code,
                    StockQuantity = product.StockQuantity
                })
                .ToList()
        };
    }
}
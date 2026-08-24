using Inventory.Api.Data;
using Inventory.Api.Dtos;
using Inventory.Api.Exceptions;
using Inventory.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Services;

public class ProductService : IProductService
{
    private readonly InventoryDbContext _context;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        InventoryDbContext context,
        ILogger<ProductService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<Product>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _context.Products
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Product> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(
                product => product.Id == id,
                cancellationToken);

        if (product is null)
        {
            throw new ProductNotFoundException(id);
        }

        return product;
    }

    public async Task<Product> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var code = request.Code.Trim();
        var description = request.Description.Trim();

        var codeExists = await _context.Products
            .AnyAsync(
                product => product.Code == code,
                cancellationToken);

        if (codeExists)
        {
            throw new ProductCodeAlreadyExistsException(code);
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Code = code,
            Description = description,
            StockQuantity = request.StockQuantity
        };

        _context.Products.Add(product);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Product {ProductId} with code {ProductCode} created.",
            product.Id,
            product.Code);

        return product;
    }
}
using Inventory.Api.Dtos;
using Inventory.Api.Models;

namespace Inventory.Api.Services;

public interface IProductService
{
    Task<IReadOnlyCollection<Product>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<Product> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<Product> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken);
}
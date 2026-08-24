using Inventory.Api.Dtos;

namespace Inventory.Api.Services;

public interface IStockService
{
    Task<DecreaseStockResponse> DecreaseAsync(
        DecreaseStockRequest request,
        CancellationToken cancellationToken);
}
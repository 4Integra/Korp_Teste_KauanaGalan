using Inventory.Api.Dtos;

namespace Inventory.Api.Services;

public interface IStockService
{
    Task<DecreaseStockResponse> DecreaseAsync(
        DecreaseStockRequest request,
        CancellationToken cancellationToken);

    Task<IncreaseStockResponse> IncreaseAsync(
        IncreaseStockRequest request,
        CancellationToken cancellationToken);
}
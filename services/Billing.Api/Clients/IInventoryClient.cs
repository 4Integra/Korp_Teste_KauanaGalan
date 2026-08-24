using Billing.Api.Dtos;

namespace Billing.Api.Clients;

public interface IInventoryClient
{
    Task<InventoryProductResponse?> GetProductByIdAsync(
        Guid productId,
        CancellationToken cancellationToken);

    Task<HttpResponseMessage> DecreaseStockAsync(
        InventoryDecreaseStockRequest request,
        CancellationToken cancellationToken);
}
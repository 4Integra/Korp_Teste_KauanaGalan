using System.Net;
using System.Net.Http.Json;
using Billing.Api.Dtos;

namespace Billing.Api.Clients;

public class InventoryClient : IInventoryClient
{
    private readonly HttpClient _httpClient;

    public InventoryClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<InventoryProductResponse?> GetProductByIdAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(
            $"/api/products/{productId}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<InventoryProductResponse>(
                cancellationToken: cancellationToken);
    }

    public async Task<HttpResponseMessage> DecreaseStockAsync(
        InventoryDecreaseStockRequest request,
        CancellationToken cancellationToken)
    {
        return await _httpClient.PostAsJsonAsync(
            "/api/stock/decrease",
            request,
            cancellationToken);
    }
}
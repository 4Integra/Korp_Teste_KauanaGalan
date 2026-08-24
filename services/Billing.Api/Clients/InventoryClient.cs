using System.Net;
using System.Net.Http.Json;
using Billing.Api.Dtos;

namespace Billing.Api.Clients;

public class InventoryClient
{
    private readonly HttpClient _httpClient;

    public InventoryClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<InventoryProductResponse?> GetProductByIdAsync(
        Guid productId)
    {
        var response = await _httpClient.GetAsync(
            $"/api/products/{productId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<InventoryProductResponse>();
    }
}
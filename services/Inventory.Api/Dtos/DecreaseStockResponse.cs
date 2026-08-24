namespace Inventory.Api.Dtos;

public class DecreaseStockResponse
{
    public string Message { get; set; } = string.Empty;

    public List<DecreaseStockItemResponse> Items { get; set; } = [];
}

public class DecreaseStockItemResponse
{
    public Guid ProductId { get; set; }

    public string Code { get; set; } = string.Empty;

    public int StockQuantity { get; set; }
}
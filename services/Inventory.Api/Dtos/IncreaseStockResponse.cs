namespace Inventory.Api.Dtos;

public class IncreaseStockResponse
{
    public string Message { get; set; } = string.Empty;

    public List<IncreaseStockItemResponse> Items { get; set; } = [];
}

public class IncreaseStockItemResponse
{
    public Guid ProductId { get; set; }

    public string Code { get; set; } = string.Empty;

    public int StockQuantity { get; set; }
}
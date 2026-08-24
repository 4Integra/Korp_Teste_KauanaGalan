namespace Billing.Api.Dtos;

public class InventoryDecreaseStockRequest
{
    public List<InventoryDecreaseStockItemRequest> Items { get; set; } = [];
}

public class InventoryDecreaseStockItemRequest
{
    public Guid ProductId { get; set; }

    public int Quantity { get; set; }
}
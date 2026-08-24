using System.ComponentModel.DataAnnotations;

namespace Inventory.Api.Dtos;

public class IncreaseStockRequest
{
    [Required]
    [MinLength(1)]
    public List<IncreaseStockItemRequest> Items { get; set; } = [];
}

public class IncreaseStockItemRequest
{
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
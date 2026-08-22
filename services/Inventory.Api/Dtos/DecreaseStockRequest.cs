using System.ComponentModel.DataAnnotations;

namespace Inventory.Api.Dtos;

public class DecreaseStockRequest
{
    [Required]
    [MinLength(1)]
    public List<DecreaseStockItemRequest> Items { get; set; } = [];
}

public class DecreaseStockItemRequest
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
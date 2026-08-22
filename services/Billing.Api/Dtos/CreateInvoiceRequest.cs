using System.ComponentModel.DataAnnotations;

namespace Billing.Api.Dtos;

public class CreateInvoiceRequest
{
    [Required]
    [MinLength(1)]
    public List<CreateInvoiceItemRequest> Items { get; set; } = [];
}

public class CreateInvoiceItemRequest
{
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
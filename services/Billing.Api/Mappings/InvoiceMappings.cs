using Billing.Api.Dtos;
using Billing.Api.Models;

namespace Billing.Api.Mappings;

public static class InvoiceMappings
{
    public static InvoiceResponse ToResponse(this Invoice invoice)
    {
        return new InvoiceResponse
        {
            Id = invoice.Id,
            Number = invoice.Number,
            Status = invoice.Status.ToString(),
            CreatedAt = DateTime.SpecifyKind(
                invoice.CreatedAt,
                DateTimeKind.Utc),

            Items = invoice.Items
                .Select(item => new InvoiceItemResponse
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                })
                .ToList()
        };
    }
}

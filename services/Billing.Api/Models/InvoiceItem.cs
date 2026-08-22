namespace Billing.Api.Models;

public class InvoiceItem
{
    public Guid Id { get; set; }

    public Guid InvoiceId { get; set; }

    public Invoice Invoice { get; set; } = null!;

    public Guid ProductId { get; set; }

    public int Quantity { get; set; }
}
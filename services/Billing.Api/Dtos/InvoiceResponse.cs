namespace Billing.Api.Dtos;

public class InvoiceResponse
{
    public Guid Id { get; set; }

    public int Number { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public List<InvoiceItemResponse> Items { get; set; } = [];
}

public class InvoiceItemResponse
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public int Quantity { get; set; }
}
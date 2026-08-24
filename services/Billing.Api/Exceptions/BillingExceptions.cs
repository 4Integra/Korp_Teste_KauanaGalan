namespace Billing.Api.Exceptions;

public class ProductNotFoundException : Exception
{
    public Guid ProductId { get; }

    public ProductNotFoundException(Guid productId)
        : base("Um dos produtos informados não existe.")
    {
        ProductId = productId;
    }
}

public class InvoiceNotFoundException : Exception
{
    public InvoiceNotFoundException(Guid invoiceId)
        : base($"Nota fiscal {invoiceId} não encontrada.")
    {
    }
}

public class InvoiceNotOpenException : Exception
{
    public InvoiceNotOpenException()
        : base("Somente notas abertas podem ser impressas.")
    {
    }
}

public class InventoryUnavailableException : Exception
{
    public InventoryUnavailableException()
        : base(
            "O serviço de estoque está temporariamente indisponível. Tente novamente.")
    {
    }
}

public class InventoryOperationException : Exception
{
    public int StatusCode { get; }

    public InventoryOperationException(
        int statusCode,
        string message)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
namespace Inventory.Api.Exceptions;

public class ProductNotFoundException : Exception
{
    public ProductNotFoundException(Guid productId)
        : base($"Produto {productId} não encontrado.")
    {
    }
}

public class ProductCodeAlreadyExistsException : Exception
{
    public string Code { get; }

    public ProductCodeAlreadyExistsException(string code)
        : base($"Já existe um produto com o código '{code}'.")
    {
        Code = code;
    }
}

public class ProductsNotFoundException : Exception
{
    public IReadOnlyCollection<Guid> ProductIds { get; }

    public ProductsNotFoundException(
        IReadOnlyCollection<Guid> productIds)
        : base("Um ou mais produtos não foram encontrados.")
    {
        ProductIds = productIds;
    }
}

public class InsufficientStockException : Exception
{
    public Guid ProductId { get; }

    public string ProductCode { get; }

    public int AvailableQuantity { get; }

    public int RequestedQuantity { get; }

    public InsufficientStockException(
        Guid productId,
        string productCode,
        int availableQuantity,
        int requestedQuantity)
        : base("Estoque insuficiente.")
    {
        ProductId = productId;
        ProductCode = productCode;
        AvailableQuantity = availableQuantity;
        RequestedQuantity = requestedQuantity;
    }
}
using Billing.Api.Dtos;

namespace Billing.Api.Services;

public interface IInvoiceService
{
    Task<InvoiceResponse> CreateAsync(
        CreateInvoiceRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<InvoiceResponse>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<InvoiceResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<InvoiceResponse> PrintAsync(
        Guid id,
        CancellationToken cancellationToken);
}
using Billing.Api.Dtos;
using Billing.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Billing.Api.Controllers;

[ApiController]
[Route("api/invoices")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoicesController(
        IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceResponse>> Create(
        CreateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        var invoice = await _invoiceService.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = invoice.Id },
            invoice);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InvoiceResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var invoices = await _invoiceService.GetAllAsync(
            cancellationToken);

        return Ok(invoices);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InvoiceResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var invoice = await _invoiceService.GetByIdAsync(
            id,
            cancellationToken);

        return Ok(invoice);
    }

    [HttpPost("{id:guid}/print")]
    public async Task<ActionResult<InvoiceResponse>> Print(
        Guid id,
        CancellationToken cancellationToken)
    {
        var invoice = await _invoiceService.PrintAsync(
            id,
            cancellationToken);

        return Ok(invoice);
    }
}
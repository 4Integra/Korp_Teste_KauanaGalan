using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Billing.Api.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = exception switch
        {
            ProductNotFoundException ex => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Produto inválido",
                Detail = ex.Message,
                Extensions =
                {
                    ["productId"] = ex.ProductId
                }
            },

            InvoiceNotFoundException ex => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Nota fiscal não encontrada",
                Detail = ex.Message
            },

            InvoiceNotOpenException ex => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Nota fiscal não pode ser impressa",
                Detail = ex.Message
            },

            InventoryUnavailableException ex => new ProblemDetails
            {
                Status = StatusCodes.Status503ServiceUnavailable,
                Title = "Serviço de estoque indisponível",
                Detail = ex.Message
            },

            InventoryOperationException ex => new ProblemDetails
            {
                Status = ex.StatusCode,
                Title = "Falha na operação de estoque",
                Detail = ex.Message
            },

            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Erro interno",
                Detail = "Ocorreu um erro inesperado."
            }
        };

        if (problemDetails.Status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unexpected error while processing the request.");
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Request failed with status code {StatusCode}",
                problemDetails.Status);
        }

        httpContext.Response.StatusCode =
            problemDetails.Status
            ?? StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }
}
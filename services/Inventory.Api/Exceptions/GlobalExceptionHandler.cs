using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Exceptions;

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
                Status = StatusCodes.Status404NotFound,
                Title = "Produto não encontrado",
                Detail = ex.Message
            },

            ProductCodeAlreadyExistsException ex => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Código de produto já cadastrado",
                Detail = ex.Message,
                Extensions =
                {
                    ["code"] = ex.Code
                }
            },
            ProductsNotFoundException ex => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Produtos não encontrados",
                Detail = ex.Message,
                Extensions =
                {
                    ["productIds"] = ex.ProductIds
                }
            },

            InsufficientStockException ex => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Estoque insuficiente",
                Detail = ex.Message,
                Extensions =
                {
                    ["productId"] = ex.ProductId,
                    ["productCode"] = ex.ProductCode,
                    ["availableQuantity"] = ex.AvailableQuantity,
                    ["requestedQuantity"] = ex.RequestedQuantity
                }
            },

            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Erro interno",
                Detail = "Ocorreu um erro inesperado."
            }
        };

        if (problemDetails.Status ==
            StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unexpected error while processing the request.");
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Request failed with status code {StatusCode}.",
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
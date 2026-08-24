using Inventory.Api.Dtos;
using Inventory.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/stock")]
public class StockController : ControllerBase
{
    private readonly IStockService _stockService;

    public StockController(
        IStockService stockService)
    {
        _stockService = stockService;
    }

    [HttpPost("decrease")]
    public async Task<ActionResult<DecreaseStockResponse>> Decrease(
        DecreaseStockRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _stockService.DecreaseAsync(
            request,
            cancellationToken);

        return Ok(result);
    }
}
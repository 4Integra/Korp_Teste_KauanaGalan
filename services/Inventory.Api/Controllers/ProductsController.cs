using Inventory.Api.Dtos;
using Inventory.Api.Models;
using Inventory.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(
        IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll(
        CancellationToken cancellationToken)
    {
        var products =
            await _productService.GetAllAsync(
                cancellationToken);

        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Product>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var product =
            await _productService.GetByIdAsync(
                id,
                cancellationToken);

        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product =
            await _productService.CreateAsync(
                request,
                cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            product
        );
    }
}
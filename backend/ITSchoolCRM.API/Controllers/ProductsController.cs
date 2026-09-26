using ITSchoolCRM.API.Auth;
using ITSchoolCRM.API.DTOs.Products;
using ITSchoolCRM.API.Services.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<List<ProductDto>>> GetAll(CancellationToken cancellationToken)
    {
        var products = await _service.GetAllAsync(cancellationToken);

        return Ok(products);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<ProductDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var product = await _service.GetByIdAsync(id, cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpGet("active")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<List<ProductDto>>> GetActive(CancellationToken cancellationToken)
    {
        var products = await _service.GetActiveAsync(cancellationToken);

        return Ok(products);
    }

    [HttpGet("search")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<List<ProductDto>>> Search([FromQuery] string? search, CancellationToken cancellationToken)
    {
        var products = await _service.SearchAsync(search, cancellationToken);

        return Ok(products);
    }

    [HttpPost]
    [Authorize(Policy = Policies.ManagerAccess)]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto dto, CancellationToken cancellationToken)
    {
        var product = await _service.CreateAsync(dto, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.ManagerAccess)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto, CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateAsync(id, dto, cancellationToken);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Policies.AdminAccess)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
using Microsoft.AspNetCore.Mvc;
using PRN232.API.Models;
using PRN232.API.Services;

namespace PRN232.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        this._service = service;
    }

    // TC01: Get all products
    [HttpGet]
    public IActionResult LoadProducts()
    {
        return Ok(_service.GetAll());
    }

    // TC02: Create product
    [HttpPost]
    public IActionResult InsertProduct([FromBody] CreateProductDto inputDto)
    {
        var prod = _service.Create(inputDto);
        return CreatedAtAction("GetProductById", new { id = prod.Id }, prod);
    }

    // TC03: Get product by ID
    [HttpGet("{id:guid}")]
    public IActionResult GetProductById(Guid id)
    {
        var data = _service.GetById(id);
        if (data is null)
            return NotFound(new { message = $"Product with ID {id} was not found." });
        return Ok(data);
    }

    // TC04: Update product
    [HttpPut("{id:guid}")]
    public IActionResult UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
    {
        return StatusCode(500, new { message = "Failed to update product for student" });
    }
}

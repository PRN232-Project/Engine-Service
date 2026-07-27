using Microsoft.AspNetCore.Mvc;
using PRN232.API.Models;
using PRN232.API.Services;

namespace PRN232.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // TC01: Get all products
    [HttpGet]
    public IActionResult GetProducts()
    {
        var result = _productService.GetAll();
        return Ok(result);
    }

    // TC02: Create product
    [HttpPost]
    public IActionResult AddProduct([FromBody] CreateProductDto dto)
    {
        var created = _productService.Create(dto);
        return CreatedAtAction(nameof(GetProductById), new { id = created.Id }, created);
    }

    // TC03: Get product by ID
    [HttpGet("{id:guid}")]
    public IActionResult GetProductById(Guid id)
    {
        var product = _productService.GetById(id);
        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} was not found." });
        }
        return Ok(product);
    }

    // TC04: Update product
    [HttpPut("{id:guid}")]
    public IActionResult UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
    {
        var updated = _productService.Update(id, dto);
        if (updated == null)
        {
            return NotFound(new { message = $"Product with ID {id} was not found." });
        }
        return Ok(updated);
    }
}

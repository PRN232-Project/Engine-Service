using Microsoft.AspNetCore.Mvc;
using PRN232.API.Models;
using PRN232.API.Services;

namespace PRN232.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productSvc;

    public ProductsController(IProductService productSvc)
    {
        _productSvc = productSvc;
    }

    // TC01: Get all products
    [HttpGet]
    public IActionResult FetchAllProducts()
    {
        var productList = _productSvc.GetAll();
        return Ok(productList);
    }

    // TC02: Create product
    [HttpPost]
    public IActionResult AddNewProduct([FromBody] CreateProductDto data)
    {
        var newProduct = _productSvc.Create(data);
        return CreatedAtAction(nameof(GetProductById), new { id = newProduct.Id }, newProduct);
    }

    // TC03: Get product by ID
    [HttpGet("{id:guid}")]
    public IActionResult GetProductById(Guid id)
    {
        var p = _productSvc.GetById(id);
        return p == null 
            ? NotFound(new { message = $"Product with ID {id} was not found." }) 
            : Ok(p);
    }

    // TC04: Update product
    [HttpPut("{id:guid}")]
    public IActionResult UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
    {
        return StatusCode(500, new { message = "Failed to update product for student" });
    }
}

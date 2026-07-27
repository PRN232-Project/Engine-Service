
using System;
using Microsoft.AspNetCore.Mvc;
using PRN232.Services;
namespace PRN232.API.Controllers {
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase {
        private readonly ProductService _svc;
        public ProductsController(ProductService svc) { _svc = svc; }
        
        [HttpGet]
        public IActionResult Get() {
            // [GET_ERROR_PLACEHOLDER]
            return Ok(_svc.GetAll());
        }
        
        public class CreateDto { public string Name { get; set; } public decimal Price { get; set; } }
        
        [HttpPost]
        public IActionResult Post(CreateDto dto) {
            var created = _svc.Create(dto.Name, dto.Price);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }
    }
}

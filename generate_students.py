import os
import shutil
import subprocess

base_dir = r"F:\PRN_Folder\PRN232\Team_Project\All Engine\Engine_Service\sample-student-submission"

# Clear directory
if os.path.exists(base_dir):
    shutil.rmtree(base_dir)
os.makedirs(base_dir)

def run_cmd(cmd, cwd):
    subprocess.run(cmd, cwd=cwd, shell=True, check=True)

def create_solution(student_id, error_type):
    student_dir = os.path.join(base_dir, student_id)
    os.makedirs(student_dir)
    
    # SLN
    run_cmd(f"dotnet new sln -n PRN232_PE_PRODUCT_API_{student_id}", cwd=student_dir)
    
    # Projects
    run_cmd("dotnet new webapi -n PRN232.API -f net8.0", cwd=student_dir)
    run_cmd("dotnet new classlib -n PRN232.Services -f net8.0", cwd=student_dir)
    run_cmd("dotnet new classlib -n PRN232.Repositories -f net8.0", cwd=student_dir)
    
    # Add EF Core to Repositories
    run_cmd("dotnet add PRN232.Repositories/PRN232.Repositories.csproj package Microsoft.EntityFrameworkCore.SqlServer -v 8.0.0", cwd=student_dir)
    
    # Add to SLN
    run_cmd("dotnet sln add PRN232.API/PRN232.API.csproj PRN232.Services/PRN232.Services.csproj PRN232.Repositories/PRN232.Repositories.csproj", cwd=student_dir)
    
    # References
    run_cmd("dotnet add PRN232.API/PRN232.API.csproj reference PRN232.Services/PRN232.Services.csproj", cwd=student_dir)
    run_cmd("dotnet add PRN232.Services/PRN232.Services.csproj reference PRN232.Repositories/PRN232.Repositories.csproj", cwd=student_dir)
    
    # Cleanup default files
    for f in [
        r"PRN232.API\WeatherForecast.cs",
        r"PRN232.API\Controllers\WeatherForecastController.cs",
        r"PRN232.Services\Class1.cs",
        r"PRN232.Repositories\Class1.cs"
    ]:
        path = os.path.join(student_dir, f)
        if os.path.exists(path):
            os.remove(path)
            
    # Repositories Code
    model_code = """
using System;
namespace PRN232.Repositories {
    public class Product {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
"""
    db_context_code = """
using Microsoft.EntityFrameworkCore;
namespace PRN232.Repositories {
    public class MyDbContext : DbContext {
        public DbSet<Product> Products { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            // [HARDCODE_CONNECTION_STRING_PLACEHOLDER]
        }
    }
    
    public class ProductRepo {
        private static readonly System.Collections.Generic.List<Product> _db = new();
        public System.Collections.Generic.List<Product> GetAll() => _db;
        public Product Add(Product p) { _db.Add(p); return p; }
    }
}
"""
    if error_type == "hardcode_db":
        db_context_code = db_context_code.replace("// [HARDCODE_CONNECTION_STRING_PLACEHOLDER]", 'optionsBuilder.UseSqlServer("Server=localhost;Database=fake;User Id=sa;Password=secret;");')

    with open(os.path.join(student_dir, "PRN232.Repositories", "Product.cs"), "w") as f: f.write(model_code)
    with open(os.path.join(student_dir, "PRN232.Repositories", "MyDbContext.cs"), "w") as f: f.write(db_context_code)
    
    # Services Code
    service_code = """
using System;
using System.Collections.Generic;
using PRN232.Repositories;
namespace PRN232.Services {
    public class ProductService {
        private readonly ProductRepo _repo = new();
        public List<Product> GetAll() => _repo.GetAll();
        public Product Create(string name, decimal price) => _repo.Add(new Product { Id = Guid.NewGuid(), Name = name, Price = price });
    }
}
"""
    with open(os.path.join(student_dir, "PRN232.Services", "ProductService.cs"), "w") as f: f.write(service_code)
    
    # API Code
    controller_code = """
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
"""
    os.makedirs(os.path.join(student_dir, "PRN232.API", "Controllers"), exist_ok=True)
    if error_type == "api_error": controller_code = controller_code.replace("// [GET_ERROR_PLACEHOLDER]", 'return StatusCode(500, "Internal Server Error");')

    with open(os.path.join(student_dir, "PRN232.API", "Controllers", "ProductsController.cs"), "w") as f: f.write(controller_code)
    
    program_code = """
using PRN232.Services;
var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSingleton<ProductService>();
var app = builder.Build();
// [COMPILE_ERROR_PLACEHOLDER]
app.MapControllers();
app.Run();
"""
    if error_type == "compile_error":
        program_code = program_code.replace("// [COMPILE_ERROR_PLACEHOLDER]", 'Console.WriteLine(UndefinedVariable);')

    with open(os.path.join(student_dir, "PRN232.API", "Program.cs"), "w") as f: f.write(program_code)
    
    appsettings_code = '{ "ConnectionStrings": { "DefaultConnection": "Server=localhost;Database=Db;Trusted_Connection=True;" } }'
    with open(os.path.join(student_dir, "PRN232.API", "appsettings.json"), "w") as f: f.write(appsettings_code)
    
    if error_type == "missing_appsettings":
        os.remove(os.path.join(student_dir, "PRN232.API", "appsettings.json"))

print("Generating SE180003 (Compile Error)...")
create_solution("SE180003", "compile_error")

print("Generating SE180004 (Hardcoded DB in DbContext)...")
create_solution("SE180004", "hardcode_db")

print("Generating SE180005 (API Error)...")
create_solution("SE180005", "api_error")

print("Generating SE180006 (Perfect)...")
create_solution("SE180006", "perfect")

print("Done generating 3-tier architectures!")

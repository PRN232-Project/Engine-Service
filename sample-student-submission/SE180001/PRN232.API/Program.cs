var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var db = new List<ProductModel>
{
    new(Guid.NewGuid(), "Laptop Dell XPS", 1200.00m),
    new(Guid.NewGuid(), "Wireless Mouse", 25.50m)
};

// TC01: Get all products - PASS
app.MapGet("/api/products", () => Results.Ok(db));

// TC02: Create new product - PASS
app.MapPost("/api/products", (CreateProductRequest req) =>
{
    var item = new ProductModel(Guid.NewGuid(), req.Name, req.Price);
    db.Add(item);
    return Results.Created($"/api/products/{item.Id}", item);
});

// TC03: Get product by id - FAIL (Student SE180001 forgot to match id correctly)
app.MapGet("/api/products/{id:guid}", (Guid id) =>
{
    // Hardcoded 404 response for testing failure
    return Results.NotFound(new { message = "Product not found or unhandled route" });
});

// TC04: Update product - FAIL (Student SE180001 returns error)
app.MapPut("/api/products/{id:guid}", (Guid id, UpdateProductRequest req) =>
{
    return Results.Problem("Not implemented put logic for student SE180001", statusCode: 500);
});

app.Run();

public record ProductModel(Guid Id, string Name, decimal Price);
public record CreateProductRequest(string Name, decimal Price);
public record UpdateProductRequest(string Name, decimal Price);

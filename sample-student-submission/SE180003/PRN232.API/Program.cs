
using PRN232.Services;
var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSingleton<ProductService>();
var app = builder.Build();
Console.WriteLine(UndefinedVariable);
app.MapControllers();
app.Run();

using PRN232.GradingEngine.Application;
using PRN232.GradingEngine.Infrastructure;
using PRN232.GradingEngine.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add controllers support
builder.Services.AddControllers();

// Add Swagger generator support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register clean architecture layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Seed Database automatically on startup
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<GradingDbContext>();
        await DbInitializer.SeedAsync(context);
    }
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"[Warning] Database initialization failed: {ex.Message}");
    Console.WriteLine("The application will continue to run, but database-dependent features might fail.");
    Console.ResetColor();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Grading Engine API v1");
    });
}

app.UseStaticFiles();

// Redirect root to index.html
app.MapGet("/", async context =>
{
    context.Response.Redirect("/index.html");
    await Task.CompletedTask;
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

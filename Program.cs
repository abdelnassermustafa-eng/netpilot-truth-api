using TruthApi.Services;
using TruthApi.Models;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Bind ServiceConfig
builder.Services.Configure<ServiceConfig>(
    builder.Configuration.GetSection("Service")
);

// Register HealthService
builder.Services.AddSingleton<HealthService>();

var app = builder.Build();

// Global exception handler
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.ContentType = "application/json";

        var error = new ErrorResponse
        {
            Success = false,
            Error = "An unexpected error occurred",
            Timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(error);

        await context.Response.WriteAsync(json);
    });
});

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

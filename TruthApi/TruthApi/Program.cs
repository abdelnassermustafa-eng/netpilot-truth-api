using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TruthApi.Models;
using TruthApi.Services;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Bind ServiceConfig
builder.Services.Configure<ServiceConfig>(
    builder.Configuration.GetSection("Service")
);

// Bind JwtConfig
builder.Services.Configure<JwtConfig>(
    builder.Configuration.GetSection("Jwt")
);

// Bind AwsConfig
builder.Services.Configure<AwsConfig>(
    builder.Configuration.GetSection("Aws")
);

// Configure JWT authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtConfig = jwtSection.Get<JwtConfig>();

var key = Encoding.UTF8.GetBytes(jwtConfig!.Key);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // dev only
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtConfig.Issuer,
        ValidAudience = jwtConfig.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Register services
builder.Services.AddSingleton<HealthService>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<AwsEc2Service>();
builder.Services.AddSingleton<NetworkValidationService>();
builder.Services.AddSingleton<ComputeValidator>();
builder.Services.AddSingleton<StorageValidator>();

var app = builder.Build();

// Global exception handler
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.ContentType = "application/json";

        var exception = context.Features
            .Get<IExceptionHandlerFeature>()
            ?.Error;

        var error = new ErrorResponse
        {
            Success = false,
            Error = exception?.ToString() ?? "An unexpected error occurred",
            Timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(error);
        await context.Response.WriteAsync(json);
    });
});

// Enable Swagger (dev-friendly)
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Simple audit middleware
app.Use(async (context, next) =>
{
    var user = context.User;

    if (user?.Identity?.IsAuthenticated == true)
    {
        var username = user.Identity.Name;
        var role = user.Claims
            .FirstOrDefault(c => c.Type.Contains("role"))?.Value;

        Console.WriteLine(
            $"[AUDIT] {DateTime.UtcNow:o} | User: {username} | Role: {role} | Path: {context.Request.Path}"
        );
    }

    await next();
});

app.MapControllers();

app.Run();

using Microsoft.EntityFrameworkCore;
using GLMS.API.Data;
using GLMS.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// ADD SWAGGER - THIS IS NEW
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "GLMS API",
        Version = "v1",
        Description = "Global Logistics Management System API"
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("GLMSDb"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ENABLE SWAGGER - THIS IS NEW
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GLMS API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (!db.Clients.Any())
    {
        db.Clients.Add(new Client { ClientId = 1, Name = "Global Importers Ltd", Email = "contact@global.com", Phone = "+27 11 234 5678", Address = "123 Main St", Region = "Africa", CreatedAt = DateTime.UtcNow });
        db.SaveChanges();
    }

    if (!db.Contracts.Any())
    {
        db.Contracts.Add(new Contract { ContractId = 1, ContractNumber = "CT-2026-001", ClientId = 1, StartDate = DateTime.UtcNow.AddMonths(-1), EndDate = DateTime.UtcNow.AddMonths(11), Status = ContractStatus.Active, ServiceLevel = ServiceLevel.Premium, ContractValueUSD = 50000, CreatedAt = DateTime.UtcNow });
        db.SaveChanges();
    }
}

Console.WriteLine("API running on http://localhost:5143");
Console.WriteLine("Swagger UI: http://localhost:5143/swagger");
app.Run();
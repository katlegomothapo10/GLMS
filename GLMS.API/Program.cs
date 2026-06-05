
using Microsoft.EntityFrameworkCore;
using GLMS.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Use In-Memory Database for API
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("GLMSDb"));

// CORS for MVC app
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

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();

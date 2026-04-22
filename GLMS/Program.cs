using Microsoft.EntityFrameworkCore;
using GLMS.Data;
using GLMS.Models;
using GLMS.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpClient<ICurrencyService, CurrencyService>();
builder.Services.AddMemoryCache();
builder.Services.AddSession();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Create uploads directory
var uploadsPath = Path.Combine(app.Environment.WebRootPath, "uploads", "contracts");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

//Seed the database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    dbContext.Database.EnsureCreated();

    if (!dbContext.Clients.Any())
    { 
        dbContext.Clients.AddRange(
            new Client { Name = "Global Importers Ltd", Email = "contact@globalimporters.com", Phone = "+27 11 234 5678", Address = "123 Main St, Johannesburg", Region = "Africa", ContactPerson = "John Doe", CreatedAt = DateTime.UtcNow },
            new Client { Name = "EuroTech Solutions", Email = "info@eurotech.eu", Phone = "+49 30 1234 5678", Address = "45 Berliner Str, Berlin", Region = "Europe", ContactPerson = "Anna Schmidt", CreatedAt = DateTime.UtcNow },
            new Client { Name = "Asia Pacific Trading", Email = "sales@aptrading.sg", Phone = "+65 6789 1234", Address = "88 Marina Blvd, Singapore", Region = "Asia", ContactPerson = "Tan Wei Ming", CreatedAt = DateTime.UtcNow }
        );
        dbContext.SaveChanges();

        dbContext.Contracts.AddRange(
            new Contract { ContractNumber = "CT-2024-001", ClientId = 1, StartDate = new DateTime(2024, 1, 1), EndDate = new DateTime(2024, 12, 31), Status = ContractStatus.Active, ServiceLevel = ServiceLevel.Premium, ContractValueUSD = 50000, SpecialTerms = "Priority handling", CreatedAt = DateTime.UtcNow },
            new Contract { ContractNumber = "CT-2024-002", ClientId = 2, StartDate = new DateTime(2023, 6, 1), EndDate = new DateTime(2024, 5, 31), Status = ContractStatus.Expired, ServiceLevel = ServiceLevel.Standard, ContractValueUSD = 25000, CreatedAt = DateTime.UtcNow },
            new Contract { ContractNumber = "CT-2024-003", ClientId = 3, StartDate = new DateTime(2024, 3, 1), EndDate = new DateTime(2025, 2, 28), Status = ContractStatus.Active, ServiceLevel = ServiceLevel.Enterprise, ContractValueUSD = 100000, CreatedAt = DateTime.UtcNow }
        );
        dbContext.SaveChanges();

        dbContext.ServiceRequests.AddRange(
            new ServiceRequest { RequestNumber = "SR-202401-001", ContractId = 1, Description = "Container shipping", CostUSD = 5000, CostZAR = 96250, Status = RequestStatus.Completed, ExchangeRateUsed = 19.25m, RequestedDate = DateTime.UtcNow.AddDays(-30) },
            new ServiceRequest { RequestNumber = "SR-202401-002", ContractId = 1, Description = "Air freight", CostUSD = 12500, CostZAR = 240625, Status = RequestStatus.InProgress, ExchangeRateUsed = 19.25m, RequestedDate = DateTime.UtcNow.AddDays(-20) },
            new ServiceRequest { RequestNumber = "SR-202402-001", ContractId = 3, Description = "Bulk cargo", CostUSD = 25000, CostZAR = 482500, Status = RequestStatus.Approved, ExchangeRateUsed = 19.30m, RequestedDate = DateTime.UtcNow.AddDays(-10) }
        );
        dbContext.SaveChanges();

        Console.WriteLine("Database seeded with test data!");
    }
}

app.Run();
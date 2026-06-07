using GLMS.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Register API Service
builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5143/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register Currency Service - ADD THIS
builder.Services.AddHttpClient<ICurrencyService, CurrencyService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddMemoryCache();

builder.Services.AddDistributedMemoryCache();
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

app.Run();
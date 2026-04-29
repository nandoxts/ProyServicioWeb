using Microsoft.AspNetCore.Session;
using Rotativa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ===================================================
// 1. SERVICIOS
// ===================================================

// MVC
builder.Services.AddControllersWithViews();

// HttpClient
builder.Services.AddHttpClient();

// HttpContextAccessor (Session en Layout)
builder.Services.AddHttpContextAccessor();

// Session
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ===================================================
// 2. CONFIGURACIONES EXTERNAS
// ===================================================
RotativaConfiguration.Setup(
    app.Environment.WebRootPath,
    "Rotativa"
);

// ===================================================
// 3. PIPELINE (ORDEN CORRECTO)
// ===================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseSession();       // 🔥 ANTES de Authorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
    
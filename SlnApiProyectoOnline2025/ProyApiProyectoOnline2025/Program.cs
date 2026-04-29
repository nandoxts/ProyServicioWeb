using Microsoft.EntityFrameworkCore;
using ProyApiProyectoOnline2025.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ==================================
// DB
// ==================================
var cadena = builder.Configuration.GetConnectionString("cn1");

builder.Services.AddDbContext<Proyectodiciembre2025Context>(
    x => x.UseSqlServer(cadena)
);

// ==================================
// CONTROLLERS + JSON
// ==================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// ==================================
// 🔔 SIGNALR
// ==================================
builder.Services.AddSignalR();

// ==================================
// 🔥 CORS (ESTO ES LO QUE FALTABA)
// ==================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:5140") // MVC
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ==================================
// SWAGGER
// ==================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ==================================
// PIPELINE (ORDEN CRÍTICO)
// ==================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔥 CORS ANTES DE TODO
app.UseCors("CorsPolicy");

app.UseAuthorization();

app.MapControllers();

// 🔔 HUB
app.MapHub<NotificacionHub>("/notificacionHub");

app.Run();

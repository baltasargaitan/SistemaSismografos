using Aplicacion;
using Aplicacion.Interfaces.Notificaciones;
using Aplicacion.Servicios.Notificaciones;
using Infraestructura;
using Infraestructura.Persistencia;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------------
//  SWAGGER Y CONTROLADORES
// ----------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Red Sísmica - Sistema Sismógrafos",
        Version = "v1",
        Description = "Backend de ejemplo para gestión de órdenes de inspección."
    });
});

// ----------------------------------------------------------
//  REGISTRO DE CAPAS DE INFRAESTRUCTURA Y APLICACIÓN
// ----------------------------------------------------------
builder.Services.AddInfraestructura(builder.Configuration);
builder.Services.AddAplicacion();

// ----------------------------------------------------------
//  CONFIGURACIÓN SMTP (SendGrid)
// ----------------------------------------------------------
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));

// ----------------------------------------------------------
//  REGISTRO DEL SUJETO Y OBSERVADORES (Patrón Observer)
// ----------------------------------------------------------
builder.Services.AddSingleton<ISujetoCierreOrden, SujetoCierreOrden>();
builder.Services.AddSingleton<IObservadorCierreOrden, ObservadorEmailSMTP>();
builder.Services.AddSingleton<IObservadorCierreOrden, ObservadorConsola>();

// ----------------------------------------------------------
//  CORS: Permitir acceso desde el frontend (Vite localhost)
// ----------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    policy
    .AllowAnyOrigin()       // permite cualquier origen (útil para test)
    .AllowAnyHeader()
    .AllowAnyMethod()
    );
});

// ----------------------------------------------------------
//  CONSTRUIR APP
// ----------------------------------------------------------
var app = builder.Build();

// ----------------------------------------------------------
//  HABILITAR CORS ANTES DE TODO
// ----------------------------------------------------------
app.UseCors("AllowAll");

// ----------------------------------------------------------
//  SUSCRIPCIÓN DE OBSERVADORES AL SUJETO (al iniciar app)
// ----------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var sujeto = scope.ServiceProvider.GetRequiredService<ISujetoCierreOrden>();
    var observadores = scope.ServiceProvider.GetServices<IObservadorCierreOrden>();
    foreach (var obs in observadores)
        sujeto.Suscribir(obs);
    Console.WriteLine("✅ Observadores suscritos correctamente al SujetoCierreOrden.");
}

// ----------------------------------------------------------
//  SEED DE BASE DE DATOS AL ARRANCAR LA APLICACIÓN
// ----------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        Console.WriteLine(" Ejecutando seed de base de datos...");
        await AppDbContextSeed.SeedAsync(context);
        Console.WriteLine(" Seed ejecutado correctamente.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($" Error en el seed: {ex}");
    }
}

// ----------------------------------------------------------
//  SWAGGER (solo en desarrollo)
// ----------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        c.RoutePrefix = string.Empty;
    });
}

// ----------------------------------------------------------
//  PIPELINE FINAL
// ----------------------------------------------------------
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ----------------------------------------------------------
//  EJECUCIÓN
// ----------------------------------------------------------
app.Run();

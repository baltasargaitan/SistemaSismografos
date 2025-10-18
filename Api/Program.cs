using Aplicacion;
using Infraestructura;
using Infraestructura.Persistencia; 
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

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

//  Registro de capas de infraestructura y aplicación
builder.Services.AddInfraestructura(builder.Configuration);
builder.Services.AddAplicacion();

var app = builder.Build();

//  SEED: poblar base de datos al iniciar la app
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

//  Ejecuta la aplicación
app.Run();

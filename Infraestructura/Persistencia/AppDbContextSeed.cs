using Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infraestructura.Persistencia
{
    public static class AppDbContextSeed
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // 🧱 Crear la base de datos y aplicar migraciones si faltan
            await context.Database.MigrateAsync();

            // 🚩 1️⃣ Estados
            if (!await context.Estados.AnyAsync())
            {
                var estados = new List<Estado>
                {
                    new Estado("OrdenInspeccion", "Pendiente"),
                    new Estado("OrdenInspeccion", "Cerrada"),
                    new Estado("Sismografo", "Operativo"),
                    new Estado("Sismografo", "FueraDeServicio")
                };
                await context.Estados.AddRangeAsync(estados);
                await context.SaveChangesAsync();
            }

            // 🚩 2️⃣ Roles
            if (!await context.Roles.AnyAsync())
            {
                var roles = new List<Rol>
                {
                    new Rol("ResponsableReparacion", "Encargado de coordinar reparaciones"),
                    new Rol("Inspector", "Empleado que realiza inspecciones")
                };
                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
            }

            // 🚩 3️⃣ Empleados
            if (!await context.Empleados.AnyAsync())
            {
                var emp1 = new Empleado("Juan", "Pérez", "juan.perez@empresa.com", "123456789");
                var emp2 = new Empleado("María", "Gómez", "maria.gomez@empresa.com", "987654321");

                await context.Empleados.AddRangeAsync(emp1, emp2);
                await context.SaveChangesAsync();
            }

            // 🚩 4️⃣ Estación + Sismógrafo
            if (!await context.EstacionesSismologicas.AnyAsync())
            {
                var estadoOperativo = await context.Estados
                    .FirstAsync(e => e.Ambito == "Sismografo" && e.NombreEstado == "Operativo");

                var sismografo = new Sismografo(
                    identificacionSismografo: "SISMO-001",
                    nroSerie: "SN-001",
                    fechaAdquisicion: DateTime.Now.AddYears(-2),
                    estadoActual: estadoOperativo
                );
                sismografo.SetEstadoActual(estadoOperativo);

                var estacion = new EstacionSismologica(
                    codigoEstacion: "EST-001",
                    nombre: "Estación Central",
                    latitud: -34.6037,
                    longitud: -58.3816,
                    documentoCertificacionAdquirida: "DOC-001",
                    nroCertificacionAdquisicion: "CERT-001",
                    fechaSolicitudCertificacion: DateTime.Now.AddYears(-1)
                );

                estacion.Sismografos.Add(sismografo);

                await context.EstacionesSismologicas.AddAsync(estacion);
                await context.SaveChangesAsync();
            }

            // 🚩 5️⃣ Orden de Inspección pendiente
            if (!await context.OrdenesDeInspeccion.AnyAsync())
            {
                var estadoPendiente = await context.Estados
                    .FirstAsync(e => e.Ambito == "OrdenInspeccion" && e.NombreEstado == "Pendiente");

                var estacion = await context.EstacionesSismologicas.FirstAsync();

                var orden = new OrdenDeInspeccion(
                    nroOrden: 1001,
                    fechaHoraInicio: DateTime.Now.AddHours(-5),
                    estado: estadoPendiente,
                    estacion: estacion
                );

                await context.OrdenesDeInspeccion.AddAsync(orden);
                await context.SaveChangesAsync();
            }

            // 🚩 6️⃣ Motivos Fuera de Servicio
            if (!await context.MotivosTipo.AnyAsync())
            {
                var motivos = new List<MotivoTipo>
                {
                    new MotivoTipo("1","Falla 1"),
                    new MotivoTipo("2","Falla 2"),
                    new MotivoTipo("3","Falla 3"),
                };

                await context.MotivosTipo.AddRangeAsync(motivos);
                await context.SaveChangesAsync();
            }

            Console.WriteLine(" Seed ejecutado correctamente con motivos fuera de servicio.");
        }
    }
}

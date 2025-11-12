//using Dominio.Entidades;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace Infraestructura.Persistencia
//{
//    public static class AppDbContextSeed
//    {
//        public static async Task SeedAsync(AppDbContext context)
//        {
//            await context.Database.MigrateAsync();

//            // Estados
//            if (!await context.Estados.AnyAsync())
//            {
//                var estados = new List<Estado>
//                {
//                    new Estado("OrdenInspeccion", "Pendiente"),
//                    new Estado("OrdenInspeccion", "Cerrada"),
//                    new Estado("OrdenInspeccion", "Completada"), // agregamos explícitamente “Realizada”
//                    new Estado("Sismografo", "Operativo"),
//                    new Estado("Sismografo", "FueraDeServicio")
//                };
//                await context.Estados.AddRangeAsync(estados);
//                await context.SaveChangesAsync();
//            }

//            // Roles
//            if (!await context.Roles.AnyAsync())
//            {
//                var roles = new List<Rol>
//                {
//                    new Rol("ResponsableReparacion", "Encargado de coordinar reparaciones"),
//                    new Rol("Inspector", "Empleado que realiza inspecciones")
//                };
//                await context.Roles.AddRangeAsync(roles);
//                await context.SaveChangesAsync();
//            }

//            // Empleados
//            if (!await context.Empleados.AnyAsync())
//            {
//                var emp1 = new Empleado("Juan", "Pérez", "ikermavi2015@gmail.com", "123456789");   // será el RI logueado
//                var emp2 = new Empleado("Sol", "Vega", "sol.vega@empresa.com", "987654321"); // otra inspectora
//                var emp3 = new Empleado("Marcos", "Pomenich", "marcos.pomenich@empresa.com", "5551234"); // responsable reparación

//                await context.Empleados.AddRangeAsync(emp1, emp2, emp3);
//                await context.SaveChangesAsync();
//            }

//            var empJuan = await context.Empleados.FirstAsync(e => e.Mail == "ikermavi2015@gmail.com");
//            var empMaria = await context.Empleados.FirstAsync(e => e.Mail == "sol.vega@empresa.com");

//            // Estaciones + Sismógrafos
//            if (!await context.EstacionesSismologicas.AnyAsync())
//            {
//                var estadoOperativo = await context.Estados
//                    .FirstAsync(e => e.Ambito == "Sismografo" && e.NombreEstado == "Operativo");

//                var estaciones = new List<EstacionSismologica>();

//                for (int i = 1; i <= 3; i++)
//                {
//                    var estacion = new EstacionSismologica(
//                        codigoEstacion: $"EST-00{i}",
//                        nombre: $"Estación {i}",
//                        latitud: -34.60 - i * 0.01,
//                        longitud: -58.38 - i * 0.01,
//                        documentoCertificacionAdquirida: $"DOC-00{i}",
//                        nroCertificacionAdquisicion: $"CERT-00{i}",
//                        fechaSolicitudCertificacion: DateTime.Now.AddYears(-1).AddMonths(i)
//                    );

//                    var sismografo = new Sismografo(
//                        identificacionSismografo: $"SISMO-00{i}",
//                        nroSerie: $"SN-00{i}",
//                        fechaAdquisicion: DateTime.Now.AddYears(-3).AddMonths(i),
//                        estadoActual: estadoOperativo
//                    );

//                    estacion.Sismografos.Add(sismografo);
//                    estaciones.Add(estacion);
//                }

//                await context.EstacionesSismologicas.AddRangeAsync(estaciones);
//                await context.SaveChangesAsync();
//            }

//            var estadoPendiente = await context.Estados.FirstAsync(e => e.Ambito == "OrdenInspeccion" && e.NombreEstado == "Pendiente");
//            var estadoCerrada = await context.Estados.FirstAsync(e => e.Ambito == "OrdenInspeccion" && e.NombreEstado == "Cerrada");
//            var estadoRealizada = await context.Estados.FirstAsync(e => e.Ambito == "OrdenInspeccion" && e.NombreEstado == "Completada");

//            var estacionesExistentes = await context.EstacionesSismologicas.ToListAsync();

//            // Ordenes de inspección
//            if (!await context.OrdenesDeInspeccion.AnyAsync())
//            {
//                var ordenes = new List<OrdenDeInspeccion>();

//                // Esta entra en el filtro (Empleado Juan, Estado “Completada”)
//                var o1 = new OrdenDeInspeccion(
//                    fechaHoraInicio: DateTime.Now.AddHours(-10),
//                    nroOrden: 1001,
//                    estado: estadoRealizada,
//                    estacion: estacionesExistentes[0],
//                    empleado: empJuan
//                );

//                // No entra: está Cerrada
//                var o2 = new OrdenDeInspeccion(
//                    fechaHoraInicio: DateTime.Now.AddHours(-20),
//                    nroOrden: 1002,
//                    estado: estadoCerrada,
//                    estacion: estacionesExistentes[1],
//                    empleado: empJuan
//                );

//                // No entra: es de otra persona
//                var o3 = new OrdenDeInspeccion(
//                    fechaHoraInicio: DateTime.Now.AddHours(-15),
//                    nroOrden: 1003,
//                    estado: estadoRealizada,
//                    estacion: estacionesExistentes[2],
//                    empleado: empMaria
//                );

//                // No entra: aún Pendiente
//                var o4 = new OrdenDeInspeccion(
//                    fechaHoraInicio: DateTime.Now.AddHours(-5),
//                    nroOrden: 1004,
//                    estado: estadoPendiente,
//                    estacion: estacionesExistentes[0],
//                    empleado: empJuan
//                );

//                ordenes.AddRange(new[] { o1, o2, o3, o4 });
//                await context.OrdenesDeInspeccion.AddRangeAsync(ordenes);
//                await context.SaveChangesAsync();
//            }

//            // Motivos
//            if (!await context.MotivosTipo.AnyAsync())
//            {
//                var motivos = new List<MotivoTipo>
//                {
//                    new MotivoTipo("1","Falla eléctrica"),
//                    new MotivoTipo("2","Sin conectividad"),
//                    new MotivoTipo("3","Mantenimiento programado")
//                };
//                await context.MotivosTipo.AddRangeAsync(motivos);
//                await context.SaveChangesAsync();
//            }

//            Console.WriteLine("Seed ejecutado correctamente. Órdenes pobladas: ");
//            Console.WriteLine("- 1001 (Completada, Juan) → ENTRA EN FILTRO");
//            Console.WriteLine("- 1002 (Cerrada, Juan) → NO ENTRA");
//            Console.WriteLine("- 1003 (Completada, Sol) → NO ENTRA");
//            Console.WriteLine("- 1004 (Pendiente, Juan) → NO ENTRA");
//        }
//    }
//}


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
            try
            {
                // SOLUCIÓN DEFINITIVA: Eliminar y recrear la base de datos
                Console.WriteLine("Eliminando base de datos existente...");
                await context.Database.EnsureDeletedAsync();
                
                Console.WriteLine("Creando base de datos limpia...");
                await context.Database.EnsureCreatedAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] No se pudo recrear la base de datos: {ex.Message}");
                throw;
            }

            Console.WriteLine("Poblando datos iniciales...");

            // ==================== ESTADOS ====================
            var estados = new List<Estado>
            {
                new Estado("OrdenInspeccion", "Pendiente"),
                new Estado("OrdenInspeccion", "Cerrada"),
                new Estado("OrdenInspeccion", "Completada"),
                new Estado("Sismografo", "Operativo"),
                new Estado("Sismografo", "FueraDeServicio")
            };
            await context.Estados.AddRangeAsync(estados);
            await context.SaveChangesAsync();

            // ==================== ROLES ====================
            var rolResponsableReparacion = new Rol("ResponsableReparacion", "Encargado de coordinar reparaciones");
            var rolInspector = new Rol("Inspector", "Empleado que realiza inspecciones");
            
            await context.Roles.AddRangeAsync(rolResponsableReparacion, rolInspector);
            await context.SaveChangesAsync();

            // ==================== EMPLEADOS ====================
            var empJuan = new Empleado("Juan", "Pérez", "ikermavi2015@gmail.com", "123456789");
            var empSol = new Empleado("Sol", "Vega", "sol.vega@empresa.com", "987654321");
            var empMarcos = new Empleado("Marcos", "Pomenich", "marcos.pomenich@empresa.com", "5551234");
            var empCarla = new Empleado("Carla", "Rodríguez", "carla.rodriguez@empresa.com", "5559876");
            var empLuis = new Empleado("Luis", "Fernández", "luis.fernandez@empresa.com", "5555555");

            // Asignar roles
            empJuan.Roles.Add(rolInspector); // Inspector
            empSol.Roles.Add(rolInspector); // Inspector
            empMarcos.Roles.Add(rolResponsableReparacion); // Responsable de Reparación
            empCarla.Roles.Add(rolResponsableReparacion); // Responsable de Reparación
            empLuis.Roles.Add(rolResponsableReparacion); // Responsable de Reparación

            await context.Empleados.AddRangeAsync(empJuan, empSol, empMarcos, empCarla, empLuis);
            await context.SaveChangesAsync();

            // ==================== ESTACIONES Y SISMÓGRAFOS ====================
            var estadoOperativo = await context.Estados.FirstAsync(e => e.Ambito == "Sismografo" && e.NombreEstado == "Operativo");

            var estaciones = new List<EstacionSismologica>();
            for (int i = 1; i <= 6; i++) // Aumentado a 6 estaciones
            {
                var estacion = new EstacionSismologica(
                    codigoEstacion: $"EST-00{i}",
                    nombre: $"Estación {i}",
                    latitud: -34.60 - i * 0.01,
                    longitud: -58.38 - i * 0.01,
                    documentoCertificacionAdquirida: $"DOC-00{i}",
                    nroCertificacionAdquisicion: $"CERT-00{i}",
                    fechaSolicitudCertificacion: DateTime.Now.AddYears(-1).AddMonths(i)
                );

                var sismografo = new Sismografo(
                    identificacionSismografo: $"SISMO-00{i}",
                    nroSerie: $"SN-00{i}",
                    fechaAdquisicion: DateTime.Now.AddYears(-3).AddMonths(i),
                    estadoActual: estadoOperativo
                );

                estacion.Sismografos.Add(sismografo);
                estaciones.Add(estacion);
            }

            await context.EstacionesSismologicas.AddRangeAsync(estaciones);
            await context.SaveChangesAsync();

            // ==================== ORDENES ====================
            var estadoPendiente = await context.Estados.FirstAsync(e => e.Ambito == "OrdenInspeccion" && e.NombreEstado == "Pendiente");
            var estadoCerrada = await context.Estados.FirstAsync(e => e.Ambito == "OrdenInspeccion" && e.NombreEstado == "Cerrada");
            var estadoCompletada = await context.Estados.FirstAsync(e => e.Ambito == "OrdenInspeccion" && e.NombreEstado == "Completada");

            var ordenes = new List<OrdenDeInspeccion>
            {
                // ========== ÓRDENES DE JUAN (ikermavi2015@gmail.com) ==========
                // Completadas (cerrables)
                new OrdenDeInspeccion(DateTime.Now.AddHours(-10), 1001, estadoCompletada, estaciones[0], empJuan),
                new OrdenDeInspeccion(DateTime.Now.AddHours(-8), 1005, estadoCompletada, estaciones[1], empJuan),
                new OrdenDeInspeccion(DateTime.Now.AddHours(-6), 1006, estadoCompletada, estaciones[3], empJuan),
                new OrdenDeInspeccion(DateTime.Now.AddHours(-4), 1007, estadoCompletada, estaciones[4], empJuan),
                
                // Cerradas (no cerrables)
                new OrdenDeInspeccion(DateTime.Now.AddHours(-20), 1002, estadoCerrada, estaciones[1], empJuan),
                
                // Pendientes (no cerrables)
                new OrdenDeInspeccion(DateTime.Now.AddHours(-5), 1004, estadoPendiente, estaciones[0], empJuan),
                new OrdenDeInspeccion(DateTime.Now.AddHours(-3), 1008, estadoPendiente, estaciones[5], empJuan),
                
                // ========== ÓRDENES DE SOL (otras personas) ==========
                new OrdenDeInspeccion(DateTime.Now.AddHours(-15), 1003, estadoCompletada, estaciones[2], empSol),
                new OrdenDeInspeccion(DateTime.Now.AddHours(-12), 1009, estadoCompletada, estaciones[4], empSol)
            };

            await context.OrdenesDeInspeccion.AddRangeAsync(ordenes);
            await context.SaveChangesAsync();

            // ==================== MOTIVOS ====================
            var motivos = new List<MotivoTipo>
            {
                new MotivoTipo("1", "Falla eléctrica"),
                new MotivoTipo("2", "Sin conectividad"),
                new MotivoTipo("3", "Mantenimiento programado"),
                new MotivoTipo("4", "Calibración requerida"),
                new MotivoTipo("5", "Daño estructural")
            };
            await context.MotivosTipo.AddRangeAsync(motivos);
            await context.SaveChangesAsync();

            Console.WriteLine("\n════════════════════════════════════════════════════════");
            Console.WriteLine("✅ SEED EJECUTADO CORRECTAMENTE");
            Console.WriteLine("════════════════════════════════════════════════════════");
            
            Console.WriteLine("\n📋 ÓRDENES DE JUAN (ikermavi2015@gmail.com):");
            Console.WriteLine("   ✓ CERRABLES (Completadas):");
            Console.WriteLine("     - 1001 (Estación 1) ← CERRABLE");
            Console.WriteLine("     - 1005 (Estación 2) ← CERRABLE");
            Console.WriteLine("     - 1006 (Estación 4) ← CERRABLE");
            Console.WriteLine("     - 1007 (Estación 5) ← CERRABLE");
            Console.WriteLine("   ✗ NO CERRABLES:");
            Console.WriteLine("     - 1002 (Cerrada)");
            Console.WriteLine("     - 1004 (Pendiente)");
            Console.WriteLine("     - 1008 (Pendiente)");
            
            Console.WriteLine("\n📋 ÓRDENES DE SOL:");
            Console.WriteLine("     - 1003 (Completada)");
            Console.WriteLine("     - 1009 (Completada)");
            
            Console.WriteLine("\n👥 EMPLEADOS RESPONSABLES DE REPARACIÓN:");
            Console.WriteLine("     - Marcos Pomenich (marcos.pomenich@empresa.com)");
            Console.WriteLine("     - Carla Rodríguez (carla.rodriguez@empresa.com)");
            Console.WriteLine("     - Luis Fernández (luis.fernandez@empresa.com)");
            
            Console.WriteLine("\n🏢 ESTACIONES Y SISMÓGRAFOS:");
            Console.WriteLine("     - 6 estaciones creadas (EST-001 a EST-006)");
            Console.WriteLine("     - 6 sismógrafos operativos (SISMO-001 a SISMO-006)");
            
            Console.WriteLine("\n📝 MOTIVOS DISPONIBLES:");
            Console.WriteLine("     - 5 tipos de motivos cargados");
            Console.WriteLine("════════════════════════════════════════════════════════\n");
        }
    }
}




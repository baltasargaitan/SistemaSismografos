using Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructura.Config
{
    public class OrdenDeInspeccionConfig : IEntityTypeConfiguration<OrdenDeInspeccion>
    {
        public void Configure(EntityTypeBuilder<OrdenDeInspeccion> builder)
        {
            builder.ToTable("OrdenesDeInspeccion");

            // 🔑 Clave principal
            builder.HasKey(o => o.NroOrden);

            builder.Property(o => o.NroOrden)
                   .ValueGeneratedNever();

            builder.Property(o => o.FechaHoraInicio)
                   .IsRequired();

            builder.Property(o => o.FechaHoraFinalizacion);
            builder.Property(o => o.FechaHoraCierre);
            builder.Property(o => o.ObservacionCierre)
                   .HasMaxLength(500);

            // 🔗 Relación con Estado (entidad con tabla propia y clave compuesta)
            builder.HasOne(o => o.Estado)
                   .WithMany() // sin navegación inversa
                   .HasForeignKey("Ambito", "NombreEstado") // columnas FK en OrdenesDeInspeccion
                   .HasPrincipalKey(e => new { e.Ambito, e.NombreEstado })
                   .OnDelete(DeleteBehavior.Restrict);

            // 🔗 Relación con Estacion
            builder.HasOne(o => o.Estacion)
                   .WithMany()
                   .HasForeignKey("CodigoEstacion")
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

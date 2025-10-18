using Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructura.Config
{
    public class CambioEstadoConfig : IEntityTypeConfiguration<CambioEstado>
    {
        public void Configure(EntityTypeBuilder<CambioEstado> builder)
        {
            builder.ToTable("CambioEstados");

            // 🔹 Propiedades
            builder.Property(c => c.FechaHoraInicio)
                   .IsRequired();

            builder.Property(c => c.FechaHoraFin);

            // 🔹 Propiedades sombra (no están en la clase de dominio)
            builder.Property<string>("Ambito")
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property<string>("NombreEstado")
                   .HasMaxLength(50)
                   .IsRequired();

            // ✅ Clave compuesta usando nombres de propiedades sombra
            builder.HasKey("FechaHoraInicio", "Ambito", "NombreEstado");

            // 🔹 Relación con Estado (por clave compuesta)
            builder.HasOne(c => c.Estado)
                   .WithMany()
                   .HasForeignKey("Ambito", "NombreEstado")
                   .OnDelete(DeleteBehavior.Restrict);

            // 🔹 Relación con Motivos fuera de servicio
            builder.HasMany(c => c.MotivosFueraServicio)
                   .WithOne()
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

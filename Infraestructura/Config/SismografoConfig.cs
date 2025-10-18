using Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructura.Config
{
    public class SismografoConfig : IEntityTypeConfiguration<Sismografo>
    {
        public void Configure(EntityTypeBuilder<Sismografo> builder)
        {
            builder.ToTable("Sismografos");

            // 🔑 Clave primaria (no usás Ids, así que usamos Identificación como clave)
            builder.HasKey(s => s.IdentificacionSismografo);

            // 🧱 Propiedades básicas
            builder.Property(s => s.IdentificacionSismografo)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(s => s.NroSerie)
                   .HasMaxLength(100);

            builder.Property(s => s.FechaAdquisicion)
                   .IsRequired();

            // 🔗 Relación con Estado (entidad con tabla propia y clave compuesta)
            builder.HasOne(s => s.EstadoActual)
                   .WithMany() // No hay navegación inversa
                   .HasForeignKey("Ambito", "NombreEstado") // columnas FK en Sismografos
                   .HasPrincipalKey(e => new { e.Ambito, e.NombreEstado }) // clave compuesta en Estados
                   .OnDelete(DeleteBehavior.Restrict); // evita borrado en cascada

            // 📜 Relación con los cambios de estado (si los hay)
            builder.HasMany(s => s.CambiosEstado)
                   .WithOne()
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

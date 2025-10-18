using Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructura.Config
{
    public class EstadoConfig : IEntityTypeConfiguration<Estado>
    {
        public void Configure(EntityTypeBuilder<Estado> builder)
        {
            builder.ToTable("Estados");

            // 🔑 Clave compuesta, ya que no hay Id numérico
            builder.HasKey(e => new { e.Ambito, e.NombreEstado });

            // 🔹 Propiedades
            builder.Property(e => e.Ambito)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(e => e.NombreEstado)
                   .IsRequired()
                   .HasMaxLength(50);

        }
    }
}

using Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructura.Config
{
    public class SesionConfig : IEntityTypeConfiguration<Sesion>
    {
        public void Configure(EntityTypeBuilder<Sesion> builder)
        {
            builder.ToTable("Sesiones");

            builder.HasKey(s => s.FechaHoraDesde);

            builder.Property(s => s.FechaHoraDesde).IsRequired();
            builder.Property(s => s.FechaHoraHasta);

            builder.HasOne(s => s.Usuario)
                   .WithMany()
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

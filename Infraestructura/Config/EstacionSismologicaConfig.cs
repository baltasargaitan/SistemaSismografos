using Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructura.Config
{
    public class EstacionSismologicaConfig : IEntityTypeConfiguration<EstacionSismologica>
    {
        public void Configure(EntityTypeBuilder<EstacionSismologica> builder)
        {
            builder.ToTable("EstacionesSismologica");

            builder.HasKey(e => e.CodigoEstacion);

            builder.Property(e => e.CodigoEstacion).HasMaxLength(50);
            builder.Property(e => e.Nombre).HasMaxLength(200);
            builder.Property(e => e.DocumentoCertificacionAdquirida).HasMaxLength(200);
            builder.Property(e => e.NroCertificacionAdquisicion).HasMaxLength(100);
            builder.Property(e => e.FechaSolicitudCertificacion).IsRequired();

            builder.HasMany(e => e.Sismografos)
                   .WithOne()
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

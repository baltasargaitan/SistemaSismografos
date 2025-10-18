using Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructura.Config
{
    public class MotivoTipoConfig : IEntityTypeConfiguration<MotivoTipo>
    {
        public void Configure(EntityTypeBuilder<MotivoTipo> builder)
        {
            builder.ToTable("MotivosTipo");

            builder.HasKey(m => m.TipoMotivo);

            builder.Property(m => m.TipoMotivo).HasMaxLength(50);
            builder.Property(m => m.Descripcion).HasMaxLength(200);
        }
    }
}

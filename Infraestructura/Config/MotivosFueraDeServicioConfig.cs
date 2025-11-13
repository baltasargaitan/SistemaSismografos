using Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infraestructura.Config
{
    public class MotivoFueraServicioConfig : IEntityTypeConfiguration<MotivoFueraServicio>
    {
        public void Configure(EntityTypeBuilder<MotivoFueraServicio> builder)
        {
            builder.ToTable("MotivosFueraServicio");

            // ✅ Clave primaria: ID auto-incremental generado por EF (propiedad sombra)
            // No viola OOP porque el dominio NO conoce este ID, es solo para persistencia
            builder.Property<int>("Id");
            builder.HasKey("Id");

            // Mapeo del comentario
            builder.Property("Comentario")
                .IsRequired()
                .HasMaxLength(500);

            // Conversión personalizada del campo 'tipo'
            var tipoConverter = new ValueConverter<MotivoTipo, string>(
                v => v != null ? v.TipoMotivo : null, // al guardar en la BD
                v => new MotivoTipo(v, string.Empty)   // al leer desde la BD
            );

            builder.Property("Tipo")
                .HasConversion(tipoConverter)
                .HasColumnName("TipoMotivo")
                .HasMaxLength(50)
                .IsRequired();
        }
    }
}

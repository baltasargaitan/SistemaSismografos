using Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructura.Config
{
    public class EmpleadoConfig : IEntityTypeConfiguration<Empleado>
    {
        public void Configure(EntityTypeBuilder<Empleado> builder)
        {
            builder.ToTable("Empleados");

            builder.HasKey(e => e.Mail);

            builder.Property(e => e.Mail)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(e => e.Nombre)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(e => e.Apellido)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(e => e.Telefono)
                   .HasMaxLength(20);

            builder.HasMany(e => e.Roles)
                   .WithMany();
        }
    }
}

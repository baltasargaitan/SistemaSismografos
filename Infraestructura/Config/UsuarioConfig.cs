using Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infraestructura.Config
{
    public class UsuarioConfig : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuarios");

            builder.HasKey(u => u.NombreUsuario);

            builder.Property(u => u.NombreUsuario).HasMaxLength(100);
            builder.Property(u => u.Contraseña).HasMaxLength(100);

            builder.HasOne(u => u.Empleado)
                   .WithMany()
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

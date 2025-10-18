using Dominio.Entidades;
using Dominio.Repositorios;
using Infraestructura.Persistencia;

namespace Infraestructura.Repositorios
{
    public class RepositorioMotivoFueraServicio : RepositorioBase<MotivoFueraServicio>, IRepositorioMotivoFueraServicio
    {
        public RepositorioMotivoFueraServicio(AppDbContext context) : base(context) { }
    }
}

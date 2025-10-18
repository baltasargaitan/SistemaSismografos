namespace Dominio.Entidades
{
    public class Sesion
    {
        public DateTime FechaHoraDesde { get; private set; }
        public DateTime? FechaHoraHasta { get; private set; }
        public Usuario Usuario { get; private set; }

        public Sesion(Usuario usuario)
        {
            Usuario = usuario;
            FechaHoraDesde = DateTime.Now;
        }

        public Usuario ObtenerUsuario() => Usuario;
        public bool EsVigente() => !FechaHoraHasta.HasValue;

        protected Sesion() { }
    }
}

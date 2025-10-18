namespace Dominio.Entidades
{
    public class MotivoTipo
    {
        public string TipoMotivo { get; private set; }
        public string Descripcion { get; private set; }

        public MotivoTipo(string tipoMotivo, string descripcion)
        {
            TipoMotivo = tipoMotivo;
            Descripcion = descripcion;
        }

        public string GetDescripcion() => Descripcion;
    }
}
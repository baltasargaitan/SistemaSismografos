namespace Dominio.Entidades
{
    public class Rol
    {
        public string Nombre { get; private set; } = string.Empty;
        public string Descripcion { get; private set; } = string.Empty;

        public Rol(string nombre, string descripcion)
        {
            Nombre = nombre;
            Descripcion = descripcion;
        }

        private Rol() { }
        public string GetNombre() => Nombre;
    }
}

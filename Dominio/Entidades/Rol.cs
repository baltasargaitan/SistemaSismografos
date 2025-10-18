namespace Dominio.Entidades
{
    public class Rol
    {

       

        public string Nombre { get; private set; }
        public string Descripcion { get; private set; }


        public Rol(string nombre, string descripcion)
        {
            Nombre = nombre;
            Descripcion = descripcion;
        }

        private Rol() { }
        public string GetNombre() => Nombre;

    }
}

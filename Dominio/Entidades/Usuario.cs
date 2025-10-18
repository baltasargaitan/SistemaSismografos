namespace Dominio.Entidades
{
    public class Usuario
    {
        public string NombreUsuario { get; private set; }
        public string Contraseña { get; private set; }
        public Empleado Empleado { get; private set; }

        public Usuario(string nombreUsuario, string contraseña, Empleado empleado = null)
        {
            NombreUsuario = nombreUsuario;
            Contraseña = contraseña;
            Empleado = empleado;
        }

        public Empleado GetRILogueado() => Empleado;
        protected Usuario() { }
    }
}

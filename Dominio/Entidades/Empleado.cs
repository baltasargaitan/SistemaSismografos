using System.Collections.Generic;
using System.Linq;

namespace Dominio.Entidades
{
    public class Empleado
    {
        public string Mail { get; private set; }
        public string Nombre { get; private set; }
        public string Apellido { get; private set; }
        public string Telefono { get; private set; }
        public List<Rol> Roles { get; private set; } = new();

        public Empleado(string nombre, string apellido, string mail, string telefono)
        {
            Nombre = nombre;
            Apellido = apellido;
            Mail = mail;
            Telefono = telefono;
        }

        private Empleado() { }

        public bool EsResponsableDeReparacion() =>
            Roles.Any(r => r.GetNombre() == "ResponsableReparacion");

        public string ObtenerMail() => Mail;
        public string GetNombreCompleto() => $"{Nombre} {Apellido}";
    }
}

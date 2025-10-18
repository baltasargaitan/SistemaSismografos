namespace Dominio.Entidades
{
    public class EstacionSismologica
    {
        public string CodigoEstacion { get; private set; }
        public string DocumentoCertificacionAdquirida { get; private set; }
        public DateTime FechaSolicitudCertificacion { get; private set; }
        public double Latitud { get; private set; }
        public double Longitud { get; private set; }
        public string Nombre { get; private set; }
        public string NroCertificacionAdquisicion { get; private set; }
        public List<Sismografo> Sismografos { get; private set; } = new();

        public EstacionSismologica(
            string codigoEstacion,
            string documentoCertificacionAdquirida,
            DateTime fechaSolicitudCertificacion,
            double latitud,
            double longitud,
            string nombre,
            string nroCertificacionAdquisicion)
        {
            CodigoEstacion = codigoEstacion;
            DocumentoCertificacionAdquirida = documentoCertificacionAdquirida;
            FechaSolicitudCertificacion = fechaSolicitudCertificacion;
            Latitud = latitud;
            Longitud = longitud;
            Nombre = nombre;
            NroCertificacionAdquisicion = nroCertificacionAdquisicion;
        }

        public string GetCodigoEstacion() => CodigoEstacion;
        public string GetNombre() => Nombre;

        public Sismografo ObtenerIdSismografo() => Sismografos.FirstOrDefault();
        public void PonerSismografoFueraDeServicio()
        {
            var sismografo = ObtenerIdSismografo();
            sismografo?.SetEstadoActual(new Estado("Sismografo", "FueraDeServicio"));
        }
    }
}
namespace Dominio.Entidades
{
    public class Sismografo
    {
        public DateTime FechaAdquisicion { get; private set; }
        public string IdentificacionSismografo { get; private set; }
        public string NroSerie { get; private set; }
        public Estado EstadoActual { get; private set; }
        public List<CambioEstado> CambiosEstado { get; private set; } = new();  //navegabilidad

        public Sismografo(
            DateTime fechaAdquisicion,
            string identificacionSismografo,
            string nroSerie,
            Estado estadoActual)
        {
            FechaAdquisicion = fechaAdquisicion;
            IdentificacionSismografo = identificacionSismografo;
            NroSerie = nroSerie;
            EstadoActual = estadoActual;
        }

        public string GetIdentificadorSismografo() => IdentificacionSismografo;
        public Estado ObtenerEstadoActual() => EstadoActual;


        public CambioEstado CrearCambioEstado(Estado nuevoEstado)
        {
            var cambio = new CambioEstado(nuevoEstado);
            CambiosEstado.Add(cambio);
            return cambio;
        }

        private Sismografo() { }

        public void SetEstadoActual(Estado estado) => EstadoActual = estado;
        public void EnviarAReparar() => SetEstadoActual(new Estado("Sismografo", "EnReparacion"));
        public bool SosDeEstacionSismologica(EstacionSismologica est) => true;
    }
}
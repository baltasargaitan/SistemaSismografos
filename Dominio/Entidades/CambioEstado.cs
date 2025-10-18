namespace Dominio.Entidades
{
    public class CambioEstado
    {
        [Obsolete("Solo para EF", true)]
        protected CambioEstado() { }


        public DateTime FechaHoraInicio { get; private set; }
        public DateTime? FechaHoraFin { get; private set; }
        public Estado Estado { get; private set; }
        public List<MotivoFueraServicio> MotivosFueraServicio { get; private set; } = new();

        public CambioEstado(Estado estado)
        {
            Estado = estado;
            FechaHoraInicio = DateTime.Now;
        }

        public bool EsEstadoActual() => !FechaHoraFin.HasValue;
        public void SetFechaHoraFin() => FechaHoraFin = DateTime.Now;
        public void CrearMotivosFueraDeServicio(MotivoFueraServicio motivo) => MotivosFueraServicio.Add(motivo);
    }
}

namespace Dominio.Entidades
{
    public class OrdenDeInspeccion
    {
        public DateTime FechaHoraInicio { get; private set; }
        public DateTime? FechaHoraFinalizacion { get; private set; }
        public DateTime? FechaHoraCierre { get; private set; }
        public int NroOrden { get; private set; }
        public string? ObservacionCierre { get; private set; }
        public Estado? Estado { get; private set; }
        public EstacionSismologica? Estacion { get; private set; }

        public Empleado Empleado { get; private set; }



        public OrdenDeInspeccion(
            DateTime fechaHoraInicio,
            int nroOrden,
            Estado estado,
            EstacionSismologica estacion,
            Empleado empleado)
        {
            FechaHoraInicio = fechaHoraInicio;
            NroOrden = nroOrden;
            Estado = estado;
            Estacion = estacion;
            Empleado = empleado;
        }

        public bool EsDeEmpleado(Empleado emp)
        {
            if (Empleado == null || emp == null)
                return false;

            // Compara por mail, identificador único elegido y propuesto por SOL 
            return Empleado.ObtenerMail() == emp.ObtenerMail();
        }

        public bool EstaCompletamenteRealizada()
        {
            return Estado != null && Estado.EsCompletamenteRealizada();
        }
        public int GetNroOrden() => NroOrden;
        public DateTime GetFechaHoraInicio() => FechaHoraInicio;
        public DateTime? GetFechaFinalizacion() => FechaHoraFinalizacion;
        public EstacionSismologica GetEstacion() => Estacion;
        public Estado GetEstado() => Estado;
        public string GetObservacion() => ObservacionCierre;

        public void SetFechaHoraCierre() => FechaHoraCierre = DateTime.Now;
        public void SetObservacion(string obs) => ObservacionCierre = obs;
        public void SetEstado(Estado estado) => Estado = estado;

        private OrdenDeInspeccion() { }
        //public void Cerrar()
        //{
        //    SetFechaHoraCierre();
        //    SetEstado(new Estado("OrdenInspeccion", "Cerrada"));
        //}

        public void Cerrar(string observacion, Estado estadoCerrado)
        {
            if (string.IsNullOrWhiteSpace(observacion))
                throw new InvalidOperationException("Observación requerida");

            if (Estado?.EsCerrada() == true)
                throw new InvalidOperationException("Orden ya cerrada");

            ObservacionCierre = observacion;
            FechaHoraCierre = DateTime.Now;
            Estado = estadoCerrado;
        }




        public void EnviarSismografoParaReparacion() => Estacion.ObtenerIdSismografo()?.EnviarAReparar();
    }
}
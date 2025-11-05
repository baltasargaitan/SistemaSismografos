//namespace Dominio.Entidades
//{
//    public class CambioEstado
//    {
//        [Obsolete("Solo para EF", true)]
//        protected CambioEstado() { }


//        public DateTime FechaHoraInicio { get; private set; }
//        public DateTime? FechaHoraFin { get; private set; }
//        public Estado Estado { get; private set; }
//        public List<MotivoFueraServicio> MotivosFueraServicio { get; private set; } = new();

//        public CambioEstado(Estado estado)
//        {
//            Estado = estado;
//            FechaHoraInicio = DateTime.Now;
//        }

//        public bool EsEstadoActual() => !FechaHoraFin.HasValue;
//        public void SetFechaHoraFin() => FechaHoraFin = DateTime.Now;



//        //revisar metodo
//        public void CrearMotivosFueraDeServicio(MotivoFueraServicio motivo) => MotivosFueraServicio.Add(motivo);
//    }
//}


using System;
using System.Collections.Generic;

namespace Dominio.Entidades
{
    public class CambioEstado
    {
        [Obsolete("Solo para EF", true)]
        protected CambioEstado() { }

        public DateTime FechaHoraInicio { get; private set; }
        public DateTime? FechaHoraFin { get; private set; }
        public Estado Estado { get; private set; }

        // Agregación: un cambio puede tener varios motivos
        private readonly List<MotivoFueraServicio> _motivosFueraServicio = new();
        public IReadOnlyCollection<MotivoFueraServicio> MotivosFueraServicio => _motivosFueraServicio.AsReadOnly();

        public CambioEstado(Estado estado)
        {
            Estado = estado ?? throw new ArgumentNullException(nameof(estado));
            FechaHoraInicio = DateTime.Now;
        }

        public bool EsEstadoActual() => !FechaHoraFin.HasValue;
        public void SetFechaHoraFin() => FechaHoraFin = DateTime.Now;

        // Agregar un motivo individual
        public void CrearMotivosFueraDeServicio(MotivoFueraServicio motivo)
        {
            if (motivo == null) throw new ArgumentNullException(nameof(motivo));
            _motivosFueraServicio.Add(motivo);
        }

        // Agregar varios motivos (para el caso de cierre múltiple)
        public void CrearMotivosFueraDeServicio(IEnumerable<MotivoFueraServicio> motivos)
        {
            if (motivos == null) return;
            foreach (var m in motivos)
                if (m != null)
                    _motivosFueraServicio.Add(m);
        }
    }
}

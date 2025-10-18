using Aplicacion.DTOs;
using System.Collections.Generic;

namespace Aplicacion.Interfaces
{
    public interface IPantallaCierreInspeccion
    {
        void HabilitarVentana();
        void MostrarSeleccionOrdenes(IEnumerable<OrdenResumenDTO> ordenes);
        int TomarSeleccionOrden(); // retorna nroOrden seleccionado por el usuario
        string PedirObservacion(); // texto ingresado por usuario
        void MostrarMotivosFueraDeServicioSeleccionables(IEnumerable<string> motivos);
        string TomarSeleccionMotivoFueraDeServicio(); // devuelve tipoMotivo seleccionado
        string SolicitarIngresoComentario(); // comentario ingresado por usuario
        bool SolicitarConfirmacionCierreOrden(); // true si confirma
        void MostrarMensaje(string mensaje);
    }
}

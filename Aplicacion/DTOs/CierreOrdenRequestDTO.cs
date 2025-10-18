namespace Aplicacion.DTOs
{
    public class CierreOrdenRequest
    {
        public int NroOrden { get; set; }
        public string Observacion { get; set; }
        public string MotivoTipo { get; set; }
        public string Comentario { get; set; }
        public bool Confirmar { get; set; }
    }
}

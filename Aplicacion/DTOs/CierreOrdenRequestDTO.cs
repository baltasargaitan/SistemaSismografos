namespace Aplicacion.DTOs
{
    public class CierreOrdenRequest
    {
        public int NroOrden { get; set; }
        public string Observacion { get; set; }
        public List<string> MotivosTipo { get; set; } = new();
        public List<string> Comentarios { get; set; } = new();
        public bool Confirmar { get; set; }
    }

}

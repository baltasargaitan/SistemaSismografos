namespace Frontend.DTOs
{
    public class OrdenDTO
    {
        public int nroOrden { get; set; }
        public string estacion { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public DateTime fechaInicio { get; set; }
    }
}

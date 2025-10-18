namespace Dominio.Entidades
{
    public class Estado
    {
        public string Ambito { get; private set; }
        public string NombreEstado { get; private set; }

        public Estado(string ambito, string nombreEstado)
        {
            Ambito = ambito;
            NombreEstado = nombreEstado;
        }

        public bool EsCompletamenteRealizada() => NombreEstado == "Completada";
        public bool EsAmbitoOrdenInspeccion() => Ambito == "OrdenInspeccion";
        public bool EsCerrada() => NombreEstado == "Cerrada";
        public bool EsAmbitoSismografico() => Ambito == "Sismografo";
        public bool EsFueraDeServicio() => NombreEstado == "FueraDeServicio";
    }
}

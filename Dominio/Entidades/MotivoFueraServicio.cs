using System;

namespace Dominio.Entidades
{
    public class MotivoFueraServicio
    {
        public MotivoTipo Tipo { get; private set; }
        public string Comentario { get; private set; }

        [Obsolete("Solo para EF", true)]
        protected MotivoFueraServicio() { }

        public MotivoFueraServicio(MotivoTipo tipo, string comentario)
        {
            Tipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
            Comentario = comentario ?? string.Empty;
        }
    }
}

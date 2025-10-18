using System;

namespace Dominio.Entidades
{
    public class MotivoFueraServicio
    {
        public string Comentario { get; private set; }
        public MotivoTipo Tipo { get; private set; }

        public MotivoFueraServicio(MotivoTipo tipo, string comentario)
        {
            Tipo = tipo;
            Comentario = comentario;
        }
    }
}

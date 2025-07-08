using Microsoft.AspNetCore.Http.HttpResults;

namespace BancoDigitalAna.Core.Entities
{
    public class Movimento
    {
        public int IdMovimento { get; set; }
        /// <summary>
        /// Este campo será preenchido automaticamente pelo banco de dados
        /// </summary>
        public DateTime Datamovimento { get; set; }
        public string? TipoMovimento { get; set; }
        public float Valor { get; set; }
        public int IdContaCorrente { get; set; }
    }
}

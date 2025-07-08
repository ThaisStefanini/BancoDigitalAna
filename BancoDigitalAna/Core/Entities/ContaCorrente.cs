using Microsoft.AspNetCore.Http.HttpResults;

namespace BancoDigitalAna.Core.Entities
{
    public class ContaCorrente
    {
        public int IdContaCorrente { get; set; }
        public int Numero { get; set; }
        public string? Nome { get; set; }
        public string? Senha { get; set; }
        public int Ativo { get; set; }
        public string? Salt { get; set; }
    }
}

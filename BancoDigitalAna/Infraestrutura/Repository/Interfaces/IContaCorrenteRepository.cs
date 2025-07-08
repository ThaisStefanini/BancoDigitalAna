using BancoDigitalAna.Core.Entities;
using Microsoft.Data.Sqlite;

namespace BancoDigitalAna.Infraestrutura.Repository.Interfaces
{
    public interface IContaCorrenteRepository
    {
        public Task<int> AdicionarConta(ContaCorrente conta);
        public Task<ContaCorrente?> ObterConta(int numero, string nome);
        public Task<ContaCorrente?> ObterContaPorId(int idcontacorrente);
        public Task<int> ObterProximoNumeroContaAsync();
        public Task<bool> DesativarConta(int idcontacorrente);
    }
}

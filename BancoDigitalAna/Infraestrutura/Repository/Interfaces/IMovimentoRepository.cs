using BancoDigitalAna.Core.Entities;
using Microsoft.Data.Sqlite;

namespace BancoDigitalAna.Infraestrutura.Repository.Interfaces
{
    public interface IMovimentoRepository
    {
        public Task<int> AdicionarMovimento(Movimento movimento);
        public Task<IEnumerable<Movimento>> MovimentosPorConta(int idcontacorrente);
    }
}

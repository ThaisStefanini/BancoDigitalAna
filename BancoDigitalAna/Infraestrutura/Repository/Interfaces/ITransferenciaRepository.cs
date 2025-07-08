using BancoDigitalAna.Core.Entities;
using Microsoft.Data.Sqlite;

namespace BancoDigitalAna.Infraestrutura.Repository.Interfaces
{
    public interface ITransferenciaRepository
    {
        public Task<int> AdicionarTransferencia(Transferencia transferencia);
    }
}

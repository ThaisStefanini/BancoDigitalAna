using BancoDigitalAna.Core.Entities;
using BancoDigitalAna.Infraestrutura.Repository.Base;
using BancoDigitalAna.Infraestrutura.Repository.Interfaces;
using Dapper;
using Microsoft.Data.Sqlite;

namespace BancoDigitalAna.Infraestrutura.Repository
{
    public class TransferenciaRepository : _Repository, ITransferenciaRepository
    {
        public TransferenciaRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<int> AdicionarTransferencia(Transferencia transferencia)
        {
            using var connection = new SqliteConnection(_connectionString);

            // Campo DataMovimento é preenchido automaticamente pelo banco de dados
            var sql = @"INSERT INTO transferencia (idcontacorrente_origem, idcontacorrente_destino, datamovimento, valor) 
                    VALUES (@idOrigemParam, @idDestinoParam, @dataParam, @valorParam);
                    SELECT last_insert_rowid();";
            var parametros = new
            {
                idOrigemParam = transferencia.IdContaCorrente_Origem,
                idDestinoParam = transferencia.IdContaCorrente_Destino,
                dataParam = DateTime.Now,
                valorParam = transferencia.Valor
            };

            return await connection.ExecuteScalarAsync<int>(sql, parametros);
        }
    }
}

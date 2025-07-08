using BancoDigitalAna.Core.Entities;
using BancoDigitalAna.Infraestrutura.Repository.Base;
using BancoDigitalAna.Infraestrutura.Repository.Interfaces;
using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.Sqlite;
using static System.Net.Mime.MediaTypeNames;

namespace BancoDigitalAna.Infraestrutura.Repository
{
    public class MovimentoRepository : _Repository, IMovimentoRepository
    {
        public MovimentoRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<int> AdicionarMovimento(Movimento movimento)
        {
            using var connection = new SqliteConnection(_connectionString);

            // Campo DataMovimento é preenchido automaticamente pelo banco de dados
            var sql = @"INSERT INTO movimento (idcontacorrente, tipomovimento, valor) 
                    VALUES (@idParam, @tipoParam, @valorParam);
                    SELECT last_insert_rowid();";
            var parametros = new
            {
                idParam = movimento.IdContaCorrente,
                tipoParam = movimento.TipoMovimento,
                valorParam = movimento.Valor
            };

            return await connection.ExecuteScalarAsync<int>(sql, parametros);
        }

        public async Task<IEnumerable<Movimento>> MovimentosPorConta(int idcontacorrente)
        {
            using var connection = new SqliteConnection(_connectionString);

            var sql = "SELECT idmovimento, idcontacorrente, datamovimento, tipomovimento, valor " +
                        "FROM movimento WHERE idcontacorrente = @idcontacorrente";

            return await connection.QueryAsync<Movimento>(sql, new { idcontacorrente });
        }

    }
}

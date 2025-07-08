using BancoDigitalAna.Core.Entities;
using BancoDigitalAna.Infraestrutura.Repository.Base;
using BancoDigitalAna.Infraestrutura.Repository.Interfaces;
using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.Sqlite;

namespace BancoDigitalAna.Infraestrutura.Repository
{
    public class ContaCorrenteRepository: _Repository, IContaCorrenteRepository
    {
        public ContaCorrenteRepository(IConfiguration configuration) : base(configuration)
        {
        }

        public async Task<int> AdicionarConta(ContaCorrente conta)
        {
            using var connection = new SqliteConnection(_connectionString);

            var sql = @"INSERT INTO contacorrente (numero, nome, senha, ativo, salt) 
                    VALUES (@numeroParam, @nomeParam, @senhaParam, @ativoParam, @saltParam);
                    SELECT last_insert_rowid();";
            var parametros = new
            {
                numeroParam = conta.Numero,
                nomeParam = conta.Nome,
                senhaParam = conta.Senha,
                ativoParam = conta.Ativo,
                saltParam = conta.Salt
            };

            return await connection.ExecuteScalarAsync<int>(sql, parametros);
        }

        public async Task<ContaCorrente?> ObterConta(int numero, string nome)
        {
            using var connection = new SqliteConnection(_connectionString);

            var sql = @"SELECT idcontacorrente, numero, nome, senha, ativo, salt FROM contacorrente WHERE numero = @numero OR nome = @nome LIMIT 1";

            return await connection.QueryFirstOrDefaultAsync<ContaCorrente>(sql, new { numero, nome });
        }

        public async Task<ContaCorrente?> ObterContaPorId(int idcontacorrente)
        {
            using var connection = new SqliteConnection(_connectionString);

            var sql = @"SELECT idcontacorrente, numero, nome, senha, ativo, salt FROM contacorrente WHERE idcontacorrente = @idcontacorrente LIMIT 1";

            return await connection.QueryFirstOrDefaultAsync<ContaCorrente>(sql, new { idcontacorrente });
        }

        public async Task<int> ObterProximoNumeroContaAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var sql = @"SELECT COALESCE(MAX(numero), 0) + 1 
                    FROM contacorrente";

                var proximoNumero = await connection.ExecuteScalarAsync<int>(sql, transaction: transaction);

                await transaction.CommitAsync();
                return proximoNumero;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DesativarConta(int idcontacorrente)
        {
            using var connection = new SqliteConnection(_connectionString);

            var sql = @"UPDATE contacorrente SET ativo = 0 WHERE idcontacorrente = @idcontacorrente";
            var parametros = new { idcontacorrente };

            return await connection.ExecuteScalarAsync<int>(sql, parametros) == 0;
        }

    }
}

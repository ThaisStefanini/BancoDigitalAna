using BancoDigitalAna.Api.DTO;
using BancoDigitalAna.Core.Entities;
using BancoDigitalAna.Infraestrutura.Repository;
using BancoDigitalAna.Shared.Exceptions;
using System.Net;
using static BancoDigitalAna.Shared.Helpers.Constantes;

namespace BancoDigitalAna.Core.Service
{
    public class TransferenciaService
    {
        private readonly IConfiguration _configuration;

        private readonly TransferenciaRepository _transferenciaRepository;
        private readonly ContaCorrenteRepository _contaCorrenteRepository;

        public TransferenciaService(IConfiguration configuration)
        {
            _configuration = configuration;

            _transferenciaRepository = new TransferenciaRepository(configuration);
            _contaCorrenteRepository = new ContaCorrenteRepository(configuration);
        }

        public async Task<bool> AdicionarTransferencia(int idContaOrigem, TransferenciaInternaRequest request)
        {
            var contaDestino = await ValidaContasOrigemDestino(idContaOrigem, request.NumeroContaDestino);

            // Caso contas válidas, grava transferência
            Transferencia transfer = new Transferencia()
            {
                IdContaCorrente_Origem = idContaOrigem,
                IdContaCorrente_Destino = contaDestino.IdContaCorrente,
                Valor = request.Valor
            };

            transfer.IdTransferencia = await _transferenciaRepository.AdicionarTransferencia(transfer);

            return true;
        }

        private async Task<ContaCorrente> ValidaContasOrigemDestino(int idContaOrigem, int NumeroContaDestino)
        {
            // Valida conta origem
            await ValidaContaPorId(idContaOrigem, "origem");

            //Obtem o ID da conta corrente de Destino a partir do número da conta e valida
            var contaDestino = await _contaCorrenteRepository.ObterConta(NumeroContaDestino, "");
            ValidaConta(contaDestino, "destino");

            return contaDestino;
        }

        private async Task<bool> ValidaContaPorId(int idContaCorrente, string descConta)
        {
            var conta = await _contaCorrenteRepository.ObterContaPorId(idContaCorrente);
            ValidaConta(conta, descConta);

            return true;
        }

        private bool ValidaConta(ContaCorrente? conta, string descConta)
        {
            if (conta == null || conta.Ativo == 0)
            {
                throw new BusinessException(
                    "Conta " + descConta + " inválida",
                    INVALID_ACCOUNT,
                    HttpStatusCode.Forbidden);
            }

            return true;
        }
    }
}

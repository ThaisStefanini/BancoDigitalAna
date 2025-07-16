using BancoDigitalAna.Api.DTO;
using BancoDigitalAna.Core.Entities;
using BancoDigitalAna.Infraestrutura.Repository;
using BancoDigitalAna.Shared.Exceptions;
using CpfCnpjLibrary;
using System.Net;
using static BancoDigitalAna.Shared.Helpers.Constantes;

namespace BancoDigitalAna.Core.Service
{
    public class ContaCorrenteService
    {
        private readonly HasherService _hasher;
        private readonly IConfiguration _configuration;

        private readonly ContaCorrenteRepository _contaCorrenteRepository;
        private readonly MovimentoRepository _movimentoRepository;

        public ContaCorrenteService(IConfiguration configuration)
        {
            _hasher = new HasherService();
            _configuration = configuration;

            _contaCorrenteRepository = new ContaCorrenteRepository(configuration);
            _movimentoRepository = new MovimentoRepository(configuration);
        }

        public async Task<AdicionarContaResponse> AdicionarContaCorrente(AdicionarContaRequest request)
        {
            ContaCorrente conta = await PrepararContaNova(request);

            conta.IdContaCorrente = await _contaCorrenteRepository.AdicionarConta(conta);

            return new AdicionarContaResponse { ContaNumero = conta.Numero };
        }

        private async Task<ContaCorrente> PrepararContaNova(AdicionarContaRequest request)
        {
            var conta = CriarContaBasica(request);
            conta.Numero = await GerarNumeroContaAsync(conta.Numero);
            ValidarConta(conta);
            AplicarHashSenha(conta);
            return conta;
        }

        private ContaCorrente CriarContaBasica(AdicionarContaRequest request)
        {
            return new ContaCorrente
            {
                Nome = request.Cpf,
                Senha = request.Senha,
                Ativo = 1 // Garante que toda conta criada é sempre criada ativa
            };
        }

        private async Task<int> GerarNumeroContaAsync(int numero)
        {
            int retorno = numero;
            if (numero <= 0)
            {
                retorno = await _contaCorrenteRepository.ObterProximoNumeroContaAsync();
            }
            return retorno;
        }

        private void AplicarHashSenha(ContaCorrente conta)
        {
            (conta.Senha, conta.Salt) = _hasher.HashPassword(conta.Senha ?? "");
        }

        private bool ValidarConta(ContaCorrente conta)
        {
            bool retorno = true;

            if (ValidarCpf(conta.Nome))
            {
                conta.Nome = Cpf.FormatarSemPontuacao(conta.Nome);
            }

            return retorno;
        }

        private bool ValidarCpf(string? cpf)
        {
            bool retorno = true;

            if (!Cpf.Validar(cpf))
            {
                // Se CPF inválido, lança uma exceção que será tratada no controlador
                throw new BusinessException(
                    "CPF inválido",
                    INVALID_DOCUMENT,
                    HttpStatusCode.BadRequest);
            }

            return retorno;
        }

        public async Task<bool> InativarContaCorrente(int idContacorrente)
        {
            var desativado = await _contaCorrenteRepository.DesativarConta(idContacorrente);

            if (!desativado)
            {
                throw new BusinessException(
                    "Erro ao desativar conta",
                    INVALID_ACCOUNT,
                    HttpStatusCode.BadRequest);
            }

            return desativado;
        }

        public async Task<bool> MovimentarContaCorrente(int idContaCorrente, MovimentacaoRequest request)
        {
            // Validar se dados da request estão válidos
            ValidaSolicitacaoMovimento(request);

            // Obtem conta válida e ativa se existir
            ContaCorrente? conta = await ObtemContaAtiva(idContaCorrente, request.NumeroConta);

            // Valida se conta pode receber este tipo de movimentação
            if (request.TipoMovimento == TIPOS_MOVIMENTOS.D.ToString())
            {
                // Apenas usuário logado pode executar débitos
                if (conta.IdContaCorrente != idContaCorrente)
                {
                    throw new BusinessException(
                        "Movimentação inválida.",
                        INVALID_TYPE,
                        HttpStatusCode.Forbidden);
                }
            }

            // Caso nenhuma exceção tenha sido lançada, adicionar movimento à tabela
            Movimento movimento = new Movimento()
            {
                TipoMovimento = request.TipoMovimento,
                Valor = request.Valor,
                IdContaCorrente = conta.IdContaCorrente
            };
            var idMovimentoAdicionado = await _movimentoRepository.AdicionarMovimento(movimento);

            if (idMovimentoAdicionado == 0)
            {
                throw new BusinessException(
                    "Erro ao adicionar movimento",
                    INVALID_ACCOUNT,
                    HttpStatusCode.BadRequest);
            }

            return idMovimentoAdicionado > 0;
        }

        private bool ValidaSolicitacaoMovimento(MovimentacaoRequest request)
        {
            if (request.Valor <= 0)
            {
                throw new BusinessException(
                    "Valor inválido, apenas valores positivos são aceitos.",
                    INVALID_VALUE,
                    HttpStatusCode.Forbidden);
            }

            if (Enum.IsDefined(typeof(TIPOS_MOVIMENTOS), request.TipoMovimento.ToUpper()))
            {
                request.TipoMovimento = request.TipoMovimento.ToUpper();
            }
            else
            {
                throw new BusinessException(
                    "Tipo de movimento inválido.",
                    INVALID_TYPE,
                    HttpStatusCode.Forbidden);
            }

            return true;
        }

        private async Task<ContaCorrente> ObtemContaAtiva(int idContaCorrente, int numeroConta)
        {
            ContaCorrente? conta;

            // Obtém conta
            if (numeroConta > 0)
            {
                conta = await _contaCorrenteRepository.ObterConta(numeroConta, "");
            }
            else
            {
                conta = await _contaCorrenteRepository.ObterContaPorId(idContaCorrente);
            }

            // Valida se conta existe e está ativa
            if (conta != null)
            {
                if (conta.Ativo != 1)
                {
                    throw new BusinessException(
                        "Conta inativa",
                        INACTIVE_ACCOUNT,
                        HttpStatusCode.Forbidden);
                }
            }
            else
            {
                throw new BusinessException(
                    "Conta inválida",
                    INVALID_ACCOUNT,
                    HttpStatusCode.Forbidden);
            }

            return conta;
        }

        public async Task<SaldoContaResponse> ObterSaldoContaCorrente(int idContaCorrente)
        {
            // Obtem conta válida e ativa se existir
            ContaCorrente? conta = await ObtemContaAtiva(idContaCorrente, 0);

            // Obtem movimentações
            var movimentos = await _movimentoRepository.MovimentosPorConta(idContaCorrente);

            float saldo = CalculaSaldo(movimentos);

            return new SaldoContaResponse 
            {
                NumeroConta = conta.Numero,
                NomeTitular = conta.Nome,
                HoraConsulta = DateTime.Now,
                Valor = saldo
            };
        }

        private float CalculaSaldo(IEnumerable<Movimento>? movimentos)
        {
            float saldo = 0;

            if (movimentos != null && movimentos.Any())
            {
                foreach (Movimento movimento in movimentos)
                {
                    // Adiciona Créditos e reduz Débitos
                    if (movimento.TipoMovimento == TIPOS_MOVIMENTOS.C.ToString())
                    {
                        saldo += movimento.Valor;
                    }
                    else if (movimento.TipoMovimento == TIPOS_MOVIMENTOS.D.ToString())
                    {
                        saldo -= movimento.Valor;
                    }
                }
            }

            return saldo;
        }

    }
}

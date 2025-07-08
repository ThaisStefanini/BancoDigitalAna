using BancoDigitalAna.Api.DTO;
using BancoDigitalAna.Core.Entities;
using BancoDigitalAna.Infraestrutura.Repository;
using BancoDigitalAna.Shared.Exceptions;
using CpfCnpjLibrary;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using static BancoDigitalAna.Shared.Helpers.Constantes;

namespace BancoDigitalAna.Core.Service
{
    public class AutenticacaoService
    {
        private readonly ContaCorrenteRepository _repository;
        private readonly HasherService _hasher;
        private readonly IConfiguration _configuration;

        public AutenticacaoService(IConfiguration configuration)
        {
            _repository = new ContaCorrenteRepository(configuration);
            _hasher = new HasherService();
            _configuration = configuration;
        }

        public async Task<LoginResponse> AutenticarAsync(LoginRequest request)
        {
            var conta = new ContaCorrente
            {
                Numero = request.NumeroConta ?? 0,
                Nome = Cpf.FormatarSemPontuacao(request.Cpf),
                Senha = request.Senha
            };

            ValidaConta(conta);

            ContaCorrente? contaBanco = new ContaCorrente();

            if (conta.Numero != 0 || conta.Nome != null)
            {
                contaBanco = await _repository.ObterConta(conta.Numero, conta.Nome);
            }

            if (contaBanco != null && contaBanco.IdContaCorrente != 0)
            {
                ValidaSenha(conta.Senha, contaBanco.Senha, contaBanco.Salt);
            }
            else
            {
                throw new BusinessException(
                    "Conta não existe",
                    USER_UNAUTHORIZED,
                    HttpStatusCode.Unauthorized);
            }

            // Erros ou dados inválidos irão gerar o BusinessException
            // Sendo assim, se chegou nesse ponto então todos os dados são válidos
            string token = GerarToken(contaBanco);

            return new LoginResponse
                {
                    Token = token,
                    ContaId = contaBanco.IdContaCorrente,
                    NumeroConta = contaBanco.Numero,
                    Cpf = contaBanco.Nome,
                    DataExpiracao = DateTime.UtcNow.AddMinutes(
                    _configuration.GetValue<int>("JwtSettings:ExpiryInMinutes"))
                };
        }

        private bool ValidaConta(ContaCorrente conta)
        {
            bool retorno = true;

            if (string.IsNullOrWhiteSpace(conta.Senha))
            {
                throw new BusinessException(
                    "Senha é obrigatória.",
                    INVALID_DOCUMENT,
                    HttpStatusCode.BadRequest);
            }

            if (conta.Numero == 0 && conta.Nome == null)
            {
                throw new BusinessException(
                    "Número da Conta ou CPF precisam ser preenchidos.",
                    INVALID_DOCUMENT,
                    HttpStatusCode.BadRequest);
            }

            return retorno;
        }

        private bool ValidaSenha(string? senha, string? senhaHash, string? salt)
        {
            bool retorno = false;

            if (senha != null && senhaHash != null && salt != null)
                retorno = _hasher.VerifyPassword(senha, senhaHash, salt);

            // Retorna Senha inválida se as senhas nulas ou se não for a senha igual a do banco
            if (!retorno)
            {
                throw new BusinessException(
                    "Senha inválida",
                    USER_UNAUTHORIZED,
                    HttpStatusCode.Unauthorized);
            }

            return retorno;
        }

        private string GerarToken(ContaCorrente conta)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, conta.Numero.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("contaId", conta.IdContaCorrente.ToString()),
                    new Claim("nomeTitular", conta.Nome ?? ""),
                    new Claim(ClaimTypes.Role, "Correntista") // Exemplo de role
                };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    jwtSettings.GetValue<int>("ExpiryInMinutes")),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool ValidarToken(int? id, string? validade)
        {
            bool retorno = true;

            if (id == null || id == 0)
            {
                throw new BusinessException(
                    "Token inválido",
                    INVALID_ACCOUNT,
                    HttpStatusCode.Forbidden);
            }

            if (!string.IsNullOrEmpty(validade) && long.TryParse(validade, out var unixTime))
            {
                // Converter Unix timestamp para DateTime
                var dataValidade = DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;

                // Verificar se o token está expirado
                if (dataValidade < DateTime.UtcNow)
                {
                    throw new BusinessException(
                        "Token expirado",
                        INVALID_DOCUMENT,
                        HttpStatusCode.Forbidden);
                }
            }
            else
            {
                throw new BusinessException(
                    "Validade do Token inválida",
                    INVALID_DOCUMENT,
                    HttpStatusCode.Forbidden);
            }

            return retorno;
        }

        public async Task<bool> ValidarSenha(int idContaCorrente, string? senha)
        {
            var conta = await _repository.ObterContaPorId(idContaCorrente);

            if (conta != null && conta.IdContaCorrente != 0)
            {
                // Gera BusinessException se senha não for válida
                ValidaSenha(senha, conta.Senha, conta.Salt);
            }
            else
            {
                throw new BusinessException(
                    "Conta não existe",
                    USER_UNAUTHORIZED,
                    HttpStatusCode.Unauthorized);
            }

            return true;
        }

        public int ValidaTokenEObtemContaId(ClaimsPrincipal user)
        {
            int contaId = int.Parse(user.FindFirst("contaId")?.Value);
            var dataValidade = user.FindFirst("exp")?.Value;

            ValidarToken(contaId, dataValidade);

            return contaId;
        }
    }
}

using BancoDigitalAna.Api.DTO;
using BancoDigitalAna.Core.Service;
using BancoDigitalAna.Shared.Exceptions;
using BancoDigitalAna.Shared.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using static BancoDigitalAna.Shared.Helpers.Constantes;

namespace BancoDigitalAna.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContaCorrenteController : ControllerBase
    {
        private readonly ILogger<ContaCorrenteController> _logger;
        private readonly IConfiguration _configuration;

        private readonly ContaCorrenteService _contacorrenteservice;
        private readonly AutenticacaoService _autenticacaoService;

        public ContaCorrenteController(ILogger<ContaCorrenteController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            _contacorrenteservice = new ContaCorrenteService(_configuration);
            _autenticacaoService = new AutenticacaoService(_configuration);
        }

        #region Endpoints Públicos

        [HttpPost("AdicionarContaCorrente")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AdicionarContaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AdicionarContaCorrente([FromBody] AdicionarContaRequest request)
        {
            try
            {
                // Cria a nova conta
                var retorno = await _contacorrenteservice.AdicionarContaCorrente(request);

                return Ok(retorno);
            }
            catch (BusinessException ex)
            {
                return StatusCode((int)ex.StatusCode, new ErrorResponse(ex.Message, ex.ErrorType));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro em " + MethodBase.GetCurrentMethod()?.Name);
                return StatusCode(500, new ErrorResponse(
                    $"Erro interno: {ex.Message}",
                    INTERNAL_ERROR));
            }
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var retorno = await _autenticacaoService.AutenticarAsync(request);

                return Ok(retorno);
            }
            catch (BusinessException ex)
            {
                return StatusCode((int)ex.StatusCode, new ErrorResponse(ex.Message, ex.ErrorType));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro em " + MethodBase.GetCurrentMethod()?.Name);
                return StatusCode(500, new ErrorResponse(
                    $"Erro interno: {ex.Message}",
                    INTERNAL_ERROR));
            }
        }

        #endregion

        #region Endpoints Protegidos

        [HttpPost("InativarContaCorrente")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InativarContaCorrente([FromBody] InativarContaRequest request)
        {
            try
            {
                // Obtém id do token JWT
                var contaId = _autenticacaoService.ValidaTokenEObtemContaId(User);

                await _autenticacaoService.ValidarSenha(contaId, request.Senha);

                // Erros ou dados inválidos irão gerar o BusinessException
                // Sendo assim, se chegou nesse ponto então todos os dados são válidos
                await _contacorrenteservice.InativarContaCorrente(contaId);

                return NoContent();
            }
            catch (BusinessException ex)
            {
                return StatusCode((int)ex.StatusCode, new ErrorResponse(ex.Message, ex.ErrorType));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro em " + MethodBase.GetCurrentMethod()?.Name);
                return StatusCode(500, new ErrorResponse(
                    $"Erro interno: {ex.Message}",
                    INTERNAL_ERROR));
            }
        }

        [HttpPost("MovimentarContaCorrente")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MovimentarContaCorrente([FromBody] MovimentacaoRequest request)
        {
            try
            {
                // Obtém id do token JWT
                var contaId = _autenticacaoService.ValidaTokenEObtemContaId(User);

                // Erros ou dados inválidos irão gerar o BusinessException
                // Sendo assim, se chegou nesse ponto então o token é válido
                await _contacorrenteservice.MovimentarContaCorrente(contaId, request);

                return NoContent();
            }
            catch (BusinessException ex)
            {
                return StatusCode((int)ex.StatusCode, new ErrorResponse(ex.Message, ex.ErrorType));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro em " + MethodBase.GetCurrentMethod()?.Name);
                return StatusCode(500, new ErrorResponse(
                    $"Erro interno: {ex.Message}",
                    INTERNAL_ERROR));
            }
        }

        [HttpGet("SaldoContaCorrente")]
        [Authorize]
        [ProducesResponseType(typeof(SaldoContaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SaldoContaCorrente()
        {
            try
            {
                // Obtém id do token JWT
                var contaId = _autenticacaoService.ValidaTokenEObtemContaId(User);

                // Erros ou dados inválidos irão gerar o BusinessException
                // Sendo assim, se chegou nesse ponto então o token é válido
                var saldoResponse = await _contacorrenteservice.ObterSaldoContaCorrente(contaId);

                return Ok(saldoResponse);
            }
            catch (BusinessException ex)
            {
                return StatusCode((int)ex.StatusCode, new ErrorResponse(ex.Message, ex.ErrorType));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro em " + MethodBase.GetCurrentMethod()?.Name);
                return StatusCode(500, new ErrorResponse(
                    $"Erro interno: {ex.Message}",
                    INTERNAL_ERROR));
            }
        }

        #endregion
    }
}

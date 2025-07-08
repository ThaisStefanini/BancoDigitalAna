using BancoDigitalAna.Api.DTO;
using BancoDigitalAna.Core.Service;
using BancoDigitalAna.Shared.Exceptions;
using BancoDigitalAna.Shared.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Reflection;
using static BancoDigitalAna.Shared.Helpers.Constantes;

namespace BancoDigitalAna.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransferenciaController : ControllerBase
    {
        private readonly ILogger<ContaCorrenteController> _logger;
        private readonly IConfiguration _configuration;

        private readonly IHttpClientFactory _httpClientFactory;

        private readonly TransferenciaService _transferenciaService;
        private readonly AutenticacaoService _autenticacaoService;

        public TransferenciaController(ILogger<ContaCorrenteController> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;

            _httpClientFactory = httpClientFactory;

            _transferenciaService = new TransferenciaService(_configuration);
            _autenticacaoService = new AutenticacaoService(_configuration);
        }

        #region Endpoints Protegidos

        [HttpPost("TransferenciaMesmaInstituicao")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> TransferenciaMesmaInstituicao([FromBody] TransferenciaInternaRequest request)
        {
            try
            {
                // Realiza Transferência
                await TransfereEntreContas(request);

                // Registra Transferência
                await AdicionaTransferencia(request);

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

        #endregion

        #region Métodos internos

        private async Task<bool> TransfereEntreContas(TransferenciaInternaRequest request)
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString();

            // Chamar API Movimento com débito via HTTP
            var debitoResponse = await ChamarApiMovimento(token, 0, request.Valor, TIPOS_MOVIMENTOS.D);

            if (!debitoResponse.IsSuccessStatusCode)
            {
                throw new BusinessException(
                    await debitoResponse.Content.ReadAsStringAsync(),
                    INTERNAL_ERROR,
                    debitoResponse.StatusCode);
            }

            // Chamar API Movimento com crédito via HTTP
            var creditoResponse = await ChamarApiMovimento(token, request.NumeroContaDestino, request.Valor, TIPOS_MOVIMENTOS.C);

            if (!creditoResponse.IsSuccessStatusCode)
            {
                // Compensação
                await ChamarApiMovimento(token, 0, request.Valor, TIPOS_MOVIMENTOS.C);

                throw new BusinessException(
                    await creditoResponse.Content.ReadAsStringAsync(),
                    INTERNAL_ERROR,
                    creditoResponse.StatusCode);
            }

            return true;
        }
        private async Task<HttpResponseMessage> ChamarApiMovimento(string token, int numeroConta, float valor, TIPOS_MOVIMENTOS tipoMovimento)
        {
            return await ChamarApiInterna(
                    "/ContaCorrente/MovimentarContaCorrente",
                    token,
                    new MovimentacaoRequest { NumeroConta = numeroConta, Valor = valor, TipoMovimento = tipoMovimento.ToString() }); ;
        }
        
        private async Task<HttpResponseMessage> ChamarApiInterna(string url, string token, object data)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));

            var urlBase = _configuration["ApiConfig:UrlBase"];

            var response = await client.PostAsJsonAsync($"{urlBase}{url}", data);
            return response;
        }

        private async Task<bool> AdicionaTransferencia(TransferenciaInternaRequest request)
        {
            // Obtém id do token JWT
            var contaId = _autenticacaoService.ValidaTokenEObtemContaId(User);

            await _transferenciaService.AdicionarTransferencia(contaId, request);
            return true;
        }

        #endregion
    }
}

namespace BancoDigitalAna.Api.DTO
{
    public class MovimentacaoRequest
    {
        public int NumeroConta { get; set; }
        public float Valor { get; set; }
        public string? TipoMovimento { get; set; }
    }
}

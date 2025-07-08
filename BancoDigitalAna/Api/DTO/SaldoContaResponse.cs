namespace BancoDigitalAna.Api.DTO
{
    public class SaldoContaResponse
    {
        public int NumeroConta { get; set; }
        public string? NomeTitular { get; set; }
        public DateTime HoraConsulta { get; set; }
        public float Valor { get; set; }
    }
}

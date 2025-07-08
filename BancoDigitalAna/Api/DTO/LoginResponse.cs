namespace BancoDigitalAna.Api.DTO
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public int ContaId { get; set; }
        public int NumeroConta { get; set; }
        public string Cpf { get; set; }
        public DateTime DataExpiracao { get; set; }
    }
}

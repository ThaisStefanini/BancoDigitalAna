namespace BancoDigitalAna.Api.DTO
{
    public class LoginRequest
    {
        public int? NumeroConta { get; set; }
        public string? Cpf { get; set; }
        public string? Senha { get; set; }
    }
}

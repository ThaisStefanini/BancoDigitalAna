using Microsoft.Extensions.Configuration;

namespace BancoDigitalAna.Infraestrutura.Repository.Base
{
    public abstract class _Repository
    {
        protected readonly string _connectionString;
        public _Repository(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("ConnectionStrings:SQLite") ?? "";
        }
    }
}

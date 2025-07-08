using Dapper;
using System.Data;
using System.Globalization;

namespace BancoDigitalAna.Shared.Helpers
{
    public class DateTimeHandler : SqlMapper.TypeHandler<DateTime>
    {
        public override DateTime Parse(object value)
        {
            // Converte TEXT do SQLite para DateTime
            return DateTime.ParseExact((string)value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        public override void SetValue(IDbDataParameter parameter, DateTime value)
        {
            // Converte DateTime para TEXT no formato SQLite
            parameter.Value = value.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}

using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace BancoDigitalAna.Core.Service
{
    public class HasherService
    {
        private readonly int _degreeOfParallelism = 8;
        private readonly int _iterations = 4;
        private readonly int _memorySize = 65536; // 64MB

        public (string Hash, string Salt) HashPassword(string password)
        {
            // Gera um salt único
            var salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var saltBase64 = Convert.ToBase64String(salt);
            var hash = HashWithArgon2(password, salt);

            return (hash, saltBase64);
        }

        public bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            try
            {
                var salt = Convert.FromBase64String(storedSalt);
                var expectedHash = storedHash;
                var actualHash = HashWithArgon2(password, salt);

                return CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(actualHash),
                    Encoding.UTF8.GetBytes(expectedHash));
            }
            catch
            {
                return false;
            }
        }

        private string HashWithArgon2(string password, byte[] salt)
        {
            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = _degreeOfParallelism;
                argon2.Iterations = _iterations;
                argon2.MemorySize = _memorySize;

                var hash = argon2.GetBytes(32); // Hash de 32 bytes
                return Convert.ToBase64String(hash);
            }
        }
    }
}

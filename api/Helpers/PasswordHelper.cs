using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace api.Helpers
{
    public static class PasswordHelper
    {
        //  Methods
        public static void CreatePasswordHash(string passwordHash, out string passwordDoubleHash, out string passwordSalt)
        {
            byte[] salt = new byte[128 / 8];
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();

            rng.GetBytes(salt);

            passwordSalt = Convert.ToBase64String(salt);
            passwordDoubleHash = GetPasswordDoubleHash(passwordHash, salt);
        }
        public static bool VerifyPassword(string passwordHash, string storedDoubleHash, string storedSalt)
        {
            byte[] salt = Convert.FromBase64String(storedSalt);
            string hashed = GetPasswordDoubleHash(passwordHash, salt);

            return hashed == storedDoubleHash;
        }
        private static string GetPasswordDoubleHash(string Hash, byte[] salt)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: Hash,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8
            ));
        }
    }
}
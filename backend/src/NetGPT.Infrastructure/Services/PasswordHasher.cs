using System;
using System.Security.Cryptography;

namespace NetGPT.Infrastructure.Services
{
    public sealed class PasswordHasher : IPasswordHasher
    {
        public void CreateHash(string password, out byte[] hash, out byte[] salt)
        {
            using var hmac = new HMACSHA512();
            salt = hmac.Key;
            hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password ?? string.Empty));
        }

        public bool Verify(string password, byte[] hash, byte[] salt)
        {
            using var hmac = new HMACSHA512(salt);
            var computed = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password ?? string.Empty));
            if (computed.Length != hash.Length) return false;
            for (int i = 0; i < hash.Length; i++) if (computed[i] != hash[i]) return false;
            return true;
        }
    }
}

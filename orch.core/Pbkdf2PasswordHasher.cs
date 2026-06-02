using System.Security.Cryptography;
using System.Text;

namespace orch.core
{
    public class Pbkdf2PasswordHasher : IPasswordHasher
    {
        private const byte FormatMarker = 0x02;
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100_000;

        private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

        public byte[] Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var key = DeriveKey(password, salt);
            var result = new byte[1 + SaltSize + KeySize];
            result[0] = FormatMarker;
            Buffer.BlockCopy(salt, 0, result, 1, SaltSize);
            Buffer.BlockCopy(key, 0, result, 1 + SaltSize, KeySize);
            return result;
        }

        public bool Verify(string password, byte[] storedHash)
        {
            if (storedHash == null || storedHash.Length == 0)
                return false;

            if (IsLegacyHash(storedHash))
                return LegacySha256Verify(password, storedHash);

            if (storedHash.Length != 1 + SaltSize + KeySize || storedHash[0] != FormatMarker)
                return false;

            var salt = storedHash.AsSpan(1, SaltSize);
            var expectedKey = storedHash.AsSpan(1 + SaltSize, KeySize);
            var actualKey = DeriveKey(password, salt);
            return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
        }

        public bool IsLegacyHash(byte[] storedHash)
            => storedHash is { Length: 32 };

        private static byte[] DeriveKey(string password, ReadOnlySpan<byte> salt)
        {
            return Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                Iterations,
                HashAlgorithm,
                KeySize);
        }

        private static bool LegacySha256Verify(string password, byte[] storedHash)
        {
            var computed = OSystemService.HashPassword(password);
            return CryptographicOperations.FixedTimeEquals(computed, storedHash);
        }
    }
}

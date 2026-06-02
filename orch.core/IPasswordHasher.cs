namespace orch.core
{
    public interface IPasswordHasher
    {
        byte[] Hash(string password);
        bool Verify(string password, byte[] storedHash);
        bool IsLegacyHash(byte[] storedHash);
    }
}

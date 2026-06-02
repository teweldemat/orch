namespace orch.core
{
    public interface ILoginRateLimiter
    {
        void CheckRateLimit(string userName, string remoteIp);
    }
}

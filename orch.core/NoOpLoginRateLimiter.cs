namespace orch.core
{
    public class NoOpLoginRateLimiter : ILoginRateLimiter
    {
        public void CheckRateLimit(string userName, string remoteIp)
        {
        }
    }
}

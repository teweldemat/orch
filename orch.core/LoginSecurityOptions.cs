namespace orch.core
{
    public class LoginSecurityOptions
    {
        public const string SectionName = "LoginSecurity";

        public int MaxFailedAttempts { get; set; } = 5;
        public int LockoutMinutes { get; set; } = 15;
        public int IpPermitLimit { get; set; } = 30;
        public int IpWindowMinutes { get; set; } = 1;
        public int UsernamePermitLimit { get; set; } = 10;
        public int UsernameWindowMinutes { get; set; } = 15;
    }
}

#nullable enable
namespace orch.core.model
{
    public class AccessTokenRequestContext
    {
        public string? AuthMethod { get; set; }
        public string? RemoteIp { get; set; }
        public string? XForwardedFor { get; set; }
        public string? ForwardedHeader { get; set; }
        public string? UserAgent { get; set; }
        public string? AcceptLanguage { get; set; }
        public string? Origin { get; set; }
        public string? Referer { get; set; }
        public string? ServerRequestId { get; set; }
        public string? ClientInfoRaw { get; set; }
        public string? ClientInfoJson { get; set; }
        public string? ClientInfoHash { get; set; }
        public string? BrowserFingerprintHash { get; set; }
        public string? NetworkFingerprintHash { get; set; }
    }
}

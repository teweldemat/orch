using Microsoft.Extensions.Options;
using orch.core.model;

namespace orch.core
{
    public interface ISystemService : IDisposable
    {
        AccessToken CreateAccessToken(
            string userName, string password, string clientInfo, int? maxTokens = null, long? expiryTime = null);
        AccessToken CreateAccessToken(
            string userName, string password, AccessTokenRequestContext requestContext, int? maxTokens = null, long? expiryTime = null);
        AccessToken PingAccessToken(Guid? access_token, AccessTokenRequestContext? requestContext = null);
        void DeleteAccessToken(params Guid[] accessTokens);
        AccessToken GetAccessTokenInfo(Guid accessToken);
        ContentFile SaveFile(string fileName, Stream r, Guid? fileId = null);
        ContentFile GetFile(Guid file_id);
    }

    public class OSystemService : ISystemService
    {
        private static readonly System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create();

        public static byte[] HashPassword(String password)
        {
            var bytes = System.Text.UTF8Encoding.UTF8.GetBytes(password);
            return sha.ComputeHash(bytes);
        }

        internal static string BuildAccountLockedMessage(long lockoutUntil, long now)
        {
            var remainingMs = Math.Max(0, lockoutUntil - now);
            var remainingMinutes = Math.Max(1, (int)Math.Ceiling(remainingMs / 60000.0));
            return remainingMinutes == 1
                ? "Too many failed login attempts. Your account is temporarily locked. Try again in about 1 minute or contact your administrator."
                : $"Too many failed login attempts. Your account is temporarily locked. Try again in about {remainingMinutes} minutes or contact your administrator.";
        }

        protected readonly IOHost host;
        protected readonly ISystemDatabase sysDb;
        protected readonly ITransactionDatabase tranDb;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILoginRateLimiter _loginRateLimiter;
        private readonly LoginSecurityOptions _loginSecurityOptions;

        public OSystemService(
            IOHost host,
            ISystemDatabase db,
            ITransactionDatabase command,
            IPasswordHasher? passwordHasher = null,
            ILoginRateLimiter? loginRateLimiter = null,
            IOptions<LoginSecurityOptions>? loginSecurityOptions = null)
        {
            this.host = host;
            this.sysDb = db;
            this.tranDb = command;
            _passwordHasher = passwordHasher ?? new Pbkdf2PasswordHasher();
            _loginRateLimiter = loginRateLimiter ?? new NoOpLoginRateLimiter();
            _loginSecurityOptions = loginSecurityOptions?.Value ?? new LoginSecurityOptions();
        }

        public AccessToken CreateAccessToken(
            string userName, string password,  string clientInfo,  int? maxTokens = null, long? expiryTime = null)
        {
            return CreateAccessToken(userName, password, new AccessTokenRequestContext
            {
                ClientInfoRaw = clientInfo
            }, maxTokens, expiryTime);
        }

        public AccessToken CreateAccessToken(
            string userName, string password, AccessTokenRequestContext requestContext, int? maxTokens = null, long? expiryTime = null)
        {
            if (maxTokens < 0)
                throw new ArgumentException($"{nameof(maxTokens)} cannot be less than 0");

            var now = host.CurrentTime();
            
            // '0' expiry time means the token never expires
            if (expiryTime == default(long))
                expiryTime = null;
            
            if (expiryTime <= now)
                throw new ArgumentException("Access token expiry time must be in the future.");

            _loginRateLimiter.CheckRateLimit(userName, requestContext?.RemoteIp ?? string.Empty);
            
            var user = tranDb.GetUserInfo(userName, true);
            var rootUser = tranDb.GetRootUser();
            
            if (rootUser == null)
                throw new InvalidOperationException("Root user doesn't exist, has the system been initialized?");

            if (user != null && user.LockoutUntil is > 0 && user.LockoutUntil > now)
                throw new InvalidOperationException(BuildAccountLockedMessage(user.LockoutUntil.Value, now));

            if (user == null ||
                user.PasswordHash is not { } passwordHash ||
                !_passwordHasher.Verify(password, passwordHash))
            {
                if (user != null)
                {
                    tranDb.RecordFailedLogin(
                        user.Id,
                        _loginSecurityOptions.MaxFailedAttempts,
                        _loginSecurityOptions.LockoutMinutes,
                        now);
                }

                throw new InvalidOperationException("Incorrect username and/or password");
            }

            if (!user.Enabled)
                throw new InvalidOperationException($"User {userName} is disabled");

            tranDb.ResetLoginFailures(user.Id);

            if (_passwordHasher.IsLegacyHash(user.PasswordHash))
                tranDb.UpdatePasswordHash(user.Id, _passwordHasher.Hash(password));

            if (maxTokens is > 0 && user.Id != rootUser.Id)
            {
                var tokens = sysDb.GetTokensByUserId(user.Id);
                var excessTokens = tokens.Count - maxTokens.Value + 1;

                if (excessTokens > 0)
                {
                    var tokensToDelete = tokens
                        .OrderBy(t => t.CreatedTime)
                        .Take(excessTokens)
                        .Select(t => t.Token)
                        .ToArray();
                    
                    sysDb.DeleteAccessToken(tokensToDelete);
                }
            }

            var accessToken = new AccessToken()
            {
                CreatedTime = now,
                ExpiryTime = expiryTime,
                LastUsed = now,
                Token = host.NextGuid(),
                UserId = user.Id,
                AuthMethod = requestContext?.AuthMethod,
                ClientInfoRaw = requestContext?.ClientInfoRaw,
                ClientInfoJson = requestContext?.ClientInfoJson,
                ClientInfoHash = requestContext?.ClientInfoHash,
                BrowserFingerprintHash = requestContext?.BrowserFingerprintHash,
                NetworkFingerprintHash = requestContext?.NetworkFingerprintHash,
                CreatedIp = requestContext?.RemoteIp,
                LastSeenIp = requestContext?.RemoteIp,
                XForwardedFor = requestContext?.XForwardedFor,
                ForwardedHeader = requestContext?.ForwardedHeader,
                UserAgent = requestContext?.UserAgent,
                AcceptLanguage = requestContext?.AcceptLanguage,
                Origin = requestContext?.Origin,
                Referer = requestContext?.Referer,
                ServerRequestId = requestContext?.ServerRequestId
            };

            sysDb.CreateAccessToken(accessToken);
            
            return accessToken;
        }

        public AccessToken PingAccessToken(Guid? access_token, AccessTokenRequestContext? requestContext = null)
        {
            if (access_token == null)
                throw new UnauthorizedAccessException();
            return sysDb.PingAccessToken(access_token.Value, requestContext);
        }

        public void DeleteAccessToken(params Guid[] accessTokens)
        {
            sysDb.DeleteAccessToken(accessTokens);
        }

        public AccessToken GetAccessTokenInfo(Guid accessToken)
        {
            return sysDb.GetAccessTokenInfo(accessToken);
        }

        public ContentFile SaveFile(string fileName, Stream r, Guid? fileId = null)
        {
            return sysDb.SaveFile(fileName, r, fileId);
        }

        public ContentFile GetFile(Guid file_id)
        {
            return sysDb.GetFile(file_id);
        }

        public void Dispose()
        {
            sysDb.Dispose();
            tranDb.Dispose();
        }
    }
}

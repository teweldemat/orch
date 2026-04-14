using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using orch.core.model;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace orch.core.auth
{
    public class AccessTokenAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemaName = "AccessTokenAuth";

        public AccessTokenAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Check in the cookies first
            var tokenFromCookie = Request.Cookies.TryGetValue("access_token", out string tokenString);
            if (!tokenFromCookie)
            {
                // If not in cookies, then check in the query string
                tokenString = Request.Query["access_token"];
            }

            if (Guid.TryParse(tokenString, out Guid accessToken))
            {
                try
                {
                    var systemService = Context.RequestServices.GetRequiredService<ISystemService>();
                    var tranDb = Context.RequestServices.GetRequiredService<ITransactionDatabase>();
                    var requestContext = CreateRequestContext(Request);

                    var token = Authenticate(accessToken, systemService, tranDb, requestContext);

                    var user = tranDb.GetUserInfo(token.UserId);

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    };

                    var identity = new ClaimsIdentity(claims, SchemaName);
                    var principal = new ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, SchemaName);

                    return Task.FromResult(AuthenticateResult.Success(ticket));
                }
                catch (AuthenticationException authEx)
                {
                    ClearAccessTokenCookieIfPresent();
                    return Task.FromResult(AuthenticateResult.Fail(authEx.Message));
                }
                catch (InvalidOperationException)
                {
                    ClearAccessTokenCookieIfPresent();
                    return Task.FromResult(AuthenticateResult.NoResult());
                }
            }

            return Task.FromResult(AuthenticateResult.NoResult());
        }

        private void ClearAccessTokenCookieIfPresent()
        {
            if (Request.Cookies.ContainsKey("access_token"))
            {
                Response.Cookies.Delete("access_token");
            }
        }

        private static AccessToken Authenticate(
            Guid accessToken,
            ISystemService systemService,
            ITransactionDatabase tranDb,
            AccessTokenRequestContext requestContext)
        {
            if (accessToken == Guid.Empty)
            {
                throw new AuthenticationException("Authentication required. Please log in.");
            }

            var token = systemService.PingAccessToken(accessToken, requestContext);
            if (token == null)
            {
                throw new AuthenticationException("The provided access token does not match any existing tokens.");
            }

            var enabled = tranDb.GetUserStatus(token.UserId);
            if (!enabled)
            {
                systemService.DeleteAccessToken(token.Token);
                throw new AuthenticationException("The user associated with this token is disabled. Token has been invalidated.");
            }

            return token;
        }

        private static AccessTokenRequestContext CreateRequestContext(HttpRequest request)
        {
            string GetHeader(string name)
            {
                var value = request.Headers[name].ToString();
                return string.IsNullOrWhiteSpace(value) ? null : value;
            }

            return new AccessTokenRequestContext
            {
                RemoteIp = request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                XForwardedFor = GetHeader("X-Forwarded-For"),
                ForwardedHeader = GetHeader("Forwarded"),
                UserAgent = GetHeader("User-Agent"),
                AcceptLanguage = GetHeader("Accept-Language"),
                Origin = GetHeader("Origin"),
                Referer = GetHeader("Referer"),
                ServerRequestId = GetHeader("X-Request-ID") ?? request.HttpContext.TraceIdentifier
            };
        }
    }
}

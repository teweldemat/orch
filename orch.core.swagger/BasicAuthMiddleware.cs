using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using orch.common;
using orch.utils.web;
using System.Text;

namespace orch.core.swagger
{
    public class BasicAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private const string TokenKey = "access_token";

        public BasicAuthMiddleware(RequestDelegate next, IServiceProvider serviceProvider, ILogger<BasicAuthMiddleware> logger)
        {
            _next = next;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();

            if (authHeader != null && authHeader.StartsWith("Basic "))
            {
                var token = authHeader["Basic ".Length..].Trim();
                string[] credentials = Encoding.UTF8.GetString(Convert.FromBase64String(token)).Split(':');
                if (credentials.Length < 2 || string.IsNullOrEmpty(credentials[0]) || string.IsNullOrEmpty(credentials[1]))
                {
                    DenyAccess(context);
                    return;
                }

                var username = credentials[0];
                var password = credentials[1];
                var existingToken = ManageSessionToken(context, username, password);
                ModifyRequestHeadersAndQueryString(context, existingToken);
                context.Response.Cookies.Append("access_token", existingToken.ToString());
            }

            await _next(context);
        }

        private static void DenyAccess(HttpContext context)
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            context.Response.WriteAsync(JsonConvert.SerializeObject(new ErrorInfo("Unauthenticated: Both username and password must be provided.", null)));
        }

        private Guid ManageSessionToken(HttpContext context, string username, string password)
        {
            using var scope = _serviceProvider.CreateScope();
            var sysService = scope.ServiceProvider.GetRequiredService<OSystemService>();
            var host = scope.ServiceProvider.GetRequiredService<IOHost>();

            if (context.Session.TryGetValue(username, out var sessionTokenBytes) && Guid.TryParse(Encoding.UTF8.GetString(sessionTokenBytes), out var existingToken))
            {
                if (sysService.PingAccessToken(existingToken) is { ExpiryTime: not null } existingAccessToken && Helpers.LongToTime((long)existingAccessToken.ExpiryTime) <= Helpers.LongToTime(host.CurrentTime()))
                {
                    TryDeleteAccessToken(sysService, existingToken);
                    return CreateSessionToken(context, sysService, username, password);
                }
                return existingToken;
            }

            return CreateSessionToken(context, sysService, username, password);
        }

        private void TryDeleteAccessToken(ISystemService sysService, Guid existingToken)
        {
            try
            {
                sysService.DeleteAccessToken(existingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to delete token: {TokenError}", ex.Message);
            }
        }

        private static Guid CreateSessionToken(HttpContext context, ISystemService sysService, string username, string password)
        {
            var newToken = sysService.CreateAccessToken(username, password, "SwaggerUI").Token;
            context.Session.Set(username, newToken.ToByteArray());
            return newToken;
        }

        private void ModifyRequestHeadersAndQueryString(HttpContext context, Guid token)
        {
            context.Request.Headers[TokenKey] = token.ToString();
            var queryString = context.Request.QueryString.ToString();
            
            var tranDb = context.RequestServices.GetRequiredService<ITransactionDatabase>();
            var systemInformation = tranDb.GetCurrentSystemInformation();
            
            if (systemInformation?.SystemId is not { } systemId)
                throw new InvalidOperationException("System Id not set. Has the system been bootstrapped?");

            if (string.IsNullOrEmpty(queryString))
            {
                queryString = $"?{TokenKey}={token}&system_id={systemId}";
            }
            else
            {
                var prefix = queryString.Contains('?') ? '&' : '?';
                queryString += $"{prefix}{TokenKey}={token}&system_id={systemId}";
            }

            context.Request.QueryString = new QueryString(queryString);
        }
    }
}

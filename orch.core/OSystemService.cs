using orch.core.model;

namespace orch.core
{
    public interface ISystemService : IDisposable
    {
        Guid CreateAccessToken(
            string userName, string password, string clientInfo, int? maxTokens = null, long? expiryTime = null);
        AccessToken PingAccessToken(Guid? access_token);
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

        private readonly IOHost host;
        private readonly ISystemDatabase sysDb;
        private readonly ITransactionDatabase tranDb;

        public OSystemService(IOHost host, ISystemDatabase db, ITransactionDatabase command)
        {
            this.host = host;
            this.sysDb = db;
            this.tranDb = command;
        }

        public Guid CreateAccessToken(
            string userName, string password,  string clientInfo,  int? maxTokens = null, long? expiryTime = null)
        {
            if (maxTokens < 0)
                throw new ArgumentException($"{nameof(maxTokens)} cannot be less than 0");

            var now = host.CurrentTime();
            var user = tranDb.GetUserInfo(userName, true);
            var rootUser = tranDb.GetRootUser();
            
            if (rootUser == null)
                throw new InvalidOperationException("Root user doesn't exist, has the system been initialized?");

            if (user == null)
                throw new InvalidOperationException($"User {userName} doesn't exist");

            if (!user.Enabled)
                throw new InvalidOperationException($"User {userName} is disabled");

            var hash = HashPassword(password);
            if (!user.PasswordHash.SequenceEqual(hash))
                throw new InvalidOperationException($"Invalid password");

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
                UserId = user.Id
            };

            sysDb.CreateAccessToken(accessToken);
            return accessToken.Token;
        }

        public AccessToken PingAccessToken(Guid? access_token)
        {
            if (access_token == null)
                throw new UnauthorizedAccessException();
            return sysDb.PingAccessToken(access_token.Value);
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
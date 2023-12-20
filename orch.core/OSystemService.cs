using orch.core.model;

namespace orch.core
{
    public interface ISystemService : IDisposable
    {
        Guid CreateAccessToken(string userName, string password, string clientInfo, int? maxTokens = null);
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

        private IOHost host;
        private ISystemDatabase db;
        private ITransactionDatabase command;

        public OSystemService(IOHost host, ISystemDatabase db, ITransactionDatabase command)
        {
            this.host = host;
            this.db = db;
            this.command = command;
        }

        public Guid CreateAccessToken(string userName, string password, string clientInfo, int? maxTokens = null)
        {
            var now = host.CurrentTime();
            var user = command.GetUserInfo(userName, true);
            var rootUser = command.GetRootUser();

            if (user == null)
                throw new InvalidOperationException($"User {userName} doesn't exist");
            if (!user.Enabled)
                throw new InvalidOperationException($"User {userName} is disabled");

            var hash = HashPassword(password);
            if (!Enumerable.SequenceEqual(user.PasswordHash, hash))
                throw new InvalidOperationException($"Invalid password");

            if (maxTokens.HasValue && maxTokens > 0 && user.Id != rootUser.Id)
            {
                var tokens = db.GetTokensByUserId(user.Id);
                int excessTokens = tokens.Count - maxTokens.Value + 1;

                if (excessTokens > 0)
                {
                    var tokensToDelete = tokens.OrderBy(t => t.CreatedTime).Take(excessTokens).Select(t => t.Token).ToArray();
                    db.DeleteAccessToken(tokensToDelete);
                }
            }

            var accessToken = new AccessToken()
            {
                CreatedTime = now,
                ExpiryTime = null,
                LastUsed = now,
                Token = host.NextGuid(),
                UserId = user.Id

            };

            db.CreateAccessToken(accessToken);
            return accessToken.Token;
        }

        public AccessToken PingAccessToken(Guid? access_token)
        {
            if (access_token == null)
                throw new UnauthorizedAccessException();
            return db.PingAccessToken(access_token.Value);
        }

        public void DeleteAccessToken(params Guid[] accessTokens)
        {
            db.DeleteAccessToken(accessTokens);
        }

        public AccessToken GetAccessTokenInfo(Guid accessToken)
        {
            return db.GetAccessTokenInfo(accessToken);
        }

        public ContentFile SaveFile(string fileName, Stream r, Guid? fileId = null)
        {
            return db.SaveFile(fileName, r, fileId);
        }

        public ContentFile GetFile(Guid file_id)
        {
            return db.GetFile(file_id);
        }

        public void Dispose()
        {
            db.Dispose();
            command.Dispose();
        }
    }
}
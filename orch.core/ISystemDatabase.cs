using orch.core.model;

namespace orch.core
{
    public interface ISystemDatabase : IDisposable
    {
        AccessToken PingAccessToken(Guid tokenId);
        AccessToken GetAccessTokenInfo(Guid tokenId);
        List<AccessToken> GetTokensByUserId(Guid userId);
        void DeleteAccessToken(params Guid[] accessTokens);
        void CreateAccessToken(AccessTokenProps accessToken);
        void CreateFile(ContentFile cf);
        ContentFile GetFile(Guid file_id);
        ContentFile SaveFile(string fileName, Stream r, Guid? fileId = null);
    }
}
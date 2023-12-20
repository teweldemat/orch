namespace orch.core.ef.System.DTOs
{
    public class CreateAccessTokenRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ClientInfo { get; set; }

        public CreateAccessTokenRequest(string userName, string passWord, string clientInfo)
            => (UserName, Password, ClientInfo) = (userName, passWord, clientInfo);
    }
}
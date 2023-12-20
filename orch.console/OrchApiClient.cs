using Newtonsoft.Json;
using System.Text;

namespace orch.console
{
    public class OrchApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;

        public OrchApiClient(string baseUri)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(baseUri) };
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        public async Task<T> ExecuteQueryAsync<T>(Guid accessToken, string query, string pars = null)
        {
            var url = $"/api/query?access_token={accessToken}&query={query}";
            if (!string.IsNullOrEmpty(pars))
            {
                url += $"&pars={Uri.EscapeDataString(pars)}";
            }

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseString);
        }


        public async Task<Guid> PostTransactionAsync(Guid? systemId, Guid? accessToken, string commandType, string jsonCommandData, string query = null, string pars = null)
        {
            var url = $"/api/command?command_type={commandType}";
            if (systemId.HasValue) url += $"&system_id={systemId.Value}";
            if (accessToken.HasValue) url += $"&access_token={accessToken.Value}";
            if (!string.IsNullOrEmpty(query)) url += $"&query={Uri.EscapeDataString(query)}";
            if (!string.IsNullOrEmpty(pars)) url += $"&pars={Uri.EscapeDataString(pars)}";

            var content = new StringContent(jsonCommandData, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Guid>(result);
        }

    }

}
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace CloudServiceTest
{
    public class BingSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _endpoint;

        public BingSearchService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _endpoint = "https://api.bing.microsoft.com/v7.0/search";
            _apiKey = configuration["AzureBingSearch:Key"];
            //_endpoint = configuration["AzureBingSearch:Endpoint"];
        }

        public async Task<string> SearchAsync(string query)
        {
            var requestUri = $"{_endpoint}?q={Uri.EscapeDataString(query)}";
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiKey);

            var response = await _httpClient.GetAsync(requestUri);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Models.Azure.BingSearchResponse>(jsonResponse);
            }

            return null;
        }
    }
}

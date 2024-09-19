using CloudServiceTest.Models.Azure;
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
            _endpoint = configuration["AzureBingSearch:Endpoint"];
            _apiKey = configuration["AzureBingSearch:Key"];
            //_endpoint = configuration["AzureBingSearch:Endpoint"];
        }

        public async Task<Models.Azure.BingSearchImage> SearchAsync(string query)
        {
            var requestUri = $"{_endpoint}?q={Uri.EscapeDataString(query)}";
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiKey);

            var response = await _httpClient.GetAsync(requestUri);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var images = JsonSerializer.Deserialize<Models.Azure.BingSearchImages>(jsonResponse);
                if (images?.value?.Length > 0) 
                {
                    Random random = new Random();
                    return images.value[random.Next(0, images.value.Length)];
                    
                }
            }

            return null;
        }
    }
}

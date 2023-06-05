namespace SpamFilter.GptApi.Services;

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Models;

public class GptService : IGptService
{
    private string _apiKey;

    public GptService(string apiKey)
    {
        _apiKey = apiKey;
    }

    public async Task<GptResponse> GenerateCompletionAsync(string prompt)
    {
        var httpClient = new HttpClient();

        var requestData = new
        {
            prompt,
            max_tokens = 60
        };

        var jsonString = JsonSerializer.Serialize(requestData);

        var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await httpClient.PostAsync("https://api.openai.com/v1/engines/davinci-codex/completions", httpContent);

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            var generationResponse = JsonSerializer.Deserialize<GptResponse>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return generationResponse
                   ?? throw new FormatException($"Couldn't deserialize response from GPT-3 API.");
        }

        throw new Exception($"Request to GPT-3 API failed with status code {response.StatusCode}");
    }
}

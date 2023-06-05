namespace SpamFilter.GptApi.Services;

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Models;
using OpenAI;
using OpenAI.Completions;
using OpenAI.Models;
using Choice = Models.Choice;

public class GptService : IGptService
{
    private string _apiKey;

    public GptService(string apiKey)
    {
        _apiKey = apiKey;
    }

    public async Task<IReadOnlyList<Choice>> GenerateCompletionAsync(string prompt)
    {
        var auth = new OpenAIAuthentication(_apiKey);
        var api = new OpenAIClient(auth);

        var result = await api.CompletionsEndpoint.CreateCompletionAsync(prompt, model: Model.Davinci);

        return result.Completions
            .Select(
            x => new Choice
            {
                Text = x.Text,
                FinishReason = x.FinishReason,
            })
            .ToArray();
    }
}

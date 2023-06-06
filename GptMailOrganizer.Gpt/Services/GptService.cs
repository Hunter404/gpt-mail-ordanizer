namespace GptMailOrganizer.Gpt.Services;

using System.Diagnostics.SymbolStore;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using Choice = Models.Choice;

public class GptService : IGptService
{
    private string _apiKey;

    public GptService(string apiKey)
    {
        _apiKey = apiKey;
    }

    public async Task<IReadOnlyList<Choice>> GenerateCompletionAsync(List<Message> prompts)
    {
        var auth = new OpenAIAuthentication(_apiKey);
        var api = new OpenAIClient(auth);

        var result = await api.ChatEndpoint.GetCompletionAsync(new ChatRequest(prompts, Model.GPT3_5_Turbo));

        return result.Choices
            .Select(
            x => new Choice
            {
                Message = x.Message,
                FinishReason = x.FinishReason,
            })
            .ToArray();
    }
}

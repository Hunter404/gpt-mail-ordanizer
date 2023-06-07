namespace GptMailOrganizer.Gpt.Services;

using Models;
using OpenAI;
using OpenAI.Chat;
using Choice = Models.Choice;

public class GptService : IGptService
{
    private readonly string _model;
    private readonly string _apiKey;

    public GptService(GptSettings gptSettings)
    {
        gptSettings.Validate();
        _model = gptSettings.Model;
        _apiKey = gptSettings.ApiKey;
    }

    public async Task<IReadOnlyList<Choice>> GenerateCompletionAsync(List<Message> prompts)
    {
        var auth = new OpenAIAuthentication(_apiKey);
        var api = new OpenAIClient(auth);

        var result = await api.ChatEndpoint.GetCompletionAsync(new ChatRequest(prompts, model: _model));

        return result.Choices
            .Select(x => new Choice(x.Message, x.FinishReason))
            .ToArray();
    }
}

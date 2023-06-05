namespace SpamFilter.GptApi.Services;

using Models;

public interface IGptService
{
    Task<GptResponse> GenerateCompletionAsync(string prompt);
}

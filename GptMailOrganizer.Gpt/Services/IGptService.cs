namespace SpamFilter.GptApi.Services;

using Models;
using OpenAI.Completions;
using Choice = Models.Choice;

public interface IGptService
{
    Task<IReadOnlyList<Choice>> GenerateCompletionAsync(string prompt);
}

namespace GptMailOrganizer.Gpt.Services;

using Choice = Models.Choice;

public interface IGptService
{
    Task<IReadOnlyList<Choice>> GenerateCompletionAsync(string prompt);
}

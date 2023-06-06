namespace GptMailOrganizer.Gpt.Services;

using OpenAI.Chat;
using Choice = Models.Choice;

public interface IGptService
{
    Task<IReadOnlyList<Choice>> GenerateCompletionAsync(List<Message> prompt);
}

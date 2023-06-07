namespace GptMailOrganizer.Gpt.Models;

public class Choice
{
    public Choice(string message, string finishReason)
    {
        Message = message;
        FinishReason = finishReason;
    }

    public string Message { get; }
    public string FinishReason { get; }
}

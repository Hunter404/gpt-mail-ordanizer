namespace SpamFilter.GptApi.Models;

using System.Text.Json.Serialization;

public class Choice
{
    public string Text { get; set; }
    public string FinishReason { get; set; }
}

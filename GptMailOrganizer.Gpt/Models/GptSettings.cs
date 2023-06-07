namespace GptMailOrganizer.Gpt.Models;

using System.Diagnostics.CodeAnalysis;

public class GptSettings
{
    public string? ApiKey { get; set; }
    public string? Model { get; set; }

    [MemberNotNull(nameof(ApiKey))]
    [MemberNotNull(nameof(Model))]
    public void Validate()
    {
        if (string.IsNullOrEmpty(Model))
        {
            throw new ArgumentNullException(nameof(Model), $"Argument {nameof(Model)} cannot be null or empty.");
        }

        if (string.IsNullOrEmpty(ApiKey))
        {
            throw new ArgumentNullException(nameof(ApiKey), $"Argument {nameof(ApiKey)} cannot be null or empty.");
        }
    }
}

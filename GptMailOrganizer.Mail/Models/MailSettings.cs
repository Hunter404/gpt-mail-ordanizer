namespace GptMailOrganizer.Mail.Models;

using System.Diagnostics.CodeAnalysis;

public class MailSettings
{
    public int MaxBatchSize { get; set; }
    public string[]? Categories { get; set; }

    [MemberNotNull(nameof(Categories))]
    public void Validate()
    {
        if (MaxBatchSize == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxBatchSize), $"Argument {nameof(MaxBatchSize)} cannot be 0.");
        }

        if (Categories == null)
        {
            throw new ArgumentNullException(nameof(Categories), $"Argument {nameof(Categories)} cannot be null.");
        }

        if (Categories.Length <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(Categories), $"Argument {nameof(Categories)} must have at least 2 categories to sort into.");
        }
    }
}

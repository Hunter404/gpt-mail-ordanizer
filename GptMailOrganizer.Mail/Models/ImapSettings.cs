namespace GptMailOrganizer.Mail.Models;

using System.Diagnostics.CodeAnalysis;

public class ImapSettings
{
    public string? Server { get; set; }
    public int Port { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? SecureSocketOptions { get; set; }

    [MemberNotNull(nameof(Server))]
    [MemberNotNull(nameof(Username))]
    [MemberNotNull(nameof(Password))]
    [MemberNotNull(nameof(SecureSocketOptions))]
    public void Validate()
    {
        if (string.IsNullOrEmpty(Server))
        {
            throw new ArgumentNullException(nameof(Server), $"Argument {nameof(Server)} cannot be null or empty.");
        }

        if (string.IsNullOrEmpty(Username))
        {
            throw new ArgumentNullException(nameof(Username), $"Argument {nameof(Username)} cannot be null or empty.");
        }

        if (string.IsNullOrEmpty(Password))
        {
            throw new ArgumentNullException(nameof(Password), $"Argument {nameof(Password)} cannot be null or empty.");
        }

        if (string.IsNullOrEmpty(SecureSocketOptions))
        {
            throw new ArgumentNullException(nameof(SecureSocketOptions), $"Argument {nameof(SecureSocketOptions)} cannot be null or empty.");
        }
    }
}

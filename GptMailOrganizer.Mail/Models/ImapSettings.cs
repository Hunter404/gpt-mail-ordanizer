namespace GptMailOrganizer.Models;

public class ImapSettings
{
    public string? Server { get; set; }
    public int Port { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? SecureSocketOptions { get; set; }
}

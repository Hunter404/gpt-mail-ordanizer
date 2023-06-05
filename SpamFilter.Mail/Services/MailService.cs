namespace SpamFilter.Mail.Services;

using GptApi.Services;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;

public class MailService : IMailService
{
    private IGptService _gptService;
    private string _imapTarget;
    private string _username;
    private string _password;

    public MailService(IGptService gptService, string imapTarget, string username, string password)
    {
        _gptService = gptService;
        _imapTarget = imapTarget;
        _username = username;
        _password = password;
    }

    public async Task RunAsync()
    {
        await Task.Run(Main);
    }

    private async void Main()
    {
        using var client = new ImapClient();
        var initialMessageCount = 0;

        while (true)
        {
            if (!client.IsConnected)
            {
                await client.ConnectAsync(_imapTarget, 993, SecureSocketOptions.SslOnConnect);
                await client.AuthenticateAsync(_username, _password);
            }

            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly);

            if (inbox.Count <= initialMessageCount)
            {
                await Task.Delay(60 * 1000); // Sleep for a minute.

                continue;
            }

            var folders = await client.GetFolder(client.PersonalNamespaces[0]).GetSubfoldersAsync(false);

            for (var i = initialMessageCount; i < inbox.Count; i++)
            {
                var message = await inbox.GetMessageAsync(i);
                var sender = message.Sender.ToString();
                var subject = message.Subject;

                var category = await CategorizeEmailAsync(sender, subject);

                var targetFolder = folders.FirstOrDefault(f => f.Name == category);
                if (targetFolder != null)
                {
                    await inbox.MoveToAsync(i, targetFolder);
                }
            }

            initialMessageCount = inbox.Count;
        }
    }

    private async Task<string> CategorizeEmailAsync(string sender, string subject)
    {
        var prompt = $"Sender: \"{sender}\". \nSubject: \"{subject}\". \n" +
            "Is this email best categorized as: 1) Personal, 2) Work, 3) Spam, 4) Newsletters, 5) Social, 6) Purchases, or 7) Other?";

        // Compose the request to the GPT-3 API
        var response = await _gptService.GenerateCompletionAsync(prompt);

        // Get the first response
        var category = response.Choices.First().Text.Trim();

        return category;
    }
}

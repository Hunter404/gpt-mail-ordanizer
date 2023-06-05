namespace GptMailOrganizer.Mail.Services;

using Gpt.Services;
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
        while (true)
        {
            try
            {
                await Task.Run(Main);
            }
            catch (Exception e)
            {
                // todo: catch exceptions earlier? Will do for now.
                Console.WriteLine(e);

                await Task.Delay(60 * 1000); // Restart in a minute in case we got rate throttled.
            }
        }
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
            await inbox.OpenAsync(FolderAccess.ReadWrite);

            if (inbox.Count <= initialMessageCount)
            {
                await Task.Delay(60 * 1000);

                continue;
            }

            var folders = await inbox.GetSubfoldersAsync(false);

            for (var i = initialMessageCount; i < inbox.Count; i++)
            {
                var message = await inbox.GetMessageAsync(i);
                var sender = message.From.ToString();
                var subject = message.Subject;

                var category = await CategorizeEmailAsync(sender, subject);

                var targetFolder = folders.FirstOrDefault(f => f.Name == category);
                if (targetFolder != null)
                {
                    Console.WriteLine($"Moved {subject} to {targetFolder}");
                    await inbox.MoveToAsync(i, targetFolder);
                }

                await Task.Delay(2000); // Prevent rate limits/throttling.
            }

            initialMessageCount = inbox.Count;
        }
    }

    private async Task<string> CategorizeEmailAsync(string sender, string subject)
    {
        var prompt = $"Sender: \"{sender}\". \nSubject: \"{subject}\". \n" +
            "Is this email best categorized as: Personal, Work, Spam, Newsletters, Social, Purchases, or Other?";

        // Compose the request to the GPT-3 API
        var response = await _gptService.GenerateCompletionAsync(prompt);

        // Get the first response
        var category = response.First().Text.Trim();

        return category;
    }
}

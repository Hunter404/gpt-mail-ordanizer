namespace GptMailOrganizer.Mail.Services;

using System.Text;
using Gpt.Services;
using MailKit;
using MailKit.Security;
using Models;
using OpenAI.Chat;
using ImapClient = MailKit.Net.Imap.ImapClient;

public class MailService : IMailService
{
    private readonly string _categoriesQuery;
    private readonly IGptService _gptService;
    private readonly ImapSettings _imapSettings;
    private readonly MailSettings _mailSettings;

    public MailService(IGptService gptService, ImapSettings imapSettings, MailSettings mailSettings)
    {
        _gptService = gptService;
        
        _imapSettings = imapSettings;
        _imapSettings.Validate();

        _mailSettings = mailSettings;
        _mailSettings.Validate();

        _categoriesQuery = string.Join(",", _mailSettings.Categories.Take(_mailSettings.Categories.Length - 1));
        _categoriesQuery += $",and {_mailSettings.Categories.Last()}.";
    }

    public async Task RunAsync() => await Task.Run(Main);

    private async Task ConnectAndAuthenticateAsync(ImapClient imapClient)
    {
        if (!imapClient.IsConnected)
        {
            if (!Enum.TryParse<SecureSocketOptions>(_imapSettings.SecureSocketOptions, out var secureSocketOptions))
                secureSocketOptions = SecureSocketOptions.SslOnConnect;

            await imapClient.ConnectAsync(_imapSettings.Server, _imapSettings.Port, secureSocketOptions);
        }

        if (!imapClient.IsAuthenticated)
        {
            await imapClient.AuthenticateAsync(_imapSettings.Username, _imapSettings.Password);
        }
    }

    private async void Main()
    {
        while (true)
        {
            try
            {
                using var client = new ImapClient();

                while (true)
                {
                    await ConnectAndAuthenticateAsync(client);

                    var inbox = client.Inbox;
                    await inbox.OpenAsync(FolderAccess.ReadWrite);

                    if (inbox.Count == 0)
                    {
                        await Task.Delay(60 * 1000);

                        continue;
                    }

                    var batchSize = Math.Min(inbox.Count, _mailSettings.MaxBatchSize);
                    var maxBatchSize = 50;
                    var emails = new List<EmailRequest>();

                    for (var i = 0; i < Math.Min(inbox.Count, maxBatchSize); i++)
                    {
                        // todo: this method of grabbing all mails is probably flawed in that the list gets shorter while the loop gets longer. Find a better solution.
                        var message = await inbox.GetMessageAsync(i);
                        var sender = message.From.ToString();
                        var subject = message.Subject;

                        emails.Add(new EmailRequest { Id = i, Sender = sender, Subject = subject });
                    }

                    var emailResponses = await CategorizeEmailChunkAsync(emails);
                    foreach (var item in emailResponses)
                    {
                        var targetFolder = folders.FirstOrDefault(f => f.Name == item.Category);
                        await inbox.MoveToAsync(item.Id, targetFolder);

                        Console.WriteLine($"Moved {item.Id} to {targetFolder}");
                    }

                    await Task.Delay(2000); // Prevent rate limits/throttling.
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                await Task.Delay(60 * 1000); // Restart in a minute in case we got rate throttled.
            }
        }
    }

    private async Task<List<EmailResponse>> CategorizeEmailChunkAsync(IReadOnlyList<EmailRequest> emails)
    {
        var system =
            $"Please categorize the following emails, each identified by a unique ID, into these categories:{_categoriesQuery}."
            + "Personal,"
            + "Work,"
            + "Spam,"
            + "Newsletters,"
            + "Social,"
            + "Purchases,"
            + "and Other."
            + "The format is ID,Sender,Subject."
            + "Reply with the ID and category, separated by a comma, no space.";

        var user = string.Join("\n", emails.Select(x => $"{x.Id},{x.Sender},{x.Subject}"));

        var prompt = new List<Message>()
        {
            new (Role.System, system),
            new (Role.User, user)
        };

        // Compose the request to the GPT-3 API
        var response = (await _gptService.GenerateCompletionAsync(prompt)).First();

        var ok = response.FinishReason?.ToLower() == "stop" && !string.IsNullOrEmpty(response.Message);
        if (!ok)
        {
            throw new Exception($"Unknown exception occured: {response.FinishReason}, {response.Message}");
        }

        return response.Message!
            .Split('\n', StringSplitOptions.TrimEntries) // Assume every item is a new line.
            .Select(line => line.Split(',', StringSplitOptions.TrimEntries)) // Assume every item is id:category.
            .Select(split => new EmailResponse() { Id = int.Parse(split[0]), Category = split[1] })
            .ToList();
    }
}

internal class EmailRequest
{
    public int Id { get; set; }
    public string Subject { get; set; }
    public string Sender { get; set; }
}

internal class EmailResponse
{
    public int Id { get; set; }

    public string Category { get; set; }
}

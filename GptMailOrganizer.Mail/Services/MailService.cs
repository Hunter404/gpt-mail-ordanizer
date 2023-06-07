namespace GptMailOrganizer.Mail.Services;

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
                    var emails = new List<EmailRequest>();

                    // Subtract one since range of 0 to 50 is 51.
                    var messages = await inbox.FetchAsync(0, batchSize - 1, MessageSummaryItems.UniqueId);
                    foreach (var message in messages)
                    {
                        var mail = await inbox.GetMessageAsync(message.UniqueId);
                        var sender = mail.From.ToString();
                        var subject = mail.Subject;

                        emails.Add(new EmailRequest(id: message.UniqueId.Id, sender: sender, subject: subject));
                    }

                    var emailResponses = await CategorizeEmailChunkAsync(emails);
                    var folders = await inbox.GetSubfoldersAsync();

                    foreach (var item in emailResponses)
                    {
                        var targetFolder = folders.FirstOrDefault(f => f.Name == item.Category);
                        await inbox.MoveToAsync(new UniqueId(item.Id), targetFolder);

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
            .Select(split => new EmailResponse(id: uint.Parse(split[0]), category: split[1]))
            .ToList();
    }
}

internal class EmailRequest
{
    public EmailRequest(uint id, string sender, string subject)
    {
        Id = id;
        Sender = sender;
        Subject = subject;
    }

    public uint Id { get; }
    public string Subject { get; }
    public string Sender { get; }
}

internal class EmailResponse
{
    public EmailResponse(uint id, string category)
    {
        Id = id;
        Category = category;
    }

    public uint Id { get; }

    public string Category { get; }
}

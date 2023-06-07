using GptMailOrganizer.Gpt.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using GptMailOrganizer.Gpt.Services;
using GptMailOrganizer.Mail.Models;
using GptMailOrganizer.Mail.Services;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(
        (hostingContext, configuration) =>
        {
            configuration.Sources.Clear();

            var env = hostingContext.HostingEnvironment;

            configuration
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", false, true)
                .Build();
        })
    .ConfigureServices(
        (hostingContext, services) =>
        {
            var gptSettings = hostingContext.Configuration
                .GetSection("OpenAi")
                .Get<GptSettings>() ?? throw new FormatException("Couldn't deserialize OpenAi settings.");
            services.AddSingleton<IGptService, GptService>(_ => new GptService(gptSettings));

            var imapSettings = hostingContext.Configuration
                .GetSection("Imap")
                .Get<ImapSettings>() ?? throw new FormatException("Couldn't deserialize Imap settings.");
            services.AddSingleton(imapSettings);

            var mailSettings = hostingContext.Configuration
                .GetSection("Mail")
                .Get<MailSettings>() ?? throw new FormatException("Couldn't deserialize Mail settings.");
            services.AddSingleton(mailSettings);

            services.AddSingleton<IMailService, MailService>();
        })
        .Build();

var mailService = host.Services.GetRequiredService<IMailService>();
await mailService.RunAsync();

await host.RunAsync();

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using GptMailOrganizer.Gpt.Services;
using GptMailOrganizer.Mail.Services;
using GptMailOrganizer.Models;

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
            var apiKey = hostingContext.Configuration["OpenAI:ApiKey"]
                         ?? throw new InvalidOperationException("Missing 'OpenAI:ApiKey' in configuration.");
            services.AddSingleton<IGptService, GptService>(_ => new GptService(apiKey));

            var imapSettings = hostingContext.Configuration
                .GetSection("Imap")
                .Get<ImapSettings>();

            services.AddSingleton<ImapSettings>(imapSettings);
            services.AddSingleton<IMailService, MailService>();
        })
        .Build();

var mailService = host.Services.GetRequiredService<IMailService>();
await mailService.RunAsync();

await host.RunAsync();

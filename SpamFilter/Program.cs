using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using SpamFilter.Mail.Services;
using SpamFilter.GptApi.Services;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(
        (hostingContext, configuration) =>
        {
            configuration.Sources.Clear();

            var env = hostingContext.HostingEnvironment;

            configuration
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
                .Build();
        })
    .ConfigureServices(
        (hostingContext, services) =>
        {
            var apiKey = hostingContext.Configuration["OpenAI:ApiKey"]
                         ?? throw new InvalidOperationException("Missing 'OpenAI:ApiKey' in configuration.");
            services.AddSingleton<IGptService, GptService>(_ => new GptService(apiKey));

            var mailServer = hostingContext.Configuration["IMAP:Host"]
                             ?? throw new InvalidOperationException("Missing 'IMAP:Host' in configuration.");
            var mailUsername = hostingContext.Configuration["IMAP:Username"]
                               ?? throw new InvalidOperationException("Missing 'IMAP:Username' in configuration.");
            var mailPassword = hostingContext.Configuration["IMAP:Password"]
                               ?? throw new InvalidOperationException("Missing 'IMAP:Password' in configuration.");

            services.AddSingleton<IMailService, MailService>(
                serviceProvider => new MailService(serviceProvider.GetRequiredService<IGptService>(),mailServer, mailUsername, mailPassword));
        })
        .Build();

var mailService = host.Services.GetRequiredService<IMailService>();
await mailService.RunAsync();

await host.RunAsync();

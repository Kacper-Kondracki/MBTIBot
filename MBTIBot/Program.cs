using System.Globalization;
using EFCoreSecondLevelCacheInterceptor;
using MBTIBot;
using MBTIBot.Commands;
using MBTIBot.Responders;
using MBTIBot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Remora.Commands.Extensions;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Extensions;
using Remora.Discord.Hosting.Extensions;
using Serilog;
using Serilog.Events;

CultureInfo.CurrentCulture = new CultureInfo("pl-PL");
CultureInfo.CurrentUICulture = new CultureInfo("pl-PL");

await Host.CreateDefaultBuilder()
    .UseSerilog((_, logger) =>
    {
        logger.MinimumLevel.Override("System", LogEventLevel.Warning);
        logger.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
        logger.MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning);
        logger.WriteTo.Console();
    })
    .AddDiscordService(services =>
    {
        var options = services.GetRequiredService<IOptions<Secrets>>();
        if (string.IsNullOrWhiteSpace(options.Value.DiscordToken))
            throw new InvalidOperationException(
                "No bot toke has been provided. Set the DISCORD_TOKEN environment variable to a valid token.");

        return options.Value.DiscordToken;
    })
    .ConfigureServices((builder, services) =>
    {
        services.Configure<Secrets>(builder.Configuration.GetSection(nameof(Secrets)));
        services.Configure<BotOptions>(builder.Configuration.GetSection(nameof(BotOptions)));

        services.AddEFSecondLevelCache(options => options.UseMemoryCacheProvider());
        services.AddDbContext<BotContext>((serviceProvider, options) =>
            options.UseSqlite("Data Source=MBTIBotDatabase.db")
                .AddInterceptors(serviceProvider.GetRequiredService<SecondLevelCacheInterceptor>()));

        services.Configure<DiscordGatewayClientOptions>(g => g.Intents |= GatewayIntents.MessageContents);
        services.AddHostedService<SlashUpdateService>();
        services.AddDiscordCommands(true, false);
        services.AddCommandTree()
            .WithCommandGroup<IntroductionCommands>();
        services.AddPostExecutionEvent<PostExecutor>();
        services.AddResponder<IntroductionCreatedResponder>();
        services.AddResponder<IntroductionDeletedResponder>();
    })
    .RunConsoleAsync();
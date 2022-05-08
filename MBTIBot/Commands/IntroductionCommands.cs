using System.ComponentModel;
using EFCoreSecondLevelCacheInterceptor;
using MBTIBot.Extensions;
using MBTIBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Rest.Core;
using Remora.Results;

namespace MBTIBot.Commands;

[Group("przedstaw-się")]
[DiscordDefaultMemberPermissions(DiscordPermission.ManageRoles)]
public class IntroductionCommands : CommandGroup
{
    private readonly ICommandContext _commandContext;
    private readonly BotContext _context;
    private readonly IDiscordRestChannelAPI _discordRestChannelAPI;
    private readonly FeedbackService _feedbackService;
    private readonly ILogger<IntroductionCommands> _logger;

    public IntroductionCommands(
        ILogger<IntroductionCommands> logger,
        BotContext context,
        FeedbackService feedbackService,
        ICommandContext commandContext,
        IDiscordRestChannelAPI discordRestChannelAPI)
    {
        _logger = logger;
        _context = context;
        _feedbackService = feedbackService;
        _commandContext = commandContext;
        _discordRestChannelAPI = discordRestChannelAPI;
    }

    [Command("ustaw")]
    [Description("Ustawia kanał przedstaw się.")]
    public async Task<IResult> SetIntroductionChannel(
        [Option("kanał")] [Description("Kanał przedstaw się.")] [ChannelTypes(ChannelType.GuildText)]
        IChannel channel)
    {
        if (!_commandContext.GuildID.IsDefined(out var guildId)) return Result.FromSuccess();

        var settings = await _context.GuildSettings
            .FirstOrDefaultAsync(x => x.GuildId == guildId.Value, CancellationToken);

        if (settings is null)
        {
            settings = new GuildSettings { GuildId = guildId.Value };
            _context.GuildSettings.Add(settings);
        }

        settings.IntroductionChannel = channel.ID.Value;

        await _context.SaveChangesAsync(CancellationToken);
        _logger.LogInformation("Introduction channel set to {Channel} on guild {Guild}", channel.ID.Value,
            guildId.Value);

        if (!channel.Name.IsDefined(out var channelName)) channelName = null;

        var result = await _feedbackService.SendEmbedSuccess("Ustawiono kanał", channelName, CancellationToken);

        return !result.IsSuccess ? Result.FromError(result) : Result.FromSuccess();
    }

    [Command("resetuj")]
    [Description("Resetuje ustawiony kanał przedstaw się.")]
    public async Task<IResult> ResetIntroductionChannel()
    {
        if (!_commandContext.GuildID.IsDefined(out var guildId)) return Result.FromSuccess();

        var settings = await _context.GuildSettings
            .FirstOrDefaultAsync(x => x.GuildId == guildId.Value, CancellationToken);

        if (settings is null)
        {
            settings = new GuildSettings { GuildId = guildId.Value };
            _context.GuildSettings.Add(settings);
        }

        settings.IntroductionChannel = null;

        await _context.SaveChangesAsync(CancellationToken);
        _logger.LogInformation("Introduction channel reseted on guild {Guild}", guildId.Value);

        var result = await _feedbackService.SendEmbedSuccess("Zresetowano ustawiony kanał", ct: CancellationToken);

        return !result.IsSuccess ? Result.FromError(result) : Result.FromSuccess();
    }

    [Command("pokaż")]
    [Description("Pokazuje ustawiony kanał przedstaw się.")]
    public async Task<IResult> ShowIntroductionChannel()
    {
        Result<IMessage> result;
        if (!_commandContext.GuildID.IsDefined(out var guildId)) return Result.FromSuccess();

        var settings = await _context.GuildSettings
            .Cacheable()
            .FirstOrDefaultAsync(x => x.GuildId == guildId.Value, CancellationToken);

        if (settings?.IntroductionChannel is null)
        {
            result = await _feedbackService.SendEmbedWarning("Kanał nie jest ustawiony", ct: CancellationToken);

            return !result.IsSuccess ? Result.FromError(result) : Result.FromSuccess();
        }

        var channelResult = await _discordRestChannelAPI
            .GetChannelAsync(new Snowflake(settings.IntroductionChannel.Value), CancellationToken);

        if (!channelResult.IsSuccess)
        {
            result = await _feedbackService.SendEmbedError("Nie można uzyskać kanału", ct: CancellationToken);

            return !result.IsSuccess ? Result.FromError(result) : Result.FromSuccess();
        }

        var channel = channelResult.Entity;

        _logger.LogInformation("Introduction channel {Channel} showed on guild {Guild}", channel.ID.Value,
            guildId.Value);

        if (!channel.Name.IsDefined(out var channelName)) channelName = null;

        result = await _feedbackService.SendEmbedSuccess("Ustawiony kanał", channelName, CancellationToken);

        return !result.IsSuccess ? Result.FromError(result) : Result.FromSuccess();
    }
}
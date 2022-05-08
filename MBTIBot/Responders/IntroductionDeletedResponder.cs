using EFCoreSecondLevelCacheInterceptor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Gateway.Responders;
using Remora.Rest.Core;
using Remora.Results;

namespace MBTIBot.Responders;

public class IntroductionDeletedResponder : IResponder<IMessageDelete>
{
    private readonly BotContext _context;
    private readonly IDiscordRestChannelAPI _discordRestChannelAPI;
    private readonly ILogger<IntroductionDeletedResponder> _logger;

    public IntroductionDeletedResponder(
        ILogger<IntroductionDeletedResponder> logger,
        BotContext context,
        IDiscordRestChannelAPI discordRestChannelAPI)
    {
        _logger = logger;
        _context = context;
        _discordRestChannelAPI = discordRestChannelAPI;
    }

    public async Task<Result> RespondAsync(IMessageDelete gatewayEvent, CancellationToken ct = new())
    {
        if (!gatewayEvent.GuildID.IsDefined(out var guildId)) return Result.FromSuccess();

        var settings = await _context.GuildSettings
            .Cacheable()
            .FirstOrDefaultAsync(x => x.GuildId == guildId.Value, ct);

        if (settings?.IntroductionChannel is null) return Result.FromSuccess();

        if (gatewayEvent.ChannelID.Value != settings.IntroductionChannel) return Result.FromSuccess();

        var trackedMessage = await _context.TrackedMessages
            .Cacheable()
            .FirstOrDefaultAsync(x => x.MessageId == gatewayEvent.ID.Value, ct);

        if (trackedMessage is null) return Result.FromSuccess();

        var editChannelPermissionsResult = await _discordRestChannelAPI.EditChannelPermissionsAsync(
            gatewayEvent.ChannelID,
            new Snowflake(trackedMessage.Author), deny: DiscordPermissionSet.Empty,
            type: PermissionOverwriteType.Member, ct: ct);

        if (!editChannelPermissionsResult.IsSuccess) return Result.FromSuccess();

        _context.TrackedMessages.Remove(trackedMessage);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Granted user {User} permission to send messages on channel {Channel} on guild {Guild}",
            trackedMessage.Author, gatewayEvent.ChannelID.Value, guildId.Value);

        return Result.FromSuccess();
    }
}
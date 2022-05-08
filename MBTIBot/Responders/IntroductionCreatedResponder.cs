using EFCoreSecondLevelCacheInterceptor;
using MBTIBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace MBTIBot.Responders;

public class IntroductionCreatedResponder : IResponder<IMessageCreate>
{
    private readonly BotContext _context;
    private readonly IDiscordRestChannelAPI _discordRestChannelAPI;
    private readonly IDiscordRestUserAPI _discordRestUserAPI;
    private readonly ILogger<IntroductionCreatedResponder> _logger;

    public IntroductionCreatedResponder(
        ILogger<IntroductionCreatedResponder> logger,
        BotContext context,
        IDiscordRestChannelAPI discordRestChannelAPI,
        IDiscordRestUserAPI discordRestUserAPI)
    {
        _logger = logger;
        _context = context;
        _discordRestChannelAPI = discordRestChannelAPI;
        _discordRestUserAPI = discordRestUserAPI;
    }

    public async Task<Result> RespondAsync(IMessageCreate gatewayEvent, CancellationToken ct = new())
    {
        if (!gatewayEvent.GuildID.IsDefined(out var guildId)) return Result.FromSuccess();

        if ((gatewayEvent.Author.IsBot.IsDefined(out var isBot) && isBot) ||
            (gatewayEvent.Author.IsSystem.IsDefined(out var isSystem) && isSystem))
            return Result.FromSuccess();

        if (!gatewayEvent.Member.IsDefined()) return Result.FromSuccess();

        var settings = await _context.GuildSettings
            .Cacheable()
            .FirstOrDefaultAsync(x => x.GuildId == guildId.Value, ct);

        if (settings?.IntroductionChannel is null) return Result.FromSuccess();

        if (gatewayEvent.ChannelID.Value != settings.IntroductionChannel) return Result.FromSuccess();


        var editChannelPermissionsResult = await _discordRestChannelAPI.EditChannelPermissionsAsync(
            gatewayEvent.ChannelID,
            gatewayEvent.Author.ID, deny: new DiscordPermissionSet(DiscordPermission.SendMessages),
            type: PermissionOverwriteType.Member, ct: ct);

        if (!editChannelPermissionsResult.IsSuccess) return Result.FromSuccess();

        _context.TrackedMessages.Add(new TrackedMessage
            { MessageId = gatewayEvent.ID.Value, Author = gatewayEvent.Author.ID.Value, Guild = guildId.Value });

        await _context.SaveChangesAsync(ct);

        if (await _context.RemindedUsers
                .Cacheable()
                .FirstOrDefaultAsync(x => x.UserId == gatewayEvent.Author.ID.Value, ct) is null)
        {
            var createDMResult = await _discordRestUserAPI.CreateDMAsync(gatewayEvent.Author.ID, ct);
            if (createDMResult.IsSuccess)
            {
                var dmChannel = createDMResult.Entity;
                var createMessageResult = await _discordRestChannelAPI.CreateMessageAsync(dmChannel.ID,
                    "Dziękujemy za przedstawienie się. Pamiętaj, ten kanał nie służy do rozmów! Aby uzyskać ponownie uprawnienia do pisania na kanale, usuń swoją poprzednią wiadomość.",
                    ct: ct);

                if (createMessageResult.IsSuccess)
                {
                    _context.RemindedUsers.Add(new RemindedUsers { UserId = gatewayEvent.Author.ID.Value });

                    await _context.SaveChangesAsync(ct);
                }
            }
        }

        _logger.LogInformation("Revoked user {User} permission to send messages on channel {Channel} on guild {Guild}",
            gatewayEvent.Author.ID.Value, gatewayEvent.ChannelID.Value, guildId.Value);

        return Result.FromSuccess();
    }
}
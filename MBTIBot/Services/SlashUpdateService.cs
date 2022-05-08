using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Discord.API;
using Remora.Discord.Commands.Services;
using Remora.Rest.Core;

namespace MBTIBot.Services;

public class SlashUpdateService : BackgroundService
{
    private readonly string _debugServer;
    private readonly ILogger<SlashUpdateService> _logger;
    private readonly SlashService _slashService;

    public SlashUpdateService(ILogger<SlashUpdateService> logger, IOptions<BotOptions> options,
        SlashService slashService)
    {
        _logger = logger;
        _slashService = slashService;
        _debugServer = options.Value.DebugServer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var checkSlashSupport = _slashService.SupportsSlashCommands();
        if (!checkSlashSupport.IsSuccess)
        {
            _logger.LogWarning("The registered commands of the bot don't support slash commands: {Reason}",
                checkSlashSupport.Error?.Message);
            return;
        }

        await UpdateGuildSlash(_debugServer, stoppingToken);
    }

    public async Task UpdateGuildSlash(Snowflake guild, CancellationToken stoppingToken)
    {
        var updateSlash = await _slashService.UpdateSlashCommandsAsync(guild, ct: stoppingToken);

        if (!updateSlash.IsSuccess)
        {
            _logger.LogWarning("Failed to update slash commands: {Reason}", updateSlash.Error?.Message);
            _logger.LogWarning("Inner: {@Reason}", updateSlash.Inner);
        }
    }

    private async Task UpdateGuildSlash(string guild, CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(guild))
        {
            _logger.LogWarning("The guild string is empty");
            return;
        }

        if (!DiscordSnowflake.TryParse(_debugServer, out var debugServerSnowflake))
        {
            _logger.LogWarning("Failed to parse guild");
            return;
        }

        await UpdateGuildSlash((Snowflake)debugServerSnowflake, stoppingToken);
    }
}
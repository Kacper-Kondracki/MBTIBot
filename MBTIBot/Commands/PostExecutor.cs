using MBTIBot.Extensions;
using Microsoft.Extensions.Logging;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Commands.Services;
using Remora.Results;

namespace MBTIBot.Commands;

public class PostExecutor : IPostExecutionEvent
{
    private readonly FeedbackService _feedbackService;
    private readonly ILogger<PostExecutor> _logger;

    public PostExecutor(ILogger<PostExecutor> logger, FeedbackService feedbackService)
    {
        _logger = logger;
        _feedbackService = feedbackService;
    }

    public async Task<Result> AfterExecutionAsync(ICommandContext context, IResult commandResult,
        CancellationToken ct = new())
    {
        if (commandResult.IsSuccess ||
            context is not InteractionContext interactionContext ||
            !interactionContext.Data.Name.IsDefined(out var commandName))
            return Result.FromSuccess();

        _logger.LogWarning("Command {Command} failed with error {Error}", commandName,
            commandResult.Error?.Message);

        await _feedbackService.SendEmbedError("Błąd", commandResult.Error?.Message, ct);

        return Result.FromSuccess();
    }
}
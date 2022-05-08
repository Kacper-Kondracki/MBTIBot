using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Rest.Core;
using Remora.Results;

namespace MBTIBot.Extensions;

public static class FeedbackServiceExtensions
{
    public static async Task<Result<IMessage>> SendEmbedSuccess(this FeedbackService feedbackService, string title,
        string? description = null, CancellationToken ct = new())
    {
        return await feedbackService.SendContextualEmbedAsync(new Embed(title,
            Description: description ?? new Optional<string>(),
            Colour: feedbackService.Theme.Success), ct: ct);
    }

    public static async Task<Result<IMessage>> SendEmbedWarning(this FeedbackService feedbackService, string title,
        string? description = null, CancellationToken ct = new())
    {
        return await feedbackService.SendContextualEmbedAsync(new Embed(title,
            Description: description ?? new Optional<string>(),
            Colour: feedbackService.Theme.Warning), ct: ct);
    }

    public static async Task<Result<IMessage>> SendEmbedError(this FeedbackService feedbackService, string title,
        string? description = null, CancellationToken ct = new())
    {
        return await feedbackService.SendContextualEmbedAsync(new Embed(title,
            Description: description ?? new Optional<string>(),
            Colour: feedbackService.Theme.FaultOrDanger), ct: ct);
    }
}
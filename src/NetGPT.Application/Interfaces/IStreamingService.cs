namespace NetGPT.Application.Interfaces;

public interface IStreamingService
{
    Task StreamResponseAsync(
        Guid conversationId,
        Guid messageId,
        IAsyncEnumerable<string> chunks,
        CancellationToken cancellationToken = default);
}

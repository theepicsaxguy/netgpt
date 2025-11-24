
namespace NetGPT.Application.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IStreamingService
    {
        Task StreamResponseAsync(
            Guid conversationId,
            Guid messageId,
            IAsyncEnumerable<string> chunks,
            CancellationToken cancellationToken = default);
    }
}

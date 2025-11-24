// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NetGPT.Application.Interfaces
{
    public interface IStreamingService
    {
        Task StreamResponseAsync(
            Guid conversationId,
            Guid messageId,
            IAsyncEnumerable<string> chunks,
            CancellationToken cancellationToken = default);
    }
}

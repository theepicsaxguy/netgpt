// Copyright (c) 2025 NetGPT. All rights reserved.

using Microsoft.Extensions.AI;

namespace NetGPT.Infrastructure.Agents
{
    public interface IOpenAIClientFactory
    {
        IChatClient CreateChatClient(string? model = null);
    }
}
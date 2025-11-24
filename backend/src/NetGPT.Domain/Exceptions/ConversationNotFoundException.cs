// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Domain.Exceptions
{
    public sealed class ConversationNotFoundException(object conversationId) : NotFoundException("Conversation", conversationId)
    {
    }
}

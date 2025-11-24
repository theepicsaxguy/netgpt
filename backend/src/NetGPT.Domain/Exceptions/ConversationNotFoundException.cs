// <copyright file="ConversationNotFoundException.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Domain.Exceptions
{
    public sealed class ConversationNotFoundException(object conversationId) : NotFoundException("Conversation", conversationId)
    {
    }
}

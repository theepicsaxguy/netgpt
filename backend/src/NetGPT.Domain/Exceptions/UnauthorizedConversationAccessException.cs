// <copyright file="UnauthorizedConversationAccessException.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Domain.Exceptions
{
    public sealed class UnauthorizedConversationAccessException(object conversationId, object userId) : DomainException($"User {userId} is not authorized to access conversation {conversationId}")
    {
    }
}

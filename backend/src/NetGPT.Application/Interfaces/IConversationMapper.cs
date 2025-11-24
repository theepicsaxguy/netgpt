// <copyright file="IConversationMapper.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Application.Interfaces
{
    using NetGPT.Application.DTOs;
    using NetGPT.Domain.Aggregates;

    public interface IConversationMapper
    {
        ConversationResponse ToResponse(Conversation conversation);

        MessageResponse ToMessageResponse(Message message);
    }
}

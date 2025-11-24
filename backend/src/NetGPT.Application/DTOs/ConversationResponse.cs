// <copyright file="ConversationResponse.cs" theepicsaxguy">
// \
// </copyright>

namespace NetGPT.Application.DTOs
{
    using System;

    public record ConversationResponse(
        Guid Id,
        string Title,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        int MessageCount);
}

// <copyright file="ConversationDto.cs" theepicsaxguy">
// \
// </copyright>

namespace NetGPT.Application.DTOs
{
    using System;

    public record ConversationDto(
        Guid Id,
        string Title,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        int MessageCount);
}

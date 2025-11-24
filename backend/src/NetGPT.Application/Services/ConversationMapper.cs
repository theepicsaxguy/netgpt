
namespace NetGPT.Application.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using NetGPT.Application.DTOs;
    using NetGPT.Application.Interfaces;
    using NetGPT.Domain.Aggregates;

    public sealed class ConversationMapper : IConversationMapper
    {
        public ConversationResponse ToResponse(Conversation conversation)
        {
            return new ConversationResponse(
                conversation.Id.Value,
                conversation.Title,
                conversation.CreatedAt,
                conversation.UpdatedAt,
                conversation.Messages.Count);
        }

        public MessageResponse ToMessageResponse(Message message)
        {
            List<AttachmentDto> attachments = [.. message.Content.Attachments.Select(a => new AttachmentDto(a.FileName, a.ContentType, a.SizeBytes, a.StorageKey))];

            MessageMetadataDto? metadata = null;
            if (message.Metadata != null)
            {
                List<ToolInvocationDto>? toolInvocations = message.Metadata.ToolInvocations?
                    .Select(t => new ToolInvocationDto(
                        t.ToolName,
                        t.Arguments,
                        t.Result,
                        t.InvokedAt,
                        t.Duration.TotalMilliseconds))
                    .ToList();

                metadata = new MessageMetadataDto(
                    toolInvocations,
                    message.Metadata.AgentName,
                    message.Metadata.TokenCount);
            }

            return new MessageResponse(
                message.Id.Value,
                message.Role.ToString(),
                message.Content.Text,
                message.CreatedAt,
                attachments,
                metadata);
        }
    }
}

using NetGPT.Application.DTOs;
using NetGPT.Application.Interfaces;
using NetGPT.Domain.Aggregates.ConversationAggregate;

namespace NetGPT.Application.Mappings;

public sealed class ConversationMapper : IConversationMapper
{
    public ConversationResponse ToResponse(Conversation conversation)
    {
        return new ConversationResponse(
            conversation.Id,
            conversation.Title,
            conversation.Status.ToString(),
            conversation.CreatedAt,
            conversation.UpdatedAt,
            conversation.Messages.Count,
            conversation.TokensUsed,
            new AgentConfigurationDto(
                conversation.AgentConfiguration.ModelName,
                conversation.AgentConfiguration.Temperature,
                conversation.AgentConfiguration.MaxTokens,
                conversation.AgentConfiguration.TopP,
                conversation.AgentConfiguration.FrequencyPenalty,
                conversation.AgentConfiguration.PresencePenalty));
    }

    public MessageResponse ToMessageResponse(Message message)
    {
        return new MessageResponse(
            message.Id,
            message.Content.Role.ToString(),
            message.Content.Text,
            message.Content.Attachments?.Select(a => new FileAttachmentDto(
                a.FileName,
                a.FileUrl,
                a.ContentType,
                a.FileSizeBytes)).ToList(),
            message.Metadata != null
                ? new MessageMetadataDto(
                    message.Metadata.TokenCount,
                    message.Metadata.ResponseTime.TotalMilliseconds,
                    message.Metadata.ModelUsed,
                    message.Metadata.ToolsInvoked)
                : null,
            message.CreatedAt);
    }
}

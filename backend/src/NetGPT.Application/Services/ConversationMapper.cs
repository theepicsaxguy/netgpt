using System.Linq;
using NetGPT.Application.DTOs;
using NetGPT.Application.Interfaces;
using NetGPT.Domain.Aggregates;
using NetGPT.Domain.Primitives;

namespace NetGPT.Application.Services;

public sealed class ConversationMapper : IConversationMapper
{
    public Result<ConversationResponse> ToResponse(Conversation conversation)
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

    public Result<MessageResponse> ToMessageResponse(Message message)
    {
        var attachments = message.Content.Attachments?
            .Select(a => new FileAttachmentDto(a.FileName, a.FileUrl, a.ContentType, a.FileSizeBytes))
            .ToList();

        MessageMetadataDto? metadata = null;
        if (message.Metadata != null)
        {
            metadata = new MessageMetadataDto(
                message.Metadata.TokenCount,
                message.Metadata.ResponseTime.TotalMilliseconds,
                message.Metadata.ModelUsed,
                message.Metadata.ToolsInvoked);
        }

        return new MessageResponse(
            message.Id,
            message.Content.Role.ToString(),
            message.Content.Text,
            attachments,
            metadata,
            message.CreatedAt);
    }
}

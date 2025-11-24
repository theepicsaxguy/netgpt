
#pragma warning disable CS8618 // Non-nullable properties not initialized in constructor (EF Core)

namespace NetGPT.Domain.Aggregates
{
    using System;
    using NetGPT.Domain.Enums;
    using NetGPT.Domain.ValueObjects;

    public sealed class Message
    {
        public MessageId Id { get; private set; }

        public ConversationId ConversationId { get; private set; }

        public MessageRole Role { get; private set; }

        public MessageContent Content { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public MessageId? ParentMessageId { get; private set; }

        public MessageMetadata? Metadata { get; private set; }

        private Message()
        {
        } // EF Core

        internal static Message Create(
            ConversationId conversationId,
            MessageRole role,
            MessageContent content,
            MessageId? parentMessageId = null)
        {
            return new Message
            {
                Id = MessageId.CreateNew(),
                ConversationId = conversationId,
                Role = role,
                Content = content ?? throw new ArgumentNullException(nameof(content)),
                CreatedAt = DateTime.UtcNow,
                ParentMessageId = parentMessageId,
            };
        }

        public void AddMetadata(MessageMetadata metadata)
        {
            this.Metadata = metadata;
        }
    }
}

// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using NetGPT.Domain.Enums;
using NetGPT.Domain.ValueObjects;

#pragma warning disable CS8618 // Non-nullable properties not initialized in constructor (EF Core)

namespace NetGPT.Domain.Aggregates
{
    public sealed class Message
    {
        private Message()
        {
        } // EF Core

        public MessageId Id { get; private set; }

        public ConversationId ConversationId { get; private set; }

        public MessageRole Role { get; private set; }

        public MessageContent Content { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public MessageId? ParentMessageId { get; private set; }

        public MessageMetadata? Metadata { get; private set; }

        public void AddMetadata(MessageMetadata metadata)
        {
            Metadata = metadata;
        }

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
    }
}

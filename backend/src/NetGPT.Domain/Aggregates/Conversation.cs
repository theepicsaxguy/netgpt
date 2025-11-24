// Copyright (c) 2025 NetGPT. All rights reserved.

#pragma warning disable CS8618 // Non-nullable properties not initialized in constructor (EF Core)

using System;
using System.Collections.Generic;
using System.Linq;
using NetGPT.Domain.Enums;
using NetGPT.Domain.Events;
using NetGPT.Domain.Exceptions;
using NetGPT.Domain.ValueObjects;

namespace NetGPT.Domain.Aggregates
{
    public sealed class Conversation
    {
        private readonly List<Message> messages = [];
        private readonly List<IDomainEvent> domainEvents = [];

        public ConversationId Id { get; private set; }

        public UserId UserId { get; private set; }

        public string Title { get; private set; }

        public ConversationStatus Status { get; private set; }

        public int TokensUsed { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        public ConversationMetadata Metadata { get; private set; }

        public AgentConfiguration AgentConfiguration { get; private set; }

        public IReadOnlyList<Message> Messages => this.messages.AsReadOnly();

        public IReadOnlyList<IDomainEvent> DomainEvents => this.domainEvents.AsReadOnly();

        private Conversation()
        {
        } // EF Core

        public static Conversation Create(UserId userId, string? title = null, AgentConfiguration? agentConfig = null)
        {
            Conversation conversation = new()
            {
                Id = ConversationId.CreateNew(),
                UserId = userId ?? throw new ArgumentNullException(nameof(userId)),
                Title = title ?? "New Conversation",
                Status = ConversationStatus.Active,
                TokensUsed = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Metadata = ConversationMetadata.Default(),
                AgentConfiguration = agentConfig ?? AgentConfiguration.Default(),
            };

            conversation.AddDomainEvent(new ConversationCreatedEvent(conversation.Id, userId));
            return conversation;
        }

        public MessageId AddMessage(MessageRole role, MessageContent content)
        {
            Message message = Message.Create(
                this.Id,
                role,
                content,
                this.messages.LastOrDefault()?.Id);

            this.messages.Add(message);
            this.UpdatedAt = DateTime.UtcNow;

            this.AddDomainEvent(new MessageAddedEvent(this.Id, message.Id, role));
            return message.Id;
        }

        public void UpdateTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new DomainException("Title cannot be empty");
            }

            this.Title = title;
            this.UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateMetadata(ConversationMetadata metadata)
        {
            this.Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            this.UpdatedAt = DateTime.UtcNow;
        }

        public Message GetMessage(MessageId messageId)
        {
            return this.messages.FirstOrDefault(m => m.Id == messageId)
                ?? throw new DomainException($"Message {messageId} not found");
        }

        public void ClearDomainEvents()
        {
            this.domainEvents.Clear();
        }

        private void AddDomainEvent(IDomainEvent domainEvent)
        {
            this.domainEvents.Add(domainEvent);
        }

        public void EnsureOwnership(UserId userId)
        {
            if (this.UserId != userId)
            {
                throw new UnauthorizedAccessException("User does not own this conversation");
            }
        }

        public void AddTokens(int tokens)
        {
            this.TokensUsed += tokens;
            this.UpdatedAt = DateTime.UtcNow;
        }

        public void Archive()
        {
            this.Status = ConversationStatus.Archived;
            this.UpdatedAt = DateTime.UtcNow;
        }
    }
}

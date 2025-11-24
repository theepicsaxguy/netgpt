// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetGPT.Domain.Aggregates;
using NetGPT.Domain.Interfaces;
using NetGPT.Domain.ValueObjects;

namespace NetGPT.Infrastructure.Persistence.Repositories
{
    public sealed class ConversationRepository(ApplicationDbContext context) : IConversationRepository
    {
        private readonly ApplicationDbContext context = context;

        public async Task<Conversation?> GetByIdAsync(ConversationId id, CancellationToken ct = default)
        {
            return await context.Conversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == id, ct);
        }

        public async Task<List<Conversation>> GetByUserIdAsync(UserId userId, int skip, int take, CancellationToken ct = default)
        {
            return await context.Conversations
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.UpdatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        }

        public async Task<Conversation> AddAsync(Conversation conversation, CancellationToken ct = default)
        {
            _ = await context.Conversations.AddAsync(conversation, ct);
            return conversation;
        }

        public Task UpdateAsync(Conversation conversation, CancellationToken ct = default)
        {
            _ = context.Conversations.Update(conversation);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(ConversationId id, CancellationToken ct = default)
        {
            Conversation? conversation = await context.Conversations.FindAsync([id], ct);
            if (conversation != null)
            {
                _ = context.Conversations.Remove(conversation);
            }
        }

        public async Task<int> CountByUserIdAsync(UserId userId, CancellationToken ct = default)
        {
            return await context.Conversations
                .Where(c => c.UserId == userId)
                .CountAsync(ct);
        }
    }
}

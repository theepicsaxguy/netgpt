// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetGPT.Domain.Aggregates;
using NetGPT.Domain.Entities;
using NetGPT.Domain.Events;
using NetGPT.Infrastructure.Persistence.Configurations;

namespace NetGPT.Infrastructure.Persistence
{
    public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<Conversation> Conversations => Set<Conversation>();

        public DbSet<Message> Messages => Set<Message>();

        public DbSet<Entities.RefreshToken> RefreshTokens => Set<Entities.RefreshToken>();

        public DbSet<User> Users => Set<User>();

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Collect domain events from Conversation aggregates
            List<IDomainEvent> domainEvents = [.. ChangeTracker.Entries<Conversation>()
                .Select(e => e.Entity)
                .Where(c => c.DomainEvents.Any())
                .SelectMany(c =>
                {
                    List<IDomainEvent> events = [.. c.DomainEvents];
                    c.ClearDomainEvents();
                    return events;
                })];

            int result = await base.SaveChangesAsync(cancellationToken);

            // TODO: Publish domain events via MediatR
            return result;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.ApplyConfiguration(new ConversationConfiguration());
            _ = modelBuilder.ApplyConfiguration(new MessageConfiguration());

            // RefreshToken uses default conventions - explicit configuration may be added later
            base.OnModelCreating(modelBuilder);
        }
    }
}

// <copyright file="ApplicationDbContext.cs" theepicsaxguy">
// \
// </copyright>

namespace NetGPT.Infrastructure.Persistence
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using NetGPT.Domain.Aggregates;
    using NetGPT.Domain.Events;
    using NetGPT.Infrastructure.Persistence.Configurations;

    public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<Conversation> Conversations => this.Set<Conversation>();

        public DbSet<Message> Messages => this.Set<Message>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.ApplyConfiguration(new ConversationConfiguration());
            _ = modelBuilder.ApplyConfiguration(new MessageConfiguration());

            base.OnModelCreating(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Collect domain events from Conversation aggregates
            List<IDomainEvent> domainEvents = [.. this.ChangeTracker.Entries<Conversation>()
                .Select(e => e.Entity)
                .Where(c => c.DomainEvents.Any())
                .SelectMany(c =>
                {
                    List<IDomainEvent> events = [.. c.DomainEvents];
                    c.ClearDomainEvents();
                    return events;
                })];

            var result = await base.SaveChangesAsync(cancellationToken);

            // TODO: Publish domain events via MediatR
            return result;
        }
    }
}

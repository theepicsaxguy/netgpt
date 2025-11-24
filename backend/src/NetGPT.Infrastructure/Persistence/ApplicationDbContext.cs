using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetGPT.Domain.Aggregates;
using NetGPT.Domain.Primitives;
using NetGPT.Infrastructure.Persistence.Configurations;

namespace NetGPT.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext
{
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ConversationConfiguration());
        modelBuilder.ApplyConfiguration(new MessageConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Collect domain events from Conversation aggregates
        var domainEvents = ChangeTracker.Entries<Conversation>()
            .Select(e => e.Entity)
            .Where(c => c.DomainEvents.Any())
            .SelectMany(c =>
            {
                var events = c.DomainEvents.ToList();
                c.ClearDomainEvents();
                return events;
            })
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        // TODO: Publish domain events via MediatR

        return result;
    }
}

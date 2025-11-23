using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetGPT.Domain.Aggregates.ConversationAggregate;

namespace NetGPT.Infrastructure.Persistence.Configurations;

public sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");
        
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.UserId).IsRequired();
        builder.Property(c => c.Title).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Status).IsRequired();
        builder.Property(c => c.TokensUsed).IsRequired();
        
        builder.OwnsOne(c => c.AgentConfiguration, config =>
        {
            config.Property(a => a.ModelName).HasColumnName("ModelName").HasMaxLength(50);
            config.Property(a => a.Temperature).HasColumnName("Temperature");
            config.Property(a => a.MaxTokens).HasColumnName("MaxTokens");
            config.Property(a => a.TopP).HasColumnName("TopP");
            config.Property(a => a.FrequencyPenalty).HasColumnName("FrequencyPenalty");
            config.Property(a => a.PresencePenalty).HasColumnName("PresencePenalty");
        });
        
        builder.HasMany(typeof(Message))
            .WithOne()
            .HasForeignKey("ConversationId")
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.CreatedAt);
    }
}

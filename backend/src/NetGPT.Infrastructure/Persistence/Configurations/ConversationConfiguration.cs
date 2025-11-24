// Copyright (c) 2025 NetGPT. All rights reserved.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetGPT.Domain.Aggregates;

namespace NetGPT.Infrastructure.Persistence.Configurations
{
    public sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
    {
        public void Configure(EntityTypeBuilder<Conversation> builder)
        {
            _ = builder.ToTable("Conversations");

            _ = builder.HasKey(c => c.Id);

            _ = builder.Property(c => c.UserId).IsRequired();
            _ = builder.Property(c => c.Title).IsRequired().HasMaxLength(200);
            _ = builder.Property(c => c.Status).IsRequired();
            _ = builder.Property(c => c.TokensUsed).IsRequired();

            _ = builder.OwnsOne(c => c.AgentConfiguration, config =>
            {
                _ = config.Property(a => a.ModelName).HasColumnName("ModelName").HasMaxLength(50);
                _ = config.Property(a => a.Temperature).HasColumnName("Temperature");
                _ = config.Property(a => a.MaxTokens).HasColumnName("MaxTokens");
                _ = config.Property(a => a.TopP).HasColumnName("TopP");
                _ = config.Property(a => a.FrequencyPenalty).HasColumnName("FrequencyPenalty");
                _ = config.Property(a => a.PresencePenalty).HasColumnName("PresencePenalty");
            });

            _ = builder.HasMany(typeof(Message))
                .WithOne()
                .HasForeignKey("ConversationId")
                .OnDelete(DeleteBehavior.Cascade);

            _ = builder.HasIndex(c => c.UserId);
            _ = builder.HasIndex(c => c.CreatedAt);
        }
    }
}

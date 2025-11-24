using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetGPT.Domain.Aggregates;

namespace NetGPT.Infrastructure.Persistence.Configurations;

public sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.ConversationId).IsRequired();
        builder.Property(m => m.Role).IsRequired();
        builder.Property(m => m.CreatedAt).IsRequired();

        builder.OwnsOne(m => m.Content, content =>
        {
            content.Property(c => c.Text).HasColumnName("Text").IsRequired();
            content.OwnsMany(c => c.Attachments, attachment =>
            {
                attachment.ToTable("MessageAttachments");
                attachment.Property(a => a.FileName).HasMaxLength(255);
                attachment.Property(a => a.StorageKey).HasColumnName("StorageKey").HasMaxLength(1000);
                attachment.Property(a => a.ContentType).HasMaxLength(100);
                attachment.Property(a => a.SizeBytes).HasColumnName("SizeBytes");
            });
        });

        builder.OwnsOne(m => m.Metadata, metadata =>
        {
            metadata.Property(m => m.TokenCount).HasColumnName("TokenCount");
            metadata.Property(m => m.AgentName).HasColumnName("AgentName").HasMaxLength(50);
            // ToolInvocations and CustomProperties are complex types, handle as JSON if needed
        });

        builder.HasIndex(m => m.ConversationId);
        builder.HasIndex(m => m.CreatedAt);
    }
}

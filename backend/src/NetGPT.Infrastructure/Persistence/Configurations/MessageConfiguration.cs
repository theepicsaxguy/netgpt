// <copyright file="MessageConfiguration.cs" theepicsaxguy">
// \
// </copyright>

namespace NetGPT.Infrastructure.Persistence.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NetGPT.Domain.Aggregates;

    public sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            _ = builder.ToTable("Messages");

            _ = builder.HasKey(m => m.Id);

            _ = builder.Property(m => m.ConversationId).IsRequired();
            _ = builder.Property(m => m.Role).IsRequired();
            _ = builder.Property(m => m.CreatedAt).IsRequired();

            _ = builder.OwnsOne(m => m.Content, content =>
            {
                _ = content.Property(c => c.Text).HasColumnName("Text").IsRequired();
                _ = content.OwnsMany(c => c.Attachments, attachment =>
                {
                    _ = attachment.ToTable("MessageAttachments");
                    _ = attachment.Property(a => a.FileName).HasMaxLength(255);
                    _ = attachment.Property(a => a.StorageKey).HasColumnName("StorageKey").HasMaxLength(1000);
                    _ = attachment.Property(a => a.ContentType).HasMaxLength(100);
                    _ = attachment.Property(a => a.SizeBytes).HasColumnName("SizeBytes");
                });
            });

            _ = builder.OwnsOne(m => m.Metadata, metadata =>
            {
                _ = metadata.Property(m => m.TokenCount).HasColumnName("TokenCount");
                _ = metadata.Property(m => m.AgentName).HasColumnName("AgentName").HasMaxLength(50);

                // ToolInvocations and CustomProperties are complex types, handle as JSON if needed
            });

            _ = builder.HasIndex(m => m.ConversationId);
            _ = builder.HasIndex(m => m.CreatedAt);
        }
    }
}

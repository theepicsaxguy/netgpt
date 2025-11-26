// Copyright (c) 2025 NetGPT. All rights reserved.

using Microsoft.EntityFrameworkCore;
using NetGPT.Domain.Entities;

namespace NetGPT.Infrastructure.Declarative
{
    public sealed class DefinitionDbContext(DbContextOptions<DefinitionDbContext> options) : DbContext(options)
    {
        public DbSet<DefinitionEntity> Definitions => Set<DefinitionEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<DefinitionEntity>(b =>
            {
                _ = b.HasKey(e => e.Id);
                _ = b.Property(e => e.Name).IsRequired();
                _ = b.Property(e => e.Kind).IsRequired();
                _ = b.Property(e => e.Version).IsRequired();
                _ = b.Property(e => e.ContentYaml).IsRequired();
                _ = b.Property(e => e.CreatedBy).IsRequired();
                _ = b.Property(e => e.CreatedAtUtc).IsRequired();
                _ = b.Property(e => e.ContentHash).IsRequired(false);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using NetGPT.Infrastructure.Declarative;

namespace NetGPT.Infrastructure.Persistence
{
    public sealed class DefinitionDbContext : DbContext
    {
        public DefinitionDbContext(DbContextOptions<DefinitionDbContext> options) : base(options)
        {
        }

        public DbSet<DefinitionEntity> Definitions => Set<DefinitionEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DefinitionEntity>(b =>
            {
                b.HasKey(e => e.Id);
                b.Property(e => e.Name).IsRequired();
                b.Property(e => e.Kind).IsRequired();
                b.Property(e => e.Version).IsRequired();
                b.Property(e => e.ContentYaml).IsRequired();
                b.Property(e => e.CreatedBy).IsRequired();
                b.Property(e => e.CreatedAtUtc).IsRequired();
                b.Property(e => e.ContentHash).IsRequired(false);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}


namespace NetGPT.Infrastructure.Repositories
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore.Storage;
    using NetGPT.Domain.Interfaces;
    using NetGPT.Infrastructure.Persistence;

    public sealed class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
    {
        private readonly ApplicationDbContext context = context;
        private IDbContextTransaction? transaction;

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return await this.context.SaveChangesAsync(ct);
        }

        public async Task BeginTransactionAsync(CancellationToken ct = default)
        {
            this.transaction = await this.context.Database.BeginTransactionAsync(ct);
        }

        public async Task CommitTransactionAsync(CancellationToken ct = default)
        {
            if (this.transaction != null)
            {
                await this.transaction.CommitAsync(ct);
                await this.transaction.DisposeAsync();
                this.transaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken ct = default)
        {
            if (this.transaction != null)
            {
                await this.transaction.RollbackAsync(ct);
                await this.transaction.DisposeAsync();
                this.transaction = null;
            }
        }
    }
}

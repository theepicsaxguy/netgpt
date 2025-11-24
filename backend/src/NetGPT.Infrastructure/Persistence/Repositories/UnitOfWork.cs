// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using NetGPT.Domain.Interfaces;

namespace NetGPT.Infrastructure.Persistence.Repositories
{
    public sealed class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
    {
        private readonly ApplicationDbContext context = context;
        private IDbContextTransaction? transaction;

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return await context.SaveChangesAsync(ct);
        }

        public async Task BeginTransactionAsync(CancellationToken ct = default)
        {
            transaction = await context.Database.BeginTransactionAsync(ct);
        }

        public async Task CommitTransactionAsync(CancellationToken ct = default)
        {
            if (transaction != null)
            {
                await transaction.CommitAsync(ct);
                await transaction.DisposeAsync();
                transaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken ct = default)
        {
            if (transaction != null)
            {
                await transaction.RollbackAsync(ct);
                await transaction.DisposeAsync();
                transaction = null;
            }
        }
    }
}

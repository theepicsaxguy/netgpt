// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Threading;
using System.Threading.Tasks;

namespace NetGPT.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken ct = default);

        Task BeginTransactionAsync(CancellationToken ct = default);

        Task CommitTransactionAsync(CancellationToken ct = default);

        Task RollbackTransactionAsync(CancellationToken ct = default);
    }
}

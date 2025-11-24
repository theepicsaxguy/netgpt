// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Domain.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken ct = default);

        Task BeginTransactionAsync(CancellationToken ct = default);

        Task CommitTransactionAsync(CancellationToken ct = default);

        Task RollbackTransactionAsync(CancellationToken ct = default);
    }
}

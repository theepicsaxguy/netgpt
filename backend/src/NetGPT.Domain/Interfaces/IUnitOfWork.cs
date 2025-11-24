// <copyright file="IUnitOfWork.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

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

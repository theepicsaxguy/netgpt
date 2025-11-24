// <copyright file="CancelAgentThreadHandler.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Application.Handlers.Admin
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NetGPT.Application.Commands.Admin;

    public sealed class CancelAgentThreadHandler : IRequestHandler<CancelAgentThreadCommand, Unit>
    {
        public Task<Unit> Handle(CancelAgentThreadCommand request, CancellationToken cancellationToken)
        {
            // TODO: wire into orchestrator cancellation
            return Task.FromResult(Unit.Value);
        }
    }
}

// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NetGPT.Application.Commands.Admin;

namespace NetGPT.Application.Handlers.Admin
{
    public sealed class ResumeAgentThreadHandler : IRequestHandler<ResumeAgentThreadCommand, Unit>
    {
        public Task<Unit> Handle(ResumeAgentThreadCommand request, CancellationToken cancellationToken)
        {
            // TODO: implement resume logic
            return Task.FromResult(Unit.Value);
        }
    }
}

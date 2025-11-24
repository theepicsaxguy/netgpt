using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NetGPT.Application.Commands.Admin;

namespace NetGPT.Application.Handlers.Admin;

public sealed class CancelAgentThreadHandler : IRequestHandler<CancelAgentThreadCommand, Unit>
{
    public Task<Unit> Handle(CancelAgentThreadCommand request, CancellationToken cancellationToken)
    {
        // TODO: wire into orchestrator cancellation
        return Task.FromResult(Unit.Value);
    }
}

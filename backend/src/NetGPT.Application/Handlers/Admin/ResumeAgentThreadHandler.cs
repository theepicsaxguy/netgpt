
namespace NetGPT.Application.Handlers.Admin
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NetGPT.Application.Commands.Admin;

    public sealed class ResumeAgentThreadHandler : IRequestHandler<ResumeAgentThreadCommand, Unit>
    {
        public Task<Unit> Handle(ResumeAgentThreadCommand request, CancellationToken cancellationToken)
        {
            // TODO: implement resume logic
            return Task.FromResult(Unit.Value);
        }
    }
}


namespace NetGPT.Application.Handlers.Admin
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NetGPT.Application.DTOs;
    using NetGPT.Application.Queries.Admin;

    public sealed class GetAgentThreadByIdHandler : IRequestHandler<GetAgentThreadByIdQuery, AgentThreadDetailDto>
    {
        public Task<AgentThreadDetailDto> Handle(GetAgentThreadByIdQuery request, CancellationToken cancellationToken)
        {
            // TODO: implement lookup in persistence
            AgentThreadDetailDto dto = new(
                request.ThreadId,
                Guid.Empty,
                "NotFound",
                DateTime.UtcNow,
                null,
                [],
                []);
            return Task.FromResult(dto);
        }
    }
}

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NetGPT.Application.DTOs;
using NetGPT.Application.Queries.Admin;
using System;

namespace NetGPT.Application.Handlers.Admin;

public sealed class GetAgentThreadByIdHandler : IRequestHandler<GetAgentThreadByIdQuery, AgentThreadDetailDto>
{
    public Task<AgentThreadDetailDto> Handle(GetAgentThreadByIdQuery request, CancellationToken cancellationToken)
    {
        // TODO: implement lookup in persistence
        var dto = new AgentThreadDetailDto(
            request.ThreadId,
            Guid.Empty,
            "NotFound",
            DateTime.UtcNow,
            null,
            Array.Empty<ToolInvocationDetailDto>(),
            Array.Empty<StreamingChunkDetailDto>());
        return Task.FromResult(dto);
    }
}

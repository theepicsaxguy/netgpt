// <copyright file="CancelAgentThreadCommand.cs" theepicsaxguy">
// \
// </copyright>

namespace NetGPT.Application.Commands.Admin
{
    using System;
    using MediatR;

    public sealed record CancelAgentThreadCommand(Guid ThreadId) : IRequest<Unit>;
}

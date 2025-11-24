// <copyright file="RerunAgentThreadCommand.cs" theepicsaxguy">
// \
// </copyright>

namespace NetGPT.Application.Commands.Admin
{
    using System;
    using MediatR;

    public sealed record RerunAgentThreadCommand(Guid ThreadId) : IRequest<Guid>;
}

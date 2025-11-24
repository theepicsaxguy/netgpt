// <copyright file="ResumeAgentThreadCommand.cs" theepicsaxguy">
// \
// </copyright>

namespace NetGPT.Application.Commands.Admin
{
    using System;
    using MediatR;

    public sealed record ResumeAgentThreadCommand(Guid ThreadId) : IRequest<Unit>;
}

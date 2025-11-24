using System;
using MediatR;

namespace NetGPT.Application.Commands.Admin;

public sealed record ResumeAgentThreadCommand(Guid ThreadId) : IRequest<Unit>;

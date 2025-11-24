using System;
using MediatR;

namespace NetGPT.Application.Commands.Admin;

public sealed record RerunAgentThreadCommand(Guid ThreadId) : IRequest<Guid>;

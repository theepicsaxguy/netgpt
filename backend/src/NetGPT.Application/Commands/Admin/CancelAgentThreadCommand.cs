using System;
using MediatR;

namespace NetGPT.Application.Commands.Admin;

public sealed record CancelAgentThreadCommand(Guid ThreadId) : IRequest<Unit>;

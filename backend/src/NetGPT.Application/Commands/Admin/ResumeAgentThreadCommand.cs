// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Application.Commands.Admin
{
    using System;
    using MediatR;

    public sealed record ResumeAgentThreadCommand(Guid ThreadId) : IRequest<Unit>;
}

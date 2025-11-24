// <copyright file="ResumeAgentThreadCommand.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Application.Commands.Admin
{
    using System;
    using MediatR;

    public sealed record ResumeAgentThreadCommand(Guid ThreadId) : IRequest<Unit>;
}

// <copyright file="RerunAgentThreadCommand.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Application.Commands.Admin
{
    using System;
    using MediatR;

    public sealed record RerunAgentThreadCommand(Guid ThreadId) : IRequest<Guid>;
}

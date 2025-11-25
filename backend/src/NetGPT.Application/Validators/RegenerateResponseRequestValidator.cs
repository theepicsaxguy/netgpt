// Copyright (c) 2025 NetGPT. All rights reserved.

using FluentValidation;
using NetGPT.Application.DTOs;

namespace NetGPT.Application.Validators
{
    public sealed class RegenerateResponseRequestValidator : AbstractValidator<RegenerateResponseRequest>
    {
        public RegenerateResponseRequestValidator()
        {
            _ = RuleFor(x => x.ConversationId).NotEmpty();
            _ = RuleFor(x => x.MessageId).NotEmpty();
        }
    }
}

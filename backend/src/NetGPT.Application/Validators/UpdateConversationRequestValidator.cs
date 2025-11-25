// Copyright (c) 2025 NetGPT. All rights reserved.

using FluentValidation;
using NetGPT.Application.DTOs;

namespace NetGPT.Application.Validators
{
    public sealed class UpdateConversationRequestValidator : AbstractValidator<UpdateConversationRequest>
    {
        public UpdateConversationRequestValidator()
        {
            _ = When(x => x.Title != null, () =>
            {
                _ = RuleFor(x => x.Title).MaximumLength(200);
            });
        }
    }
}

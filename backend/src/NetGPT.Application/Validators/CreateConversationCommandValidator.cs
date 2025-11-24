// Copyright (c) 2025 NetGPT. All rights reserved.

using FluentValidation;
using NetGPT.Application.Commands;

namespace NetGPT.Application.Validators
{
    public class CreateConversationCommandValidator : AbstractValidator<CreateConversationCommand>
    {
        public CreateConversationCommandValidator()
        {
            _ = RuleFor(x => x.UserId)
                .NotEmpty();

            _ = RuleFor(x => x.Title)
                .MaximumLength(500)
                .When(x => x.Title != null);
        }
    }
}

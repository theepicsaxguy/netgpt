// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Application.Validators
{
    using FluentValidation;
    using NetGPT.Application.Commands;

    public class CreateConversationCommandValidator : AbstractValidator<CreateConversationCommand>
    {
        public CreateConversationCommandValidator()
        {
            _ = this.RuleFor(x => x.UserId)
                .NotEmpty();

            _ = this.RuleFor(x => x.Title)
                .MaximumLength(500)
                .When(x => x.Title != null);
        }
    }
}

// Copyright (c) 2025 NetGPT. All rights reserved.

using FluentValidation;
using NetGPT.Application.Commands;

namespace NetGPT.Application.Validators
{
    public sealed class CreateConversationValidator : AbstractValidator<CreateConversationCommand>
    {
        public CreateConversationValidator()
        {
            _ = RuleFor(x => x.UserId).NotEmpty();

            _ = When(x => x.Configuration != null, () =>
            {
                _ = RuleFor(x => x.Configuration!.Temperature)
                    .InclusiveBetween(0f, 2f)
                    .When(x => x.Configuration!.Temperature.HasValue);

                _ = RuleFor(x => x.Configuration!.MaxTokens)
                    .InclusiveBetween(1, 128000)
                    .When(x => x.Configuration!.MaxTokens.HasValue);
            });
        }
    }
}

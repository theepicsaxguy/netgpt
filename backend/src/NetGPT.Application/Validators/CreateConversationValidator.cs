// <copyright file="CreateConversationValidator.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Application.Validators
{
    using FluentValidation;
    using NetGPT.Application.Commands;

    public sealed class CreateConversationValidator : AbstractValidator<CreateConversationCommand>
    {
        public CreateConversationValidator()
        {
            _ = this.RuleFor(x => x.UserId).NotEmpty();

            _ = this.When(x => x.Configuration != null, () =>
            {
                _ = this.RuleFor(x => x.Configuration!.Temperature)
                    .InclusiveBetween(0f, 2f)
                    .When(x => x.Configuration!.Temperature.HasValue);

                _ = this.RuleFor(x => x.Configuration!.MaxTokens)
                    .InclusiveBetween(1, 128000)
                    .When(x => x.Configuration!.MaxTokens.HasValue);
            });
        }
    }
}

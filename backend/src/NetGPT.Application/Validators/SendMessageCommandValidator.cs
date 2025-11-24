// Copyright (c) 2025 NetGPT. All rights reserved.

using FluentValidation;
using NetGPT.Application.Commands;

namespace NetGPT.Application.Validators
{
    public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
    {
        public SendMessageCommandValidator()
        {
            _ = RuleFor(x => x.ConversationId)
                .NotEmpty();

            _ = RuleFor(x => x.UserId)
                .NotEmpty();

            _ = RuleFor(x => x.Content)
                .NotEmpty()
                .MaximumLength(50000);
        }
    }
}

// Copyright (c) 2025 NetGPT. All rights reserved.

using FluentValidation;
using NetGPT.Application.Commands;

namespace NetGPT.Application.Validators
{
    public sealed class SendMessageValidator : AbstractValidator<SendMessageCommand>
    {
        public SendMessageValidator()
        {
            _ = RuleFor(x => x.ConversationId).NotEmpty();
            _ = RuleFor(x => x.UserId).NotEmpty();
            _ = RuleFor(x => x.Content)
                .NotEmpty()
                .MaximumLength(32000);
        }
    }
}

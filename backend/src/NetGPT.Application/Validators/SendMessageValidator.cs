// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Application.Validators
{
    using FluentValidation;
    using NetGPT.Application.Commands;

    public sealed class SendMessageValidator : AbstractValidator<SendMessageCommand>
    {
        public SendMessageValidator()
        {
            _ = this.RuleFor(x => x.ConversationId).NotEmpty();
            _ = this.RuleFor(x => x.UserId).NotEmpty();
            _ = this.RuleFor(x => x.Content)
                .NotEmpty()
                .MaximumLength(32000);
        }
    }
}

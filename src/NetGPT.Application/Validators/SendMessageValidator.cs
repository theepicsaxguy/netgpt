using FluentValidation;
using NetGPT.Application.Commands;

namespace NetGPT.Application.Validators;

public sealed class SendMessageValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(32000);
    }
}

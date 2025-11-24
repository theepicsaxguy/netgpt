// <copyright file="SendMessageCommandValidator.cs" theepicsaxguy">
// \
// </copyright>

namespace NetGPT.Application.Validators
{
    using FluentValidation;
    using NetGPT.Application.Commands;

    public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
    {
        public SendMessageCommandValidator()
        {
            _ = this.RuleFor(x => x.ConversationId)
                .NotEmpty();

            _ = this.RuleFor(x => x.UserId)
                .NotEmpty();

            _ = this.RuleFor(x => x.Content)
                .NotEmpty()
                .MaximumLength(50000);
        }
    }
}

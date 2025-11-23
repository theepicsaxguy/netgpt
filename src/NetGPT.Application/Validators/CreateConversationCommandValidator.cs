using FluentValidation;
using NetGPT.Application.Commands;

namespace NetGPT.Application.Validators;

public class CreateConversationCommandValidator : AbstractValidator<CreateConversationCommand>
{
    public CreateConversationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .MaximumLength(500)
            .When(x => x.Title != null);
    }
}
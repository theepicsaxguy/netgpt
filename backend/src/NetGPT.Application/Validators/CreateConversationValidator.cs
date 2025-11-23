using FluentValidation;
using NetGPT.Application.Commands;

namespace NetGPT.Application.Validators;

public sealed class CreateConversationValidator : AbstractValidator<CreateConversationCommand>
{
    public CreateConversationValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        
        When(x => x.Configuration != null, () =>
        {
            RuleFor(x => x.Configuration!.Temperature)
                .InclusiveBetween(0f, 2f)
                .When(x => x.Configuration!.Temperature.HasValue);
            
            RuleFor(x => x.Configuration!.MaxTokens)
                .InclusiveBetween(1, 128000)
                .When(x => x.Configuration!.MaxTokens.HasValue);
        });
    }
}

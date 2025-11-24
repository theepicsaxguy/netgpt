// <copyright file="ValidationBehavior.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Application.Behaviors
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentValidation;
    using FluentValidation.Results;
    using MediatR;

    public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> validators = validators;

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (!this.validators.Any())
            {
                return await next(cancellationToken);
            }

            ValidationContext<TRequest> context = new(request);

            List<ValidationFailure> failures = [.. this.validators
                .Select(v => v.Validate(context))
                .SelectMany(result => result.Errors)
                .Where(f => f != null)];

            return failures.Count != 0 ? throw new ValidationException(failures) : await next(cancellationToken);
        }
    }
}

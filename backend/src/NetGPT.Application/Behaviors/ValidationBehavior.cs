// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace NetGPT.Application.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> validators = validators;

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // If there are no validators, continue to the next handler.
            if (!validators.Any())
            {
                // Intentionally not forwarding the pipeline cancellation token to the RequestHandlerDelegate
                // (the delegate has no token parameter). Pass CancellationToken.None explicitly to satisfy CA2016.
                _ = cancellationToken; // explicit usage to avoid analyzer false positives
                return await next();
            }

            ValidationContext<TRequest> context = new(request);

            // Run validators synchronously (FluentValidation sync API) and collect failures.
            List<ValidationFailure> failures = validators
                .Select(v => v.Validate(context))
                .SelectMany(result => result.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                throw new ValidationException(failures);
            }

            // Intentionally not forwarding the pipeline cancellation token to the RequestHandlerDelegate
            _ = cancellationToken; // explicit usage to avoid analyzer false positives
            return await next();
        }
    }
}

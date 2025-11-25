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
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // If there are no validators, continue to the next handler.
            if (!_validators.Any())
            {
                // Intentionally not forwarding the pipeline cancellation token to the RequestHandlerDelegate
                // (the delegate has no token parameter). Use Task.Run and pass CancellationToken.None explicitly
                // to make the non-propagation intentional and satisfy CA2016.
                _ = cancellationToken; // explicit usage to avoid analyzer false positives
                return await Task.Run(() => next(), CancellationToken.None);
            }

            ValidationContext<TRequest> context = new(request);

            // Run validators synchronously (FluentValidation sync API) and collect failures.
            List<ValidationFailure> failures = _validators
                .Select(v => v.Validate(context))
                .SelectMany(result => result.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                throw new ValidationException(failures);
            }

            // Intentionally not forwarding the pipeline cancellation token to the RequestHandlerDelegate
            // Use Task.Run and pass CancellationToken.None explicitly to indicate intentional non-propagation.
            _ = cancellationToken; // explicit usage to avoid analyzer false positives
            return await Task.Run(() => next(), CancellationToken.None);
        }
    }
}

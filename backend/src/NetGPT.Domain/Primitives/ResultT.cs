// Copyright (c) 2025 NetGPT. All rights reserved.

using System;

namespace NetGPT.Domain.Primitives
{
    /// <summary>
    /// Represents the result of an operation with a value, either success or failure.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class Result<TValue> : Result
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        public TValue Value => IsSuccess
            ? field!
            : throw new InvalidOperationException("The value of a failure result cannot be accessed.");

        protected internal Result(TValue? value, bool isSuccess, DomainError error)
            : base(isSuccess, error)
        {
            Value = value;
        }

        /// <summary>
        /// Implicitly converts a value to a successful result.
        /// </summary>
        /// <param name="value">The value.</param>
        public static implicit operator Result<TValue>(TValue value)
        {
            return Success(value);
        }
    }
}
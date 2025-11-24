// Copyright (c) 2025 NetGPT. All rights reserved.

using System;

namespace NetGPT.Domain.Primitives
{
    /// <summary>
    /// Represents the result of an operation, either success or failure.
    /// </summary>
    public class Result
    {
        protected Result(bool isSuccess, DomainError error)
        {
            if (isSuccess && error != DomainError.None)
            {
                throw new InvalidOperationException();
            }

            if (!isSuccess && error == DomainError.None)
            {
                throw new InvalidOperationException();
            }

            IsSuccess = isSuccess;
            Error = error;
        }

        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets a value indicating whether the operation failed.
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// Gets the error associated with the failure.
        /// </summary>
        public DomainError Error { get; }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <returns>A successful result.</returns>
        public static Result Success()
        {
            return new(true, DomainError.None);
        }

        /// <summary>
        /// Creates a failed result with the specified error.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <returns>A failed result.</returns>
        public static Result Failure(DomainError error)
        {
            return new(false, error);
        }

        /// <summary>
        /// Creates a successful result with the specified value.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>A successful result with the value.</returns>
        public static Result<TValue> Success<TValue>(TValue value)
        {
            return new(value, true, DomainError.None);
        }

        /// <summary>
        /// Creates a failed result with the specified error.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="error">The error.</param>
        /// <returns>A failed result.</returns>
        public static Result<TValue> Failure<TValue>(DomainError error)
        {
            return new(default, false, error);
        }
    }
}

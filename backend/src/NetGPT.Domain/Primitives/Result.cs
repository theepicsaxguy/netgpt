// <copyright file="Result.cs" theepicsaxguy">
// \
// </copyright>

namespace NetGPT.Domain.Primitives
{
    using System;

    /// <summary>
    /// Represents the result of an operation, either success or failure.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets a value indicating whether the operation failed.
        /// </summary>
        public bool IsFailure => !this.IsSuccess;

        /// <summary>
        /// Gets the error associated with the failure.
        /// </summary>
        public Error Error { get; }

        protected Result(bool isSuccess, Error error)
        {
            if (isSuccess && error != Error.None)
            {
                throw new InvalidOperationException();
            }

            if (!isSuccess && error == Error.None)
            {
                throw new InvalidOperationException();
            }

            this.IsSuccess = isSuccess;
            this.Error = error;
        }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <returns>A successful result.</returns>
        public static Result Success()
        {
            return new(true, Error.None);
        }

        /// <summary>
        /// Creates a failed result with the specified error.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <returns>A failed result.</returns>
        public static Result Failure(Error error)
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
            return new(value, true, Error.None);
        }

        /// <summary>
        /// Creates a failed result with the specified error.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="error">The error.</param>
        /// <returns>A failed result.</returns>
        public static Result<TValue> Failure<TValue>(Error error)
        {
            return new(default, false, error);
        }
    }

    /// <summary>
    /// Represents the result of an operation with a value, either success or failure.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class Result<TValue> : Result
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        public TValue Value => this.IsSuccess
            ? field!
            : throw new InvalidOperationException("The value of a failure result cannot be accessed.");

        protected internal Result(TValue? value, bool isSuccess, Error error)
            : base(isSuccess, error)
        {
            this.Value = value;
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

    /// <summary>
    /// Represents an error.
    /// </summary>
    /// <param name="Code">The error code.</param>
    /// <param name="Message">The error message.</param>
    public record Error(string Code, string Message)
    {
        /// <summary>
        /// Represents no error.
        /// </summary>
        public static readonly Error None = new(string.Empty, string.Empty);

        /// <summary>
        /// Represents a null value error.
        /// </summary>
        public static readonly Error NullValue = new("Error.NullValue", "The specified value was null.");
    }
}
}


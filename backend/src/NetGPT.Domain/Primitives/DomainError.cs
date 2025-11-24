// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Domain.Primitives
{
    /// <summary>
    /// Represents an error.
    /// </summary>
    /// <param name="Code">The error code.</param>
    /// <param name="Message">The error message.</param>
    public record DomainError(string Code, string Message)
    {
        /// <summary>
        /// Represents no error.
        /// </summary>
        public static readonly DomainError None = new(string.Empty, string.Empty);

        /// <summary>
        /// Represents a null value error.
        /// </summary>
        public static readonly DomainError NullValue = new("Error.NullValue", "The specified value was null.");
    }
}

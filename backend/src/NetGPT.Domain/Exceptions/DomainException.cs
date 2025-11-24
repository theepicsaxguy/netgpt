// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Domain.Exceptions
{
    using System;

    public class DomainException : Exception
    {
        public DomainException(string message)
            : base(message)
        {
        }

        public DomainException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

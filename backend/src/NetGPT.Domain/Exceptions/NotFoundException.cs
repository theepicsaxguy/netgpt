// Copyright (c) 2025 NetGPT. All rights reserved.

namespace NetGPT.Domain.Exceptions
{
    public class NotFoundException(string entityName, object key) : DomainException($"{entityName} with key {key} not found")
    {
    }
}

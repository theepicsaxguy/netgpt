// <copyright file="NotFoundException.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Domain.Exceptions
{
    public class NotFoundException(string entityName, object key) : DomainException($"{entityName} with key {key} not found")
    {
    }
}

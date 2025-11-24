// <copyright file="NotFoundException.cs" theepicsaxguy">
// \
// </copyright>

namespace NetGPT.Domain.Exceptions
{
    public class NotFoundException(string entityName, object key) : DomainException($"{entityName} with key {key} not found")
    {
    }
}

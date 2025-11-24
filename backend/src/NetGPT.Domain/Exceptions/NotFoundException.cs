
namespace NetGPT.Domain.Exceptions
{
    public class NotFoundException(string entityName, object key) : DomainException($"{entityName} with key {key} not found")
    {
    }
}

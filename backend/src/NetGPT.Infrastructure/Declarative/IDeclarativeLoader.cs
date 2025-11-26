using System;
using System.Threading;
using System.Threading.Tasks;
using NetGPT.Domain.Entities;

namespace NetGPT.Infrastructure.Declarative
{
    public interface IDeclarativeLoader
    {
        Task<Application.Interfaces.IAgentExecutable> LoadAsync(DefinitionEntity definition, CancellationToken cancellationToken = default);
    }
}

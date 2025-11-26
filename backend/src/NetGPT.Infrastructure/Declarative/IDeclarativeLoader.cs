using System.Threading.Tasks;

namespace NetGPT.Infrastructure.Declarative
{
    public interface IDeclarativeLoader
    {
        Task<IAgentExecutable> LoadAsync(DefinitionEntity def);
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using NetGPT.Infrastructure.Declarative.Models;

namespace NetGPT.Infrastructure.Declarative
{
    public interface IDeclarativeLoader
    {
        Task<IAgentExecutable> LoadAsync(DefinitionEntity definition, CancellationToken cancellationToken = default);
    }
}

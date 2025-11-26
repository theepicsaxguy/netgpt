using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetGPT.Infrastructure.Declarative
{
    public interface IDefinitionRepository
    {
        Task<DefinitionEntity> CreateAsync(DefinitionEntity def);
        Task<DefinitionEntity?> GetLatestByNameAsync(string name);
        Task<DefinitionEntity?> GetByIdAsync(Guid id);
        Task<IEnumerable<DefinitionEntity>> ListLatestAsync(int page, int pageSize);
        Task<int> GetNextVersionAsync(string name);
        Task UpdateAsync(DefinitionEntity def);
    }
}

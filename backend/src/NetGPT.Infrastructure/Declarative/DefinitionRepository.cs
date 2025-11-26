using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetGPT.Infrastructure.Persistence;

namespace NetGPT.Infrastructure.Declarative
{
    public sealed class DefinitionRepository : IDefinitionRepository
    {
        private readonly DefinitionDbContext _db;

        public DefinitionRepository(DefinitionDbContext db)
        {
            _db = db;
        }

        public async Task<DefinitionEntity> CreateAsync(DefinitionEntity def)
        {
            def.Id = Guid.NewGuid();
            def.CreatedAtUtc = DateTime.UtcNow;
            _db.Definitions.Add(def);
            await _db.SaveChangesAsync();
            return def;
        }

        public async Task<DefinitionEntity?> GetLatestByNameAsync(string name)
        {
            return await _db.Definitions
                .Where(d => d.Name == name)
                .OrderByDescending(d => d.Version)
                .FirstOrDefaultAsync();
        }

        public async Task<DefinitionEntity?> GetByIdAsync(Guid id)
        {
            return await _db.Definitions.FindAsync(id);
        }

        public async Task<IEnumerable<DefinitionEntity>> ListLatestAsync(int page, int pageSize)
        {
            // For simplicity, return latest version per name
            var grouped = await _db.Definitions
                .GroupBy(d => d.Name)
                .Select(g => g.OrderByDescending(d => d.Version).FirstOrDefault())
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return grouped.Where(x => x != null)!.Cast<DefinitionEntity>();
        }

        public async Task<int> GetNextVersionAsync(string name)
        {
            var latest = await _db.Definitions
                .Where(d => d.Name == name)
                .OrderByDescending(d => d.Version)
                .FirstOrDefaultAsync();

            return latest == null ? 1 : latest.Version + 1;
        }

        public async Task UpdateAsync(DefinitionEntity def)
        {
            _db.Definitions.Update(def);
            await _db.SaveChangesAsync();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetGPT.Domain.Entities;

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
            if (def is null) throw new ArgumentNullException(nameof(def));

            // Basic required fields
            if (string.IsNullOrWhiteSpace(def.Name)) throw new ArgumentException("Definition Name is required", nameof(def.Name));
            if (string.IsNullOrWhiteSpace(def.Kind)) throw new ArgumentException("Definition Kind is required", nameof(def.Kind));
            if (string.IsNullOrWhiteSpace(def.ContentYaml)) throw new ArgumentException("Definition ContentYaml is required", nameof(def.ContentYaml));
            if (string.IsNullOrWhiteSpace(def.CreatedBy)) throw new ArgumentException("Definition CreatedBy is required", nameof(def.CreatedBy));

            // Security check: disallow raw secret values. Allow placeholders that start with =Secret. or =Env.
            // Match common secret keys in YAML like `secret: value`, `api_key: value`, `password: value`.
            var rawSecretPattern = new Regex(@"(?im)^[\s-]*?(api[_-]?key|apikey|password|secret)\s*:\s*(.+)$");
            var matches = rawSecretPattern.Matches(def.ContentYaml);
            foreach (Match m in matches)
            {
                if (m.Groups.Count < 3) continue;
                string value = m.Groups[2].Value.Trim();
                if (string.IsNullOrEmpty(value)) continue;
                // Accept placeholders that start with =Secret. or =Env.
                if (value.StartsWith("=Secret.", StringComparison.OrdinalIgnoreCase) || value.StartsWith("=Env.", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Reject raw secret values (do not persist)
                throw new InvalidOperationException("Definition contains raw secret values which are not allowed. Use placeholders like =Secret.<Name> or =Env.<Name> instead.");
            }

            // Ensure versioning: if caller didn't set a positive version, compute next version for the name
            if (def.Version <= 0)
            {
                def.Version = await GetNextVersionAsync(def.Name);
            }

            // Compute content hash if not present (used by seeding/idempotency)
            if (string.IsNullOrEmpty(def.ContentHash))
            {
                using var sha = SHA256.Create();
                byte[] bytes = Encoding.UTF8.GetBytes(def.ContentYaml);
                byte[] hash = sha.ComputeHash(bytes);
                def.ContentHash = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
            }

            def.Id = def.Id == Guid.Empty ? Guid.NewGuid() : def.Id;
            def.CreatedAtUtc = def.CreatedAtUtc == default ? DateTime.UtcNow : def.CreatedAtUtc;

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

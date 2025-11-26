using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NetGPT.Infrastructure.Declarative
{
    public static class SeedDefinitions
    {
        public static async Task RunAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var provider = scope.ServiceProvider;
            var repo = provider.GetRequiredService<IDefinitionRepository>();
            var logger = provider.GetRequiredService<ILogger<SeedDefinitions>>();
            var env = provider.GetRequiredService<IHostEnvironment>();

            string contentRoot = env.ContentRootPath;

            string[] sampleDirs = new[]
            {
                Path.Combine(contentRoot, "agent-samples"),
                Path.Combine(contentRoot, "workflow-samples")
            };

            foreach (string dir in sampleDirs)
            {
                if (!Directory.Exists(dir))
                {
                    logger.LogInformation("SeedDefinitions: sample directory not found: {dir}", dir);
                    continue;
                }

                var files = Directory.GetFiles(dir, "*.yml").Concat(Directory.GetFiles(dir, "*.yaml"));

                foreach (var file in files)
                {
                    try
                    {
                        string content = await File.ReadAllTextAsync(file);
                        string hash = ComputeSha256(content);
                        string name = Path.GetFileName(file);
                        string kind = dir.Contains("agent-samples") ? "Prompt" : "Workflow";

                        var latest = await repo.GetLatestByNameAsync(name);
                        if (latest != null && !string.IsNullOrEmpty(latest.ContentHash) && latest.ContentHash == hash)
                        {
                            logger.LogInformation("SeedDefinitions: skipping unchanged definition {name}", name);
                            continue; // idempotent - unchanged
                        }

                        int version = await repo.GetNextVersionAsync(name);
                        var def = new DefinitionEntity
                        {
                            Name = name,
                            Kind = kind,
                            Version = version,
                            ContentYaml = content,
                            ContentHash = hash,
                            CreatedBy = "seed",
                        };

                        await repo.CreateAsync(def);
                        logger.LogInformation("SeedDefinitions: seeded definition {name} v{version}", name, version);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "SeedDefinitions: failed to seed file {file}", file);
                    }
                }
            }
        }

        private static string ComputeSha256(string content)
        {
            using var sha = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            byte[] hash = sha.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}

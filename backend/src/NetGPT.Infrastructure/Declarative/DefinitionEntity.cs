using System;

namespace NetGPT.Infrastructure.Declarative
{
    public sealed class DefinitionEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string Kind { get; set; } = null!; // e.g. "Prompt" or "Workflow"

        public int Version { get; set; }

        public string ContentYaml { get; set; } = null!;

        public string CreatedBy { get; set; } = null!;

        public DateTime CreatedAtUtc { get; set; }

        public string? ContentHash { get; set; }
    }
}

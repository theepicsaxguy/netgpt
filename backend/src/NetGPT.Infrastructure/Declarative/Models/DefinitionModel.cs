// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Collections.Generic;

namespace NetGPT.Infrastructure.Declarative.Models
{
    public sealed class DefinitionModel
    {
        public string Type { get; set; } = string.Empty; // e.g., "prompt" or "workflow" or provider name

        public string Name { get; set; } = string.Empty;

        public string? Instructions { get; set; }

        public string? Model { get; set; }

        public List<string>? Tools { get; set; }
    }
}

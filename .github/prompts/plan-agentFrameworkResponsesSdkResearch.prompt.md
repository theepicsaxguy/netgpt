Plan: Agent Framework Responses SDK Research

TL;DR — I will produce a thorough, citation-backed reference that extracts the exact SDK surface (namespaces, classes, types, methods, extension methods, parameters, return types, streaming event types) Microsoft exposes for using the Responses API via the Agent Framework and Microsoft.Extensions.AI. I\'ll prioritize usage with an OpenAI-compatible base URL (non-Azure) and include how to wire that client into agents, streaming semantics, DI registration, and SignalR streaming patterns — all directly sourced from Microsoft docs and official samples.

Steps
1. Locate and read Microsoft docs and official samples for `Microsoft.Agents.AI`, `Microsoft.Agents.AI.OpenAI`, `Microsoft.Extensions.AI`, `Microsoft.Extensions.AI.OpenAI`, `Azure.AI.OpenAI`, and the OpenAI .NET SDK.
2. Extract the exact API surface: namespaces, classes, constructors, properties, methods, extension methods and their signatures (including overloads), async/streaming variants, event types and payload shapes.
3. Produce a structured reference document (per-type) listing signatures, common parameter/return semantics, and example call sequences (no ad-hoc code—only exact API references).
4. Provide precise guidance for using an OpenAI base URL (creating an OpenAI/OpenAIResponse client with custom endpoint) and how to plug it into Agent Framework agent creation.
5. Include exact streaming semantics: response events (response.created, output_item.added/done, response.completed), how the SDK surfaces them, and recommended handling patterns (cancellation, chunking).
6. Deliver results as a single research artifact: (a) a machine-readable JSON summary of docs and APIs, and (b) a human-readable markdown reference mapping SDK types to usage scenarios. Also map where code would go in your repo (`Program.cs`, `Agents/AgentFactory.cs`, `ConversationHub.cs`) for integration.

Further Considerations
1. I will prioritize OpenAI-compatible base URL usage (as you requested) and only include Azure-specific notes where Microsoft documents different APIs or auth flows.
2. I\'ll avoid speculation — every SDK signature and behavior will be quoted from Microsoft docs or Microsoft/official sample repos.
3. If you prefer, I can also produce a compact quick-reference cheat-sheet (single-page) after the full reference.

Deliverables
- A single JSON report containing: `summary`, `docs`, `snippets`, `packages`, `pitfalls`, and `samples` as requested.
- A per-type SDK reference (C#) with method/property signatures, streaming event shapes, and example call flows.
- Integration notes showing where to add sample code in the repository (e.g., `Program.cs`, `AgentFactory`, `ConversationHub`).
- Optional one-page cheat-sheet.

Now creating the longer, sourced research artifact (no follow-ups).

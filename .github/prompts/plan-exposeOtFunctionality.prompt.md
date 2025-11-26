Plan: Expose "ot" functionality in the API

Goal

Provide a clear spec of missing API endpoints and DTOs necessary to expose "ot" functionality in the API. The repo's agent and tool orchestration exists in `backend/src/NetGPT.Infrastructure/Agents/AgentOrchestrator.cs` and streaming interfaces in `backend/src/NetGPT.Application/Interfaces/IAgentOrchestrator.cs`. The API currently supports conversation creation, message sending, and NDJSON streaming, but lacks a formal tools/agent management surface and richer streaming metadata.

Scope (what will be included)

- Tools controller and DTOs to list/register/invoke tools
- Tool invocation lifecycle endpoints and optional persistence
- Direct tool invocation API
- Enrich message/stream DTOs with provenance and tool invocation metadata
- Expose agent-run options in SendMessage requests (allow tools, model overrides, run options)
- Streaming execute endpoint for declarative definitions
- Admin endpoints for enabling/disabling tools (optional)
- Telemetry exposure (optional; only if requested)

Files to add/change

- Create `backend/src/NetGPT.API/Controllers/ToolsController.cs` (new)
- Add DTOs in `backend/src/NetGPT.Application/DTOs/`:
  - `ToolInfoDto.cs`
  - `ToolMethodDto.cs`
  - `ToolInvokeRequest.cs`
  - `ToolInvokeResponse.cs`
  - `ToolInvocationStatusDto.cs`
  - `ToolPreferenceDto.cs`
  - `RunOptionsDto.cs`
  - Update `SendMessageRequest.cs` to include AllowTools, ToolPreferences, RunOptions, Stream
  - Update `StreamingChunkDto.cs` to include optional Metadata / ChunkMetadataDto
  - Update `ToolInvocationDto` to include InvocationId (optional)

- Add optional persistence: `backend/src/NetGPT.Application/Interfaces/IToolInvocationRepository.cs` and infrastructure implementation

Controller changes

- `ConversationsController`:
  - `SendMessage` should accept new `SendMessageRequest` fields and pass them to `SendMessageCommand`.
  - `StreamMessage` already calls `orchestrator.ExecuteStreamingAsync`; ensure orchestrator can accept run options/tool preferences via conversation or new overloads.

- `ConversationHub`:
  - Ensure `SendMessage` streams enriched `StreamingChunkDto` with metadata events.

- `DeclarativeDefinitionsController`:
  - Add `POST /api/declarative/definitions/{id}/execute/stream` to stream execution progress.

New `ToolsController` endpoints

- GET /api/tools -> list tools
- GET /api/tools/{name} -> get tool metadata
- POST /api/tools/register -> register tool (Admin)
- POST /api/tools/{name}/invoke -> invoke a tool synchronously
- POST /api/tools/{name}/invoke/async -> start async invocation (returns invocationId)
- GET /api/tools/invocations/{invocationId} -> get status/result
- PATCH /api/tools/{name}/enable -> enable/disable tool (Admin)

Streaming details

- Use existing `StreamingChunkDto` NDJSON response for HTTP streaming. Add metadata field to `StreamingChunkDto` to optionally include tool events like invocation start/complete, invocationId, and minor diagnostics.
- SignalR hub `ConversationHub` must emit `MessageChunk` events using the enriched chunk DTO.

Security

- Admin-only endpoints should be decorated with `[Authorize(Policy = "AdminOnly")]`.
- Tool invocation endpoints should require authentication and optionally authorization checks to restrict certain tools to specific roles/users.

Backward compatibility

- Existing endpoints should continue to work; default behavior for new fields should be opt-out (AllowTools = true) and not break clients that don't send these fields.

Next steps / Follow-ups

- Confirm meaning of "ot": telemetry vs tools/agents. This plan assumes "ot" means tools/agent tooling exposed by the infra.
- Prioritize which endpoints to implement first. Recommended start: Tools list & tool invoke; then streaming metadata, SendMessage DTO, and declarative streaming.

Notes

- The repo already contains `StreamingChunkDto`, `MessageMetadataDto`, and `ToolInvocationDto`; extend them instead of creating entirely new types where possible.

---

This file is intended as an editable prompt/spec for implementation; after you confirm I will scaffold the DTO files and a `ToolsController.cs` with minimal implementations and unit tests.

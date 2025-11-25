# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

NetGPT is a production-ready ChatGPT clone built with Clean Architecture and Domain-Driven Design principles. The backend uses .NET 10.0 with Microsoft Agent Framework for AI orchestration, PostgreSQL for data persistence, and SignalR for real-time streaming responses.

## Build and Development Commands

### From the backend/ directory:

```bash
# Build the solution
dotnet build NetGPT.sln

# Run the API (from backend/)
cd src/NetGPT.API && dotnet run

# Run migrations
cd src/NetGPT.API && dotnet ef database update --project ../NetGPT.Infrastructure

# Create a new migration
cd src/NetGPT.API && dotnet ef migrations add <MigrationName> --project ../NetGPT.Infrastructure

# Run tests (when available)
dotnet test

# Clean build artifacts
dotnet clean
```

Or use the Makefile shortcuts:
```bash
make build    # Build solution
make run      # Run API
make migrate  # Apply migrations
make test     # Run tests
```

### Docker:
```bash
docker-compose up -d    # Start with Docker
docker-compose down     # Stop containers
```

## Architecture

The codebase follows Clean Architecture with four distinct layers:

### 1. Domain Layer (NetGPT.Domain/)
- Contains core business logic, aggregates, value objects, and domain events
- Key aggregates: `Conversation`, `Message`
- Value objects: `ConversationId`, `UserId`, `MessageContent`, `ConversationMetadata`
- Domain events: `ConversationCreatedEvent`, `MessageAddedEvent`
- Result Pattern implementation in `Primitives/Result.cs` for error handling without exceptions
- Aggregates use private constructors and factory methods (e.g., `Conversation.Create()`)
- Domain events are collected in aggregates and cleared via `ClearDomainEvents()`

### 2. Application Layer (NetGPT.Application/)
- Implements CQRS pattern using MediatR
- Commands: `CreateConversationCommand`, `SendMessageCommand`, `DeleteConversationCommand`
- Queries: `GetConversationQuery`, `ListConversationsQuery`, `GetMessagesQuery`
- Handlers process commands/queries and coordinate with domain and infrastructure
- DTOs for data transfer between layers
- FluentValidation for input validation (in Validators/)
- AutoMapper for object mapping (in Mappings/)

### 3. Infrastructure Layer (NetGPT.Infrastructure/)
- **Agent Framework**: Microsoft Agent Framework (Microsoft.Agents.AI) integration
  - `AgentFactory`: Creates AI agents with configuration
  - `AgentOrchestrator`: Orchestrates agent execution and streaming responses
  - `OpenAIClientFactory`: Manages OpenAI client instances
- **Tool/Plugin System**: Runtime DI-based plugin registration via `AIFunctionFactory`
  - `IToolRegistry` and `ToolRegistry`: Manages tool registration
  - Tool plugins: `WebSearchToolPlugin`, `CodeExecutionToolPlugin`, `FileProcessingToolPlugin`
  - Tools are registered at startup in `Program.cs` using `AIFunctionFactory.Create()`
- **Persistence**: Entity Framework Core with PostgreSQL
  - `ApplicationDbContext`: Main DbContext
  - Repository pattern: `ConversationRepository` implements `IConversationRepository`
  - Unit of Work pattern: `UnitOfWork` implements `IUnitOfWork`
  - EF configurations in `Persistence/Configurations/` define entity mappings

### 4. API Layer (NetGPT.API/)
- ASP.NET Core Web API with RESTful endpoints
- Controllers: `ConversationsController`, `MessagesController`, `HealthController`
- SignalR Hub: `ConversationHub` at `/hubs/conversation` for streaming messages
- Middleware: `GlobalExceptionMiddleware` for centralized error handling
- Swagger documentation at `/swagger`

## Key Architectural Patterns

### CQRS (Command Query Responsibility Segregation)
- Commands modify state (Create, Delete, Send)
- Queries read state (Get, List)
- All handled through MediatR pipeline

### Agent Orchestration Flow
1. User sends message via REST API or SignalR Hub
2. Command handler retrieves conversation from repository
3. `AgentOrchestrator.ExecuteAsync()` is called with conversation context
4. Agent Factory creates agent with registered tools
5. Agent streams response chunks (may invoke tools during execution)
6. Response is stored as new message in conversation aggregate
7. Changes are persisted via Unit of Work

### Tool Registration Pattern
Tools are registered at runtime in `Program.cs`:
```csharp
var registry = sp.GetRequiredService<IToolRegistry>();
var tools = AIFunctionFactory.Create(new CustomToolPlugin());
foreach (var tool in tools)
{
    registry.RegisterTool(tool);
}
```

Tool methods use `[Description]` attributes for AI agent understanding.

### Result Pattern
Instead of throwing exceptions, methods return `Result` or `Result<T>`:
- Success: `Result.Success()` or `Result.Success(value)`
- Failure: `Result.Failure(new Error("Code", "Message"))`
- Check with `result.IsSuccess` or `result.IsFailure`

## Configuration

### appsettings.json structure:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=netgpt;Username=postgres;Password=postgres"
  },
  "OpenAI": {
    "ApiKey": "your-api-key",
    "DefaultModel": "gpt-4o",
    "MaxTokens": 4000
  }
}
```

Update these before running the application.

## API Endpoints

All endpoints under ``:
- `POST /conversations` - Create new conversation
- `GET /conversations` - List conversations (paginated)
- `GET /conversations/{id}` - Get single conversation
- `DELETE /conversations/{id}` - Delete conversation
- `POST /conversations/{id}/messages` - Send message (REST, non-streaming)

WebSocket endpoint:
- `/hubs/conversation` - SignalR hub for streaming responses

## Database Migrations

The project uses Entity Framework Core Code-First migrations:
- Migration files are generated in `NetGPT.Infrastructure/Migrations/`
- Always run migrations from `NetGPT.API` directory with `--project ../NetGPT.Infrastructure`
- Entity configurations are in `Infrastructure/Persistence/Configurations/`

## Adding Custom Tools/Plugins

1. Create a plugin class with methods decorated with `[Description]`:
```csharp
public sealed class CustomToolPlugin
{
    [Description("Description of what the tool does")]
    public async Task<string> MyTool(
        [Description("Parameter description")] string param)
    {
        // Implementation
        return "result";
    }
}
```

2. Register in `Program.cs`:
```csharp
var customTools = AIFunctionFactory.Create(new CustomToolPlugin());
foreach (var tool in customTools)
{
    registry.RegisterTool(tool);
}
```

## Coding Standards

### Core Principles
The codebase strictly adheres to:
- **DDD (Domain-Driven Design)**: Business logic in domain layer, aggregates as consistency boundaries
- **DRY (Don't Repeat Yourself)**: Shared abstractions, reusable components
- **SOC (Separation of Concerns)**: Clear layer boundaries, single responsibility per component
- **SRP (Single Responsibility Principle)**: Each class has one reason to change

### File Size Limit
- Aim for a maximum of **200 lines per file**
- Split large files into smaller, focused components
- Extract complex logic into separate services or utilities
- Keep classes focused on a single responsibility

### Important Conventions
- Value objects use private constructors and static factory methods
- Aggregates encapsulate domain logic and maintain invariants
- Domain events are raised in aggregates and can be handled for side effects
- Repositories work with aggregate roots only (Conversation, not Message directly)
- All DateTime values use UTC
- Entity IDs are strongly-typed value objects (ConversationId, UserId, MessageId)
- Navigation in aggregates is readonly (e.g., `IReadOnlyList<Message>`)

## CORS Configuration

Development CORS allows origins:
- http://localhost:5173 (Vite default)
- http://localhost:3000 (React default)

Modify in `Program.cs` if frontend runs on different ports.

## Dependencies

### Key NuGet Packages
- Microsoft.Agents.AI (prerelease) - Agent Framework
- Microsoft.Extensions.AI - AI abstractions
- Microsoft.Extensions.AI.OpenAI (prerelease) - OpenAI integration
- OpenAI - OpenAI SDK
- MediatR - CQRS implementation
- FluentValidation - Input validation
- Npgsql.EntityFrameworkCore.PostgreSQL - PostgreSQL provider
- Microsoft.AspNetCore.SignalR - Real-time communication

### Adding AI Framework Packages
When adding AI-related dependencies to a project:
```bash
dotnet add package Microsoft.Agents.AI --prerelease
dotnet add package OpenAI
dotnet add package Microsoft.Extensions.AI.OpenAI --prerelease
dotnet add package Microsoft.Extensions.AI
```

## Notes

- Authentication/Authorization is not yet implemented (TODO in ConversationHub)
- The project uses .NET 10.0 SDK (see global.json)
- SignalR streaming implementation in `ConversationHub.StreamAgentResponse()` is currently a placeholder
- Token counting in `AgentOrchestrator` uses simple estimation (text.Length / 4)

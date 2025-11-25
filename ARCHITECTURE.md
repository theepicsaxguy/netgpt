# NetGPT Backend Architecture Documentation

## Executive Summary

Production-ready ChatGPT clone backend built with:
- **.NET 8** with Clean Architecture
- **Microsoft Agent Framework** for AI orchestration
- **OpenAI API** (standard, not Azure)
- **PostgreSQL** for data persistence
- **SignalR** for real-time streaming
- **CQRS + Domain-Driven Design**

## Architectural Principles Applied

### SOLID Principles

**Single Responsibility Principle (SRP)**
- Each class has one reason to change
- `ConversationRepository` only handles data access
- `AgentOrchestrator` only handles agent execution
- `ConversationMapper` only handles DTO mapping
- Each handler processes one command/query

**Open/Closed Principle**
- `IToolRegistry` allows new tools without modifying core
- `IAgentFactory` supports different agent types
- Plugin system via `AIFunctionFactory` 
- Domain events enable extensibility

**Liskov Substitution Principle**
- All repositories implement `IConversationRepository`
- All handlers implement `IRequestHandler<TRequest, TResponse>`
- Value objects can be substituted for their base class

**Interface Segregation Principle**
- `IConversationRepository` vs `IUnitOfWork` separated
- `IAgentFactory` vs `IAgentOrchestrator` separated
- Small, focused interfaces

**Dependency Inversion Principle**
- Controllers depend on `IMediator` abstraction
- Handlers depend on repository interfaces
- Infrastructure implements domain interfaces

### DRY (Don't Repeat Yourself)
- Base `Entity` and `AggregateRoot` classes
- Shared `Result<T>` pattern for error handling
- Common `ValueObject` base class
- Reusable tool registration pattern

### Separation of Concerns (SoC)
- **Domain**: Business logic only, no infrastructure
- **Application**: Use cases, no persistence details
- **Infrastructure**: External services, databases
- **API**: HTTP/SignalR, no business logic

## Layer Breakdown

### Domain Layer (Entities, Values, Rules)

**Aggregates:**
- `Conversation` (root)
  - Manages messages collection
  - Enforces conversation rules
  - Raises domain events
  
**Value Objects:**
- `MessageContent`: Immutable message data
- `AgentConfiguration`: AI model settings
- `MessageMetadata`: Response analytics

**Domain Events:**
- `ConversationCreatedEvent`
- `MessageAddedEvent`
- `AgentResponseStartedEvent`
- `ToolExecutedEvent`

**Business Rules:**
- Max 32K characters per message
- Only active conversations accept messages
- Temperature between 0-2
- User can only access own conversations

### Application Layer (Use Cases)

**Commands (Write Operations):**
- `CreateConversationCommand`
- `SendMessageCommand`
- `SendMessageStreamingCommand`
- `DeleteConversationCommand`
- `UpdateConversationCommand`

**Queries (Read Operations):**
- `GetConversationQuery`
- `GetConversationsQuery` (paginated)
- `GetMessagesQuery`
- `SearchConversationsQuery`

**Handlers:**
Each command/query has dedicated handler following SRP

**DTOs:**
- Request/Response models for API
- Optimized for Orval code generation
- No domain logic

### Infrastructure Layer (External Concerns)

**Agent Framework Integration:**
```
AgentOrchestrator
  ├── AgentFactory (creates agents)
  ├── ToolRegistry (manages plugins)
  └── Tool Plugins:
      ├── WebSearchToolPlugin
      ├── CodeExecutionToolPlugin
      └── FileProcessingToolPlugin
```

**Persistence:**
- EF Core with PostgreSQL
- Fluent API configurations
- Domain event publishing on SaveChanges

**Tool/Plugin System:**
- Runtime DI registration
- Attribute-based tool definitions
- `[Description]` for LLM discovery

### API Layer (Interface)

**REST API:**
- Versioned endpoints (`/...`)
- Standard HTTP verbs
- Result<T> → HTTP status mapping

**SignalR Hub:**
- Real-time streaming
- WebSocket transport
- Message chunks with metadata

## Agent Framework Details

### Agent Creation
```csharp
var client = new OpenAIClient(apiKey);
var chatClient = client.GetChatClient("gpt-4o");
var agent = new ChatAgent(
    chatClient: chatClient,
    instructions: "System prompt",
    tools: registeredTools
);
```

### Streaming Execution
```csharp
await foreach (var update in agent.RunStreamAsync(messages))
{
    if (update.Text != null) // Content chunk
    if (update.ToolCalls != null) // Tool invocation
}
```

### Tool Registration (Runtime DI)
```csharp
// Automatically discovers methods with [Description]
var tools = AIFunctionFactory.Create(new WebSearchToolPlugin());
foreach (var tool in tools)
{
    registry.RegisterTool(tool);
}
```

## Data Flow Example

### User Sends Message (Streaming)

1. **Client** → SignalR Hub: `SendMessage(conversationId, content)`
2. **Hub** → MediatR: `SendMessageStreamingCommand`
3. **Handler**:
   - Load conversation from repository
   - Add user message to aggregate
   - Save to database
4. **Handler** → AgentOrchestrator: Execute with tools
5. **Orchestrator**:
   - Build chat history from conversation
   - Create agent with configuration
   - Stream response chunks
6. **Chunks** → SignalR → Client
7. **Handler**:
   - Add assistant message to aggregate
   - Update token usage
   - Save final state

## File Size Compliance

All files kept under ~200 lines:
- Domain entities: 50-100 lines each
- Handlers: 30-60 lines each  
- Value objects: 30-80 lines each
- Controllers: 50-100 lines each
- Configurations: 20-50 lines each

## Database Schema

```sql
Conversations
  - Id (PK)
  - UserId (indexed)
  - Title
  - Status
  - TokensUsed
  - ModelName, Temperature, etc. (owned)
  - CreatedAt, UpdatedAt

Messages
  - Id (PK)
  - ConversationId (FK, indexed)
  - Text
  - Role
  - TokenCount, ResponseTimeMs (owned)
  - CreatedAt

MessageAttachments
  - Id (PK)
  - MessageId (FK)
  - FileName, FileUrl, ContentType, FileSizeBytes
```

## Testing Strategy

**Unit Tests:**
- Domain logic (aggregates, value objects)
- Business rules validation
- Command/Query handlers

**Integration Tests:**
- Repository implementations
- API endpoints
- Database interactions

**Agent Tests:**
- Tool execution
- Response streaming
- Error handling

## Scalability Considerations

**Implemented:**
- Stateless API (horizontal scaling ready)
- Connection pooling
- Async/await throughout
- Pagination for queries

**Future:**
- Read replicas
- Caching layer (Redis)
- Message queue for async processing
- Event sourcing for audit

## Security Features

**Implemented:**
- Result pattern (no exception leakage)
- Input validation (FluentValidation)
- Aggregate invariants

**TODO (Production):**
- JWT authentication
- User authorization
- Rate limiting
- Input sanitization
- API key rotation

## Performance Optimizations

- Eager loading for conversations with messages
- Indexed columns (UserId, CreatedAt)
- Streaming responses (low memory)
- Async operations everywhere
- Compiled EF Core queries (future)

## Deployment

```bash
# Local Development
cd backend
make run
# Or: dotnet run --project src/NetGPT.API

# Docker
cd backend
make docker-up
# Or: docker-compose up

# Production (K8s)
kubectl apply -f k8s/
```

## Adding Features

### New Tool/Plugin
1. Create plugin class with `[Description]` methods
2. Register in `Program.cs` DI setup
3. Tool automatically available to all agents

### New Query
1. Create query record in `Application/Queries`
2. Create handler implementing `IRequestHandler`
3. Register via MediatR auto-discovery
4. Add controller endpoint

### New Command
1. Create command record in `Application/Commands`
2. Create handler with business logic
3. Update aggregate if needed
4. Raise domain events
5. Add controller endpoint

## Code Quality

- **No warnings as errors** (TreatWarningsAsErrors)
- **Nullable reference types** enabled
- **Immutable value objects**
- **Explicit interfaces** everywhere
- **Sealed classes** by default

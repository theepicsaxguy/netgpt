# NetGPT Backend - Production-Ready ChatGPT Clone

## Architecture

### Clean Architecture with DDD
- **Domain Layer**: Aggregates, Value Objects, Domain Events, Business Logic
- **Application Layer**: CQRS Commands/Queries, Handlers, DTOs, Interfaces  
- **Infrastructure Layer**: Agent Framework Integration, EF Core, External Services
- **API Layer**: Controllers, SignalR Hubs, Middleware

### Key Design Patterns
- **SOLID**: Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion
- **DRY**: Reusable components, shared abstractions
- **CQRS**: Separate read and write models
- **Repository Pattern**: Data access abstraction
- **Unit of Work**: Transaction management
- **Result Pattern**: Error handling without exceptions

## Agent Framework Integration

### Microsoft Agent Framework (Microsoft.Agents.AI)
- Primary agent orchestration
- Multi-agent workflows
- Tool/Plugin system with runtime DI registration
- Streaming responses via SignalR

### Tools/Plugins (Registered at Runtime)
- Web Search Tool
- Code Execution Tool (Python, JavaScript)
- File Processing Tool (PDF, Images)
- Custom tools can be added by implementing methods with `[Description]` attributes

## Project Structure

```
backend/
├── src/
│   ├── NetGPT.Domain/           # Core business logic
│   │   ├── Aggregates/
│   │   ├── ValueObjects/
│   │   ├── Events/
│   │   ├── Interfaces/
│   │   └── Primitives/
│   ├── NetGPT.Application/      # Use cases
│   │   ├── Commands/
│   │   ├── Queries/
│   │   ├── Handlers/
│   │   ├── DTOs/
│   │   └── Mappings/
│   ├── NetGPT.Infrastructure/   # External concerns
│   │   ├── Agents/
│   │   ├── Tools/
│   │   ├── Persistence/
│   │   └── Configuration/
│   └── NetGPT.API/              # HTTP/SignalR interface
│       ├── Controllers/
│       └── Hubs/
```

## Getting Started

### Prerequisites
- .NET 8 SDK
- PostgreSQL 14+
- OpenAI API Key

### Configuration

Update `src/NetGPT.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=netgpt;Username=postgres;Password=yourpassword"
  },
  "OpenAI": {
    "ApiKey": "sk-your-openai-api-key",
    "DefaultModel": "gpt-4o",
    "MaxTokens": 4000
  }
}
```

### Run Migrations
```bash
cd src/NetGPT.API
dotnet ef migrations add InitialCreate --project ../NetGPT.Infrastructure
dotnet ef database update
```

### Run Application
```bash
# Using Makefile
make run

# Or directly
cd src/NetGPT.API
dotnet run
```

API: https://localhost:5001  
Swagger: https://localhost:5001/swagger  
SignalR Hub: wss://localhost:5001/hubs/conversation

### Build
```bash
# Using Makefile
make build

# Or directly
dotnet build NetGPT.sln
```

### Run with Docker
```bash
# Using Makefile
make docker-up

# Or directly
docker-compose up -d
```

## API Endpoints

### Conversations
- `POST /conversations` - Create conversation
- `GET /conversations` - List conversations (paginated)
- `GET /conversations/{id}` - Get conversation
- `DELETE /conversations/{id}` - Delete conversation

### Messages
- `POST /conversations/{id}/messages` - Send message (REST)
- WebSocket: `/hubs/conversation` - Streaming messages

## Adding Custom Tools/Plugins

Create a new plugin class:
```csharp
public sealed class CustomToolPlugin
{
    [Description("Your tool description")]
    public async Task<string> YourTool(
        [Description("Parameter description")] string param)
    {
        // Implementation
        return "result";
    }
}
```

Register in `Program.cs`:
```csharp
var customTools = AIFunctionFactory.Create(new CustomToolPlugin());
foreach (var tool in customTools)
{
    registry.RegisterTool(tool);
}
```

## Production Deployment

### Database
- Use connection pooling (PgBouncer)
- Enable read replicas for scaling
- Regular backups

### Application
- Deploy as Docker containers
- Use Kubernetes for orchestration
- Configure health checks
- Enable distributed tracing (OpenTelemetry)

### Security
- Implement JWT authentication
- Add rate limiting
- Enable HTTPS only
- Validate all inputs
- Sanitize user content before sending to OpenAI

## License
MIT

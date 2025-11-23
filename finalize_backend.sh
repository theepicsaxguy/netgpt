#!/bin/bash

# Fix Program.cs
cat > src/NetGPT.API/Program.cs << 'EOF'
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using NetGPT.API.Hubs;
using NetGPT.Application.Handlers;
using NetGPT.Application.Interfaces;
using NetGPT.Application.Mappings;
using NetGPT.Domain.Interfaces;
using NetGPT.Infrastructure.Agents;
using NetGPT.Infrastructure.Configuration;
using NetGPT.Infrastructure.Persistence;
using NetGPT.Infrastructure.Persistence.Repositories;
using NetGPT.Infrastructure.Tools;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Services.Configure<OpenAISettings>(
    builder.Configuration.GetSection("OpenAI"));

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<CreateConversationHandler>());

// Repositories
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Mappers
builder.Services.AddSingleton<IConversationMapper, ConversationMapper>();

// Agent Framework
builder.Services.AddSingleton<IToolRegistry, ToolRegistry>();
builder.Services.AddScoped<IAgentFactory, AgentFactory>();
builder.Services.AddScoped<IAgentOrchestrator, AgentOrchestrator>();

// Register Tool Plugins at Runtime (Flexible DI)
builder.Services.AddSingleton(sp =>
{
    var registry = sp.GetRequiredService<IToolRegistry>();
    
    // Web Search Tool
    var webSearchTools = AIFunctionFactory.Create(new WebSearchToolPlugin());
    foreach (var tool in webSearchTools)
    {
        registry.RegisterTool(tool);
    }
    
    // Code Execution Tools
    var codeTools = AIFunctionFactory.Create(new CodeExecutionToolPlugin());
    foreach (var tool in codeTools)
    {
        registry.RegisterTool(tool);
    }
    
    // File Processing Tools
    var fileTools = AIFunctionFactory.Create(new FileProcessingToolPlugin());
    foreach (var tool in fileTools)
    {
        registry.RegisterTool(tool);
    }
    
    return registry;
});

// API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "NetGPT API", Version = "v1" });
});

// SignalR
builder.Services.AddSignalR();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ConversationHub>("/hubs/conversation");

app.Run();
EOF

# Create README
cat > README.md << 'EOF'
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
NetGPT.Backend/
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

Update `appsettings.json`:
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
cd src/NetGPT.API
dotnet run
```

API: https://localhost:5001  
Swagger: https://localhost:5001/swagger  
SignalR Hub: wss://localhost:5001/hubs/conversation

## API Endpoints

### Conversations
- `POST /api/v1/conversations` - Create conversation
- `GET /api/v1/conversations` - List conversations (paginated)
- `GET /api/v1/conversations/{id}` - Get conversation
- `DELETE /api/v1/conversations/{id}` - Delete conversation

### Messages
- `POST /api/v1/conversations/{id}/messages` - Send message (REST)
- WebSocket: `/hubs/conversation` - Streaming messages

## Orval Code Generation

The API is designed for Orval to auto-generate TypeScript client:

```yaml
# orval.config.ts
export default {
  netgpt: {
    input: 'https://localhost:5001/swagger/v1/swagger.json',
    output: {
      mode: 'tags-split',
      target: 'src/api/generated',
      schemas: 'src/api/models',
      client: 'react-query',
    },
  },
};
```

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
EOF

# Create .gitignore
cat > .gitignore << 'EOF'
## .NET
bin/
obj/
*.user
*.suo
*.cache
*.log

## IDE
.vs/
.vscode/
.idea/
*.swp

## Build
[Dd]ebug/
[Rr]elease/
x64/
x86/

## Configuration
appsettings.Development.json
appsettings.Production.json
*.secrets.json

## Database
*.db
*.db-shm
*.db-wal

## NuGet
*.nupkg
*.snupkg
packages/
EOF

# Create Dockerfile
cat > Dockerfile << 'EOF'
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/NetGPT.API/NetGPT.API.csproj", "src/NetGPT.API/"]
COPY ["src/NetGPT.Application/NetGPT.Application.csproj", "src/NetGPT.Application/"]
COPY ["src/NetGPT.Domain/NetGPT.Domain.csproj", "src/NetGPT.Domain/"]
COPY ["src/NetGPT.Infrastructure/NetGPT.Infrastructure.csproj", "src/NetGPT.Infrastructure/"]
RUN dotnet restore "src/NetGPT.API/NetGPT.API.csproj"
COPY . .
WORKDIR "/src/src/NetGPT.API"
RUN dotnet build "NetGPT.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NetGPT.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetGPT.API.dll"]
EOF

# Create docker-compose.yml
cat > docker-compose.yml << 'EOF'
version: '3.8'

services:
  postgres:
    image: postgres:16
    environment:
      POSTGRES_DB: netgpt
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

  api:
    build: .
    ports:
      - "5000:80"
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=netgpt;Username=postgres;Password=postgres
      - OpenAI__ApiKey=${OPENAI_API_KEY}
    depends_on:
      - postgres

volumes:
  postgres_data:
EOF

echo "Backend finalized successfully"

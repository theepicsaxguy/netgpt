#!/bin/bash

# Dockerfile
cat > Dockerfile << 'EOF'
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/NetGPT.API/NetGPT.API.csproj", "NetGPT.API/"]
COPY ["src/NetGPT.Application/NetGPT.Application.csproj", "NetGPT.Application/"]
COPY ["src/NetGPT.Infrastructure/NetGPT.Infrastructure.csproj", "NetGPT.Infrastructure/"]
COPY ["src/NetGPT.Domain/NetGPT.Domain.csproj", "NetGPT.Domain/"]

RUN dotnet restore "NetGPT.API/NetGPT.API.csproj"

COPY src/ .
RUN dotnet build "NetGPT.API/NetGPT.API.csproj" -c Release -o /app/build
RUN dotnet publish "NetGPT.API/NetGPT.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NetGPT.API.dll"]
EOF

# docker-compose.yml
cat > docker-compose.yml << 'EOF'
version: '3.8'

services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: netgpt
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

  api:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=netgpt;Username=postgres;Password=postgres
      - OpenAI__ApiKey=${OPENAI_API_KEY}
    depends_on:
      - postgres

volumes:
  postgres_data:
EOF

# .gitignore
cat > .gitignore << 'EOF'
## .NET
bin/
obj/
*.user
*.suo
*.cache
*.dll
*.exe
*.pdb
*.log

## Visual Studio
.vs/
.vscode/

## Rider
.idea/

## User-specific files
*.rsuser
*.userosscache
*.sln.docstates

## Build results
[Dd]ebug/
[Rr]elease/
x64/
x86/
[Aa][Rr][Mm]/
[Aa][Rr][Mm]64/
bld/
[Bb]in/
[Oo]bj/

## Environment
.env
.env.local
appsettings.*.json
!appsettings.json
!appsettings.Development.json

## Database
*.db
*.sqlite

## OS
.DS_Store
Thumbs.db
EOF

# README.md
cat > README.md << 'EOF'
# NetGPT Backend

Production-ready ChatGPT clone backend using .NET 8, DDD, CQRS, and Microsoft Agent Framework.

## Architecture

- **Domain Layer**: Aggregates, Value Objects, Domain Events
- **Application Layer**: CQRS Commands/Queries, Handlers, DTOs
- **Infrastructure Layer**: EF Core, Agent Framework, Tools/Plugins
- **API Layer**: REST Controllers, SignalR Hubs

## Key Features

- ✅ Multi-agent orchestration with Microsoft.Agents.AI
- ✅ Dynamic tool/plugin registration at runtime
- ✅ Real-time streaming via SignalR
- ✅ Event-driven architecture with MediatR
- ✅ Repository pattern with Unit of Work
- ✅ Clean Architecture (DDD, SOLID, SRP, SOC)
- ✅ OpenAPI/Swagger for auto-generated clients (Orval)

## Prerequisites

- .NET 8 SDK
- PostgreSQL 16+
- OpenAI API Key

## Quick Start

### Using Docker Compose

```bash
export OPENAI_API_KEY="your-key-here"
docker-compose up
```

API available at `http://localhost:8080`

### Local Development

```bash
# Update connection string in appsettings.Development.json
dotnet ef database update --project src/NetGPT.Infrastructure --startup-project src/NetGPT.API

# Run
cd src/NetGPT.API
dotnet run
```

## API Endpoints

### Conversations
- `POST /api/v1/conversations` - Create conversation
- `GET /api/v1/conversations` - List conversations
- `GET /api/v1/conversations/{id}` - Get conversation
- `DELETE /api/v1/conversations/{id}` - Delete conversation

### Messages
- `POST /api/v1/conversations/{id}/messages` - Send message
- `GET /api/v1/conversations/{id}/messages` - Get messages

### WebSocket
- `ws://localhost:8080/hubs/conversation` - Real-time streaming

## Agent Framework Integration

Agents are configured with tools at runtime:

```csharp
// Tools auto-registered from plugins
var webSearchTools = AIFunctionFactory.Create(new WebSearchToolPlugin());
var codeTools = AIFunctionFactory.Create(new CodeExecutionToolPlugin());
var fileTools = AIFunctionFactory.Create(new FileProcessingToolPlugin());
```

## Adding New Tools

1. Create plugin class in `Infrastructure/Tools/`
2. Add methods with `[Description]` attributes
3. Register in `Program.cs` DI container

```csharp
public class MyToolPlugin
{
    [Description("Does something useful")]
    public async Task<string> MyTool(
        [Description("Input parameter")] string input)
    {
        // Implementation
        return "result";
    }
}
```

## Database Migrations

```bash
dotnet ef migrations add InitialCreate --project src/NetGPT.Infrastructure --startup-project src/NetGPT.API
dotnet ef database update --project src/NetGPT.Infrastructure --startup-project src/NetGPT.API
```

## OpenAPI / Orval

Swagger spec available at `/swagger/v1/swagger.json`

Generate TypeScript client:
```bash
npx orval --config orval.config.ts
```

## Project Structure

```
src/
├── NetGPT.Domain/          # Core business logic
│   ├── Aggregates/
│   ├── ValueObjects/
│   ├── Events/
│   └── Interfaces/
├── NetGPT.Application/     # Use cases
│   ├── Commands/
│   ├── Queries/
│   ├── Handlers/
│   └── DTOs/
├── NetGPT.Infrastructure/  # External concerns
│   ├── Persistence/
│   ├── Agents/
│   └── Tools/
└── NetGPT.API/            # Presentation
    ├── Controllers/
    └── Hubs/
```

## Environment Variables

```bash
OPENAI_API_KEY=sk-...
ConnectionStrings__DefaultConnection=Host=localhost;Database=netgpt;...
```

## Production Deployment

1. Set environment variables
2. Run database migrations
3. Deploy container to Kubernetes/Cloud Run/etc.

## License

MIT
EOF

# orval.config.ts for client generation
cat > orval.config.ts << 'EOF'
import { defineConfig } from 'orval';

export default defineConfig({
  netgpt: {
    input: 'http://localhost:8080/swagger/v1/swagger.json',
    output: {
      mode: 'tags-split',
      target: '../netgpt-client/src/api/generated',
      schemas: '../netgpt-client/src/api/models',
      client: 'react-query',
      mock: false,
      prettier: true,
      override: {
        mutator: {
          path: '../netgpt-client/src/api/client.ts',
          name: 'customInstance',
        },
      },
    },
  },
});
EOF

# .dockerignore
cat > .dockerignore << 'EOF'
**/.git
**/.vs
**/.vscode
**/bin
**/obj
**/*.user
**/.DS_Store
**/node_modules
EOF

echo "Deployment files created"

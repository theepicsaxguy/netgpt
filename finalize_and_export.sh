#!/bin/bash

# Create README
cat > README.md << 'EOF'
# NetGPT Backend

Production-ready ChatGPT clone using .NET 8, DDD, CQRS, and Microsoft Agent Framework.

## Architecture

- **Domain Layer**: Aggregates, Value Objects, Domain Events
- **Application Layer**: CQRS Commands/Queries, Handlers, DTOs
- **Infrastructure Layer**: Agent Framework, Tools/Plugins, Persistence
- **API Layer**: REST Controllers, SignalR Hubs

## Features

- Multi-agent orchestration using Microsoft.Agents.AI
- Runtime tool/plugin registration via DI
- Streaming responses via SignalR
- Event-driven architecture
- PostgreSQL with EF Core
- OpenAPI/Swagger for Orval code generation

## Setup

1. Set environment variable: `export OPENAI_API_KEY=your-key`
2. Update connection string in `appsettings.json`
3. Run migrations: `dotnet ef database update -p src/NetGPT.Infrastructure -s src/NetGPT.API`
4. Run: `dotnet run --project src/NetGPT.API`

## Agent Framework

Uses Microsoft.Agents.AI with OpenAI:
- Primary agent with tool orchestration
- Web search, code execution, file processing plugins
- Streaming responses
- Multi-turn conversations

## API Endpoints

- `POST /api/v1/conversations` - Create conversation
- `GET /api/v1/conversations/{id}` - Get conversation
- `GET /api/v1/conversations` - List conversations
- `POST /api/v1/conversations/{id}/messages` - Send message
- `DELETE /api/v1/conversations/{id}` - Delete conversation

## SignalR Hub

- Connect: `ws://localhost:5000/hubs/conversation`
- `SendMessage(conversationId, content)` - Stream agent response

## Principles

- DDD with Aggregates and Value Objects
- SOLID, SRP, SOC
- CQRS with MediatR
- Result pattern for error handling
- Max 200 lines per file
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
COPY ["src/NetGPT.Infrastructure/NetGPT.Infrastructure.csproj", "src/NetGPT.Infrastructure/"]
COPY ["src/NetGPT.Domain/NetGPT.Domain.csproj", "src/NetGPT.Domain/"]
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
  api:
    build: .
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=netgpt;Username=postgres;Password=postgres
      - OpenAI__ApiKey=${OPENAI_API_KEY}
    depends_on:
      - postgres

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

volumes:
  postgres_data:
EOF

# Create .gitignore
cat > .gitignore << 'EOF'
bin/
obj/
.vs/
.vscode/
*.user
*.suo
appsettings.Development.json
*.db
*.db-shm
*.db-wal
EOF

# Create global.json
cat > global.json << 'EOF'
{
  "sdk": {
    "version": "8.0.0",
    "rollForward": "latestMinor"
  }
}
EOF

echo "Finalization complete"

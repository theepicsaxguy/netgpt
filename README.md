# NetGPT - Production-Ready ChatGPT Clone

A full-stack ChatGPT clone with a production-ready .NET backend and React frontend.

## Project Structure

```
netgpt/
├── backend/          # .NET 8 Backend API
│   ├── src/
│   │   ├── NetGPT.Domain/
│   │   ├── NetGPT.Application/
│   │   ├── NetGPT.Infrastructure/
│   │   └── NetGPT.API/
│   ├── NetGPT.sln
│   ├── Dockerfile
│   └── docker-compose.yml
├── frontend/         # React Frontend (To be implemented)
│   └── src/
└── orval.config.ts   # API client code generation
```

## Backend

The backend is built with Clean Architecture and Domain-Driven Design principles.

### Features
- **Clean Architecture with DDD**: Separated concerns across Domain, Application, Infrastructure, and API layers
- **CQRS Pattern**: Command/Query separation using MediatR
- **Microsoft Agent Framework**: AI orchestration with multi-agent workflows
- **Real-time Streaming**: SignalR for streaming AI responses
- **Tool/Plugin System**: Extensible tools (Web Search, Code Execution, File Processing)
- **OpenAI Integration**: Standard OpenAI API (not Azure)
- **PostgreSQL Database**: Entity Framework Core with migrations

### Quick Start

```bash
cd backend

# Build the solution
make build

# Run the API
make run

# Or with Docker
make docker-up
```

See [backend/README.md](./backend/README.md) for detailed backend documentation.

## Frontend

The frontend will be a modern React application with TypeScript.

### Planned Features
- Real-time chat interface with streaming responses
- Conversation management
- File upload support
- Markdown and code syntax highlighting
- SignalR WebSocket integration

### Tech Stack (Planned)
- React with TypeScript
- React Query for state management
- Tailwind CSS
- Orval for auto-generated API client

See [frontend/README.md](./frontend/README.md) for frontend setup instructions.

## API Client Generation

The project uses Orval to auto-generate TypeScript API clients from the Swagger specification:

```bash
# Generate API client (when backend is running)
npx orval
```

Configuration is in `orval.config.ts` at the root.

## Development

### Prerequisites
- .NET 8 SDK
- Node.js 18+ (for frontend)
- PostgreSQL 14+
- OpenAI API Key

### Getting Started

1. **Start the backend:**
   ```bash
   cd backend
   # Update appsettings.json with your database and OpenAI API key
   make run
   ```

2. **Access the API:**
   - API: https://localhost:5001
   - Swagger: https://localhost:5001/swagger

3. **Generate API client for frontend:**
   ```bash
   npx orval
   ```

## Architecture

- **Backend**: Clean Architecture with SOLID principles, CQRS, Repository Pattern
- **Frontend**: Component-based architecture with React Query for server state
- **API**: RESTful endpoints + SignalR for real-time streaming
- **Database**: PostgreSQL with EF Core

See [ARCHITECTURE.md](./ARCHITECTURE.md) for detailed architecture documentation.

## Contributing

See [CONTRIBUTING.md](./CONTRIBUTING.md) for contribution guidelines.

## License

MIT

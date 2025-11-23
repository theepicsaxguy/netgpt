# NetGPT Frontend

This directory is prepared for the NetGPT frontend application.

## Setup (To Be Implemented)

The frontend will be a React application that consumes the NetGPT Backend API.

### Planned Features
- Real-time chat interface with streaming responses
- Conversation management
- File upload support
- Markdown rendering
- Code syntax highlighting
- WebSocket connection for live updates

### Tech Stack (Planned)
- React with TypeScript
- React Query for API state management
- Tailwind CSS for styling
- SignalR client for real-time communication
- Orval for API client generation

## Development

```bash
# Install dependencies (when package.json is added)
npm install

# Start development server
npm run dev

# Build for production
npm run build
```

## API Integration

The frontend will use the auto-generated API client from Orval, configured in the root `orval.config.ts` file.

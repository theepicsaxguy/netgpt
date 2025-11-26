# NetGPT Document Ingestion Service

A Python/FastAPI microservice for ingesting text documents and serving semantic queries using Chonkie, FastEmbed, and Qdrant.

## Directory Structure

The service should be organized as follows:

```
netgpt-docs-service/
├── Dockerfile
├── requirements.txt
└── app/
    ├── main.py
    ├── config.py
    ├── models.py
    └── services.py
```

## Features

- **Document Ingestion**: Parse and chunk text documents using Chonkie's RecursiveChunker
- **Vector Embeddings**: Generate embeddings using FastEmbed (BAAI/bge-small-en-v1.5 by default)
- **Vector Storage**: Store and retrieve embeddings using Qdrant vector database
- **Semantic Search**: Query documents by semantic similarity

## API Endpoints

- `POST /ingest` - Ingest a document (splits text into chunks, embeds them, and stores in Qdrant)
  - Request body: `{"doc_id": "string", "text": "string"}`
  - Response: `{"status": "success", "chunks_ingested": number}`

- `POST /query` - Query the vector database with a text string
  - Request body: `{"query": "string"}`
  - Response: `[{"doc_id": "string", "chunk": "string", "score": number}]`

- `GET /health` - Health check endpoint
  - Response: `{"status": "healthy"}`

## Configuration

All configuration is via environment variables:

- `QDRANT_URL` - Qdrant connection URL (default: `http://qdrant.pc-tips.se:6333`)
- `QDRANT_API_KEY` - Qdrant API key (if authentication is required)
- `EMBEDDING_MODEL` - FastEmbed model name (default: `BAAI/bge-small-en-v1.5`)
- `COLLECTION_NAME` - Qdrant collection name (default: `netgpt_documents`)
- `CHUNK_SIZE` - Maximum tokens per chunk (default: `512`)

## Running with Docker

```bash
# Build the image
docker build -t netgpt-docs-service -f docs-service-dockerfile .

# Run the container
docker run -p 8000:8000 \
  -e QDRANT_URL=http://qdrant.pc-tips.se:6333 \
  -e EMBEDDING_MODEL=BAAI/bge-small-en-v1.5 \
  netgpt-docs-service
```

## Development

```bash
# Install dependencies
pip install -r docs-service-requirements.txt

# Run the service
uvicorn app.main:app --reload --host 0.0.0.0 --port 8000
```

## Example Usage

### Ingest a document

```bash
curl -X POST http://localhost:8000/ingest \
  -H "Content-Type: application/json" \
  -d '{
    "doc_id": "doc123",
    "text": "Your document text here. This will be chunked and embedded."
  }'
```

Response:
```json
{
  "status": "success",
  "chunks_ingested": 5
}
```

### Query documents

```bash
curl -X POST http://localhost:8000/query \
  -H "Content-Type: application/json" \
  -d '{
    "query": "search query here"
  }'
```

Response:
```json
[
  {
    "doc_id": "doc123",
    "chunk": "Relevant text chunk...",
    "score": 0.95
  }
]
```

### Health check

```bash
curl http://localhost:8000/health
```

Response:
```json
{
  "status": "healthy"
}
```

## Integration with NetGPT

This service is designed to integrate with the NetGPT monorepo as a Docker microservice:

1. **Containerization**: The service runs in its own Docker container
2. **REST API**: Exposes HTTP/JSON endpoints that can be called by other services
3. **Environment Configuration**: Follows 12-factor app principles with env-based config
4. **Logging**: Writes structured logs to stdout for platform collection
5. **Health Checks**: Provides `/health` endpoint for container orchestration

## Architecture

The service follows these patterns:

- **Chunking**: Uses Chonkie's `RecursiveChunker` to split documents into semantic chunks
- **Embedding**: Uses FastEmbed's `TextEmbedding` for fast, local embedding generation
- **Vector Database**: Uses Qdrant Python client for vector storage and similarity search
- **Similarity Metric**: Uses cosine distance for semantic similarity matching

## Dependencies

- `fastapi` - Modern web framework for building APIs
- `uvicorn[standard]` - ASGI server for running FastAPI
- `chonkie` - Text chunking library
- `fastembed` - Fast embedding generation
- `qdrant-client[fastembed]` - Qdrant vector database client

## File Organization

To organize the service files into the proper directory structure:

1. Create directory: `netgpt-docs-service/`
2. Move `docs-service-dockerfile` → `netgpt-docs-service/Dockerfile`
3. Move `docs-service-requirements.txt` → `netgpt-docs-service/requirements.txt`
4. Create directory: `netgpt-docs-service/app/`
5. Move `docs-service-main.py` → `netgpt-docs-service/app/main.py`
6. Move `docs-service-config.py` → `netgpt-docs-service/app/config.py`
7. Move `docs-service-models.py` → `netgpt-docs-service/app/models.py`
8. Move `docs-service-services.py` → `netgpt-docs-service/app/services.py`
9. Add `netgpt-docs-service/app/__init__.py` (empty file for Python package)

## Notes

- The service connects to Qdrant at `http://qdrant.pc-tips.se:6333` by default
- The first document ingestion will create the Qdrant collection automatically
- Vector dimensions are determined by the embedding model (384 for bge-small-en-v1.5)
- Authentication can be added via `QDRANT_API_KEY` environment variable
- For production use, consider adding rate limiting, authentication, and request validation

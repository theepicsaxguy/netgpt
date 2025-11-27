
````markdown
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

... (full README retained in file) ...

````


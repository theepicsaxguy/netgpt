# Document Service Architecture

This document describes the architecture and design decisions of the NetGPT Document Ingestion Service.

## Overview

The document service is a microservice designed to provide semantic search capabilities to the NetGPT system. It handles document ingestion, chunking, embedding generation, vector storage, and similarity-based retrieval.

## System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      NetGPT Services                         │
│  (.NET Backend, React Frontend, Agent Framework, etc.)      │
└────────────────────────┬────────────────────────────────────┘
                         │ HTTP/JSON
                         │ REST API
                         ▼
┌─────────────────────────────────────────────────────────────┐
│            Document Ingestion Service (FastAPI)             │
│                                                              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │   /ingest    │  │    /query    │  │   /health    │     │
│  │   endpoint   │  │   endpoint   │  │   endpoint   │     │
│  └──────┬───────┘  └──────┬───────┘  └──────────────┘     │
│         │                  │                                 │
│         ▼                  ▼                                 │
│  ┌──────────────────────────────────────────────────┐      │
│  │              Services Layer                       │      │
│  │  - ingest_document()                             │      │
│  │  - query_text()                                  │      │
│  │  - setup_collection()                            │      │
│  └──────┬───────────────────────────────┬───────────┘      │
│         │                                │                  │
│         ▼                                ▼                  │
│  ┌─────────────┐   ┌──────────────┐   ┌──────────────┐   │
│  │   Chonkie   │   │  FastEmbed   │   │    Qdrant    │   │
│  │  (Chunker)  │   │ (Embedder)   │   │   (Client)   │   │
│  └─────────────┘   └──────────────┘   └──────┬───────┘   │
└─────────────────────────────────────────────────┼──────────┘
                                                  │ gRPC/HTTP
                                                  ▼
                                    ┌──────────────────────┐
                                    │  Qdrant Vector DB    │
                                    │  (External Service)  │
                                    └──────────────────────┘
```

## Components

### 1. API Layer (main.py)

The FastAPI application exposes three endpoints:

- **POST /ingest**: Accepts a document (ID + text), processes it, and stores embeddings
- **POST /query**: Accepts a query string, returns semantically similar document chunks
- **GET /health**: Returns service health status

**Design Decisions:**
- Used FastAPI for automatic OpenAPI documentation and Pydantic validation
- RESTful design for easy integration with other services
- Async-capable (though current implementation uses sync clients)
- Follows HTTP status code conventions (200, 400, 500)

### 2. Models Layer (models.py)

Pydantic models for request/response validation:

- **DocumentIn**: Input model for document ingestion (doc_id, text)
- **QueryIn**: Input model for queries (query string)
- **SearchResult**: Output model for query results (doc_id, chunk, score)

**Design Decisions:**
- Strong typing with Pydantic ensures data validation
- Simple, focused models for each operation
- JSON-serializable for HTTP transport

### 3. Services Layer (services.py)

Core business logic:

- **setup_collection()**: Ensures Qdrant collection exists with correct schema
- **ingest_document()**: Complete ingestion pipeline
- **query_text()**: Semantic search pipeline

**Design Decisions:**
- Separation of concerns: each function has a single responsibility
- Lazy initialization: collection created on first use
- Error handling with exceptions (caught at API layer)
- Logging for observability

### 4. Configuration Layer (config.py)

Environment-based configuration following 12-factor app principles:

- QDRANT_URL: Vector database connection
- QDRANT_API_KEY: Authentication (optional)
- EMBEDDING_MODEL: Which FastEmbed model to use
- COLLECTION_NAME: Qdrant collection name
- CHUNK_SIZE: Token limit per chunk

**Design Decisions:**
- All configuration via environment variables
- Sensible defaults for development
- No hardcoded credentials or URLs

## Data Flow

### Ingestion Pipeline

```
1. Document received (doc_id + text)
                ↓
2. Text chunked using Chonkie RecursiveChunker
   - Recursive splitting for semantic coherence
   - Max tokens per chunk (default: 512)
   - GPT-2 tokenizer for accurate token counting
                ↓
3. Chunks embedded using FastEmbed
   - All chunks embedded in batch
   - Model: BAAI/bge-small-en-v1.5 (default)
   - Output: 384-dimensional vectors
                ↓
4. Collection setup (if needed)
   - Creates collection with correct dimensions
   - Cosine distance for similarity
                ↓
5. Vectors + payloads uploaded to Qdrant
   - Batch upload for efficiency
   - Payload: {doc_id, chunk_text}
   - Qdrant auto-generates point IDs
                ↓
6. Return success + chunk count
```

### Query Pipeline

```
1. Query text received
                ↓
2. Query embedded using FastEmbed
   - Same model as ingestion
   - Single vector (384-dim)
                ↓
3. Similarity search in Qdrant
   - Cosine similarity
   - Top-K results (default: 5)
   - Includes payloads
                ↓
4. Results formatted as SearchResult objects
   - doc_id, chunk text, similarity score
                ↓
5. Return ranked results
```

## Technology Choices

### FastAPI
**Why?**
- Modern, fast, async-capable Python web framework
- Automatic OpenAPI/Swagger documentation
- Built-in validation with Pydantic
- Easy to integrate with existing services
- Production-ready with Uvicorn

**Alternatives considered:**
- Flask: Less modern, no async support, no automatic docs
- Django: Too heavyweight for a microservice

### Chonkie
**Why?**
- Semantic chunking strategies
- Token-aware splitting
- Multiple chunking algorithms
- Preserves context boundaries

**Alternatives considered:**
- LangChain: Too heavy, many dependencies
- Custom splitting: Not semantic, hard to tune

### FastEmbed
**Why?**
- Fast, lightweight embedding generation
- Runs locally (no API calls)
- Multiple model options
- Compatible with Qdrant
- Low latency

**Alternatives considered:**
- OpenAI API: Costs money, API latency, rate limits
- Sentence-Transformers: Similar but FastEmbed is optimized

### Qdrant
**Why?**
- Purpose-built for vector search
- High performance
- Easy to deploy (Docker image)
- Good Python client
- Supports filtering, multi-vector, etc.

**Alternatives considered:**
- Pinecone: Cloud-only, costs money
- Weaviate: More complex, heavier
- PostgreSQL + pgvector: Not specialized, slower

## Scaling Considerations

### Vertical Scaling
- Increase CPU for faster embedding generation
- Increase memory for larger batches
- Use GPU for embedding (requires GPU-enabled FastEmbed/ONNX)

### Horizontal Scaling
- Multiple service instances behind load balancer
- Shared Qdrant cluster
- Session affinity not required (stateless service)

### Performance Optimizations
- Batch embeddings (already done)
- Cache frequent queries in Redis
- Use quantization in Qdrant for lower memory
- Use smaller embedding models for speed
- Async processing for large ingestion

## Security Considerations

### Current Implementation
- No authentication (TODO)
- No authorization (TODO)
- No rate limiting (TODO)
- No input size limits (TODO)

### Recommended for Production
- Add API key authentication
- Implement rate limiting (per API key)
- Validate and sanitize inputs
- Add request size limits
- Enable HTTPS/TLS
- Use Qdrant API key authentication
- Implement CORS properly for web clients

## Monitoring and Observability

### Logging
- Structured logging to stdout
- Log levels: INFO for operations, ERROR for failures
- Includes operation context (doc_id, query, etc.)

### Metrics to Track
- Request count (by endpoint)
- Request latency (P50, P95, P99)
- Error rate
- Document ingestion rate
- Query success rate
- Embedding generation time
- Qdrant response time
- Storage size

### Health Checks
- /health endpoint for liveness
- Can be extended to check Qdrant connectivity

## Error Handling

### Client Errors (4xx)
- 400: Invalid input (empty text, missing fields)
- 404: Document not found (future)

### Server Errors (5xx)
- 500: Internal error (Qdrant down, embedding failure, etc.)

### Retry Strategy
- Clients should retry on 5xx with exponential backoff
- Idempotent ingestion (same doc_id overwrites)

## Testing Strategy

### Unit Tests
- Test chunking logic
- Test embedding generation
- Test Qdrant client calls
- Mock external dependencies

### Integration Tests
- Test full ingestion pipeline
- Test query pipeline
- Test against real Qdrant instance

### Load Tests
- Concurrent ingestion requests
- Concurrent query requests
- Large document handling
- Query latency under load

## Future Enhancements

### Near-term
- [ ] Add authentication
- [ ] Add rate limiting
- [ ] Add input validation (size limits)
- [ ] Add metadata filtering in queries
- [ ] Add pagination for query results

### Long-term
- [ ] Support for PDF, DOCX ingestion
- [ ] Support for image embeddings
- [ ] Multi-language support
- [ ] Advanced chunking strategies
- [ ] Async processing with message queue
- [ ] Caching layer
- [ ] A/B testing different embedding models
- [ ] Fine-tuned embeddings for domain

## Integration Patterns

### Pattern 1: RAG (Retrieval Augmented Generation)
```
User query → Query docs service → Get relevant chunks → 
  Add to prompt → Send to LLM → Return response
```

### Pattern 2: Conversation Archival
```
Conversation ends → Format as text → Ingest → 
  Available for future searches
```

### Pattern 3: Knowledge Base
```
User uploads doc → Ingest → User queries → 
  Return relevant sections
```

## Deployment Architecture

```
┌──────────────────────────────────────────────┐
│          Docker Compose / Kubernetes         │
│                                              │
│  ┌────────────┐  ┌────────────┐            │
│  │   Docs     │  │   Docs     │            │
│  │ Service 1  │  │ Service 2  │  (scaled)  │
│  └─────┬──────┘  └─────┬──────┘            │
│        │                │                    │
│        └────────┬───────┘                    │
│                 │                            │
│          ┌──────▼──────┐                    │
│          │   Qdrant    │  (clustered)       │
│          │   Cluster   │                    │
│          └─────────────┘                    │
│                                              │
└──────────────────────────────────────────────┘
```

## Conclusion

The document service is designed to be:
- **Simple**: Focused on one task, minimal dependencies
- **Fast**: Optimized for low latency
- **Scalable**: Stateless, horizontally scalable
- **Reliable**: Error handling, health checks
- **Observable**: Logging, metrics
- **Maintainable**: Clean architecture, well-documented
- **Integrable**: REST API, standard patterns

It follows microservice best practices and is production-ready with minimal additional work (mainly security and monitoring).

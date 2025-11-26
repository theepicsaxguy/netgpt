# Setup Guide: Document Ingestion Service

This guide walks through setting up the document-embedding microservice in the NetGPT monorepo.

## Quick Setup

### Step 1: Organize Files

Create the directory structure and move files:

```bash
# Create the service directory
mkdir -p netgpt-docs-service/app

# Move files to proper locations
mv docs-service-dockerfile netgpt-docs-service/Dockerfile
mv docs-service-requirements.txt netgpt-docs-service/requirements.txt
mv docs-service-main.py netgpt-docs-service/app/main.py
mv docs-service-config.py netgpt-docs-service/app/config.py
mv docs-service-models.py netgpt-docs-service/app/models.py
mv docs-service-services.py netgpt-docs-service/app/services.py
mv docs-service-app-init.py netgpt-docs-service/app/__init__.py
mv docs-service-README.md netgpt-docs-service/README.md

# Move documentation to docs folder (if exists)
# mv docs-service-INTEGRATION.md docs/
# mv docs-service-SETUP.md docs/
```

### Step 2: Build and Run with Docker

```bash
# Navigate to the service directory
cd netgpt-docs-service

# Build the Docker image
docker build -t netgpt-docs-service .

# Run Qdrant (if not already running)
docker run -d \
  --name netgpt-qdrant \
  -p 6333:6333 \
  -p 6334:6334 \
  -v qdrant-storage:/qdrant/storage \
  qdrant/qdrant:latest

# Run the document service
docker run -d \
  --name netgpt-docs-service \
  -p 8000:8000 \
  -e QDRANT_URL=http://host.docker.internal:6333 \
  -e EMBEDDING_MODEL=BAAI/bge-small-en-v1.5 \
  -e COLLECTION_NAME=netgpt_documents \
  -e CHUNK_SIZE=512 \
  netgpt-docs-service

# Check if it's running
curl http://localhost:8000/health
```

### Step 3: Test the Service

```bash
# Ingest a test document
curl -X POST http://localhost:8000/ingest \
  -H "Content-Type: application/json" \
  -d '{
    "doc_id": "test_doc_1",
    "text": "Artificial intelligence and machine learning are transforming how we build software. Neural networks can learn patterns from data and make predictions."
  }'

# Query the document
curl -X POST http://localhost:8000/query \
  -H "Content-Type: application/json" \
  -d '{
    "query": "machine learning"
  }'
```

## Development Setup

### Local Development (without Docker)

```bash
# Create virtual environment
python3 -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate

# Install dependencies
cd netgpt-docs-service
pip install -r requirements.txt

# Set environment variables
export QDRANT_URL=http://localhost:6333
export EMBEDDING_MODEL=BAAI/bge-small-en-v1.5
export COLLECTION_NAME=netgpt_documents
export CHUNK_SIZE=512

# Run the service
uvicorn app.main:app --reload --host 0.0.0.0 --port 8000
```

### Running Qdrant Locally

```bash
# Option 1: Docker
docker run -p 6333:6333 -p 6334:6334 \
  -v $(pwd)/qdrant_storage:/qdrant/storage \
  qdrant/qdrant:latest

# Option 2: Docker Compose (recommended)
# See docker-compose-snippet.yml for full configuration
```

## Docker Compose Integration

### Option A: Add to Existing docker-compose.yml

If you have an existing `docker-compose.yml`, add the service from `docs-service-docker-compose-snippet.yml`:

```yaml
services:
  # ... your existing services ...
  
  docs-service:
    build:
      context: ./netgpt-docs-service
      dockerfile: Dockerfile
    container_name: netgpt-docs-service
    ports:
      - "8000:8000"
    environment:
      - QDRANT_URL=http://qdrant:6333
      - EMBEDDING_MODEL=BAAI/bge-small-en-v1.5
      - COLLECTION_NAME=netgpt_documents
      - CHUNK_SIZE=512
    depends_on:
      - qdrant
    restart: unless-stopped
    networks:
      - netgpt-network

  qdrant:
    image: qdrant/qdrant:latest
    container_name: netgpt-qdrant
    ports:
      - "6333:6333"
      - "6334:6334"
    volumes:
      - qdrant-storage:/qdrant/storage
    restart: unless-stopped
    networks:
      - netgpt-network

volumes:
  qdrant-storage:
```

Then run:

```bash
docker-compose up -d docs-service qdrant
```

### Option B: Standalone docker-compose.yml

Create a new `docker-compose.yml` in `netgpt-docs-service/`:

```yaml
version: '3.8'

services:
  docs-service:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: netgpt-docs-service
    ports:
      - "8000:8000"
    environment:
      - QDRANT_URL=http://qdrant:6333
      - EMBEDDING_MODEL=BAAI/bge-small-en-v1.5
      - COLLECTION_NAME=netgpt_documents
      - CHUNK_SIZE=512
    depends_on:
      - qdrant
    restart: unless-stopped

  qdrant:
    image: qdrant/qdrant:latest
    container_name: netgpt-qdrant
    ports:
      - "6333:6333"
      - "6334:6334"
    volumes:
      - qdrant-storage:/qdrant/storage
    restart: unless-stopped

volumes:
  qdrant-storage:
```

Then run:

```bash
cd netgpt-docs-service
docker-compose up -d
```

## Configuration Options

### Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `QDRANT_URL` | `http://qdrant.pc-tips.se:6333` | Qdrant server URL |
| `QDRANT_API_KEY` | None | API key for Qdrant authentication |
| `EMBEDDING_MODEL` | `BAAI/bge-small-en-v1.5` | FastEmbed model name |
| `COLLECTION_NAME` | `netgpt_documents` | Qdrant collection name |
| `CHUNK_SIZE` | `512` | Maximum tokens per chunk |

### Available Embedding Models

FastEmbed supports various models. Common options:

| Model | Size | Speed | Quality |
|-------|------|-------|---------|
| `BAAI/bge-small-en-v1.5` | Small | Fast | Good |
| `BAAI/bge-base-en-v1.5` | Medium | Medium | Better |
| `BAAI/bge-large-en-v1.5` | Large | Slow | Best |
| `sentence-transformers/all-MiniLM-L6-v2` | Small | Fast | Good |

To change the model:

```bash
docker run -e EMBEDDING_MODEL=BAAI/bge-base-en-v1.5 netgpt-docs-service
```

## Verification

### 1. Check Service Health

```bash
curl http://localhost:8000/health
# Expected: {"status":"healthy"}
```

### 2. View API Documentation

Open in browser:
- Swagger UI: http://localhost:8000/docs
- ReDoc: http://localhost:8000/redoc

### 3. Check Qdrant

Open in browser:
- Qdrant Dashboard: http://localhost:6333/dashboard
- Collections API: http://localhost:6333/collections

### 4. View Logs

```bash
# Docker
docker logs netgpt-docs-service
docker logs netgpt-qdrant

# Docker Compose
docker-compose logs docs-service
docker-compose logs qdrant
```

## Troubleshooting

### Issue: Service won't start

**Check logs:**
```bash
docker logs netgpt-docs-service
```

**Common causes:**
- Port 8000 already in use → Change port mapping: `-p 8001:8000`
- Missing dependencies → Rebuild image: `docker build --no-cache`
- Network issues → Check Docker network configuration

### Issue: Cannot connect to Qdrant

**Check Qdrant is running:**
```bash
docker ps | grep qdrant
curl http://localhost:6333/collections
```

**Fix connection URL:**
- Inside Docker: use service name `http://qdrant:6333`
- From host: use `http://localhost:6333`
- Between containers: ensure they're on same network

### Issue: Slow embedding generation

**Solutions:**
- Use smaller model: `BAAI/bge-small-en-v1.5`
- Reduce chunk size: `CHUNK_SIZE=256`
- Increase Docker resources (CPU/RAM)

### Issue: Empty query results

**Verify data was ingested:**
```bash
# Check Qdrant collections
curl http://localhost:6333/collections/netgpt_documents
```

**Common causes:**
- Documents not ingested → Check ingestion response
- Wrong collection name → Verify `COLLECTION_NAME` matches
- Different embedding model → Use same model for ingest and query

## Production Deployment

### Security Checklist

- [ ] Enable Qdrant authentication (`QDRANT_API_KEY`)
- [ ] Add API authentication to document service
- [ ] Use HTTPS/TLS for all connections
- [ ] Implement rate limiting
- [ ] Configure proper CORS settings
- [ ] Use secrets management (not env vars in docker-compose)
- [ ] Enable logging and monitoring
- [ ] Set up backup for Qdrant storage

### Performance Optimization

- [ ] Use persistent volumes for Qdrant
- [ ] Configure Qdrant for production (quantization, etc.)
- [ ] Set appropriate resource limits (CPU, memory)
- [ ] Enable connection pooling
- [ ] Add caching layer (Redis) for frequent queries
- [ ] Use load balancer for multiple instances

### Monitoring

Key metrics to track:
- Request latency (P50, P95, P99)
- Error rate
- Document ingestion rate
- Query success rate
- Qdrant storage usage
- Memory/CPU usage

Recommended tools:
- Prometheus + Grafana for metrics
- ELK Stack for logs
- Sentry for error tracking

## Next Steps

1. **Test the Integration**: See `docs-service-INTEGRATION.md` for C# client examples
2. **Add Authentication**: Implement JWT/OAuth if needed
3. **Scale**: Add multiple instances behind a load balancer
4. **Monitor**: Set up logging and metrics collection
5. **Optimize**: Tune embedding model and chunk size for your use case

## Support

For issues or questions:
1. Check the logs first
2. Review this documentation
3. Check Qdrant documentation: https://qdrant.tech/documentation/
4. Check FastEmbed documentation: https://qdrant.github.io/fastembed/
5. Open an issue in the repository

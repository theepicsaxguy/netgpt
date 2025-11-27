# Production Deployment Guide

This guide covers deploying the NetGPT Document Ingestion Service to production environments.

## Prerequisites

- Docker and Docker Compose installed
- Qdrant instance (self-hosted or cloud)
- SSL/TLS certificates (for HTTPS)
- Monitoring infrastructure (Prometheus, Grafana, etc.)

## Deployment Options

### Option 1: Docker Compose (Recommended for small/medium deployments)

```yaml
version: '3.8'

services:
  docs-service:
    build:
      context: ./netgpt-docs-service
      dockerfile: Dockerfile
    image: netgpt-docs-service:latest
    container_name: netgpt-docs-service
    restart: always
    ports:
      - "8000:8000"
    environment:
      - QDRANT_URL=${QDRANT_URL}
      - QDRANT_API_KEY=${QDRANT_API_KEY}
      - EMBEDDING_MODEL=BAAI/bge-small-en-v1.5
      - COLLECTION_NAME=netgpt_documents
      - CHUNK_SIZE=512
    depends_on:
      - qdrant
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8000/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
    logging:
      driver: "json-file"
      options:
        max-size: "10m"
        max-file: "3"
    resources:
      limits:
        cpus: '2'
        memory: 4G
      reservations:
        cpus: '1'
        memory: 2G

  qdrant:
    image: qdrant/qdrant:latest
    container_name: netgpt-qdrant
    restart: always
    ports:
      - "6333:6333"
      - "6334:6334"
    volumes:
      - qdrant-data:/qdrant/storage
      - ./qdrant-config.yaml:/qdrant/config/production.yaml
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:6333/health"]
      interval: 30s
      timeout: 10s
      retries: 3
    logging:
      driver: "json-file"
      options:
        max-size: "10m"
        max-file: "3"
    resources:
      limits:
        cpus: '4'
        memory: 8G
      reservations:
        cpus: '2'
        memory: 4G

volumes:
  qdrant-data:
    driver: local

networks:
  default:
    name: netgpt-network
```

### Option 2: Kubernetes (Recommended for large deployments)

#### Deployment manifest:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: docs-service
  namespace: netgpt
spec:
  replicas: 3
  selector:
    matchLabels:
      app: docs-service
  template:
    metadata:
      labels:
        app: docs-service
    spec:
      containers:
      - name: docs-service
        image: netgpt-docs-service:latest
        ports:
        - containerPort: 8000
        env:
        - name: QDRANT_URL
          valueFrom:
            configMapKeyRef:
              name: docs-service-config
              key: qdrant-url
        - name: QDRANT_API_KEY
          valueFrom:
            secretKeyRef:
              name: docs-service-secrets
              key: qdrant-api-key
        - name: EMBEDDING_MODEL
          value: "BAAI/bge-small-en-v1.5"
        - name: COLLECTION_NAME
          value: "netgpt_documents"
        - name: CHUNK_SIZE
          value: "512"
        resources:
          requests:
            memory: "2Gi"
            cpu: "1000m"
          limits:
            memory: "4Gi"
            cpu: "2000m"
        livenessProbe:
          httpGet:
            path: /health
            port: 8000
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: 8000
          initialDelaySeconds: 10
          periodSeconds: 5

---
apiVersion: v1
kind: Service
metadata:
  name: docs-service
  namespace: netgpt
spec:
  selector:
    app: docs-service
  ports:
  - protocol: TCP
    port: 80
    targetPort: 8000
  type: ClusterIP

---
apiVersion: v1
kind: ConfigMap
metadata:
  name: docs-service-config
  namespace: netgpt
data:
  qdrant-url: "http://qdrant:6333"

---
apiVersion: v1
kind: Secret
metadata:
  name: docs-service-secrets
  namespace: netgpt
type: Opaque
stringData:
  qdrant-api-key: "your-api-key-here"

---
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: docs-service-hpa
  namespace: netgpt
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: docs-service
  minReplicas: 3
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

## Configuration

### Environment Variables (Production)

```bash
# Qdrant Configuration
QDRANT_URL=https://your-qdrant-instance.com:6333
QDRANT_API_KEY=your-secure-api-key-here

# Model Configuration
EMBEDDING_MODEL=BAAI/bge-small-en-v1.5  # or bge-base for better quality
COLLECTION_NAME=netgpt_documents_prod

# Chunking Configuration
CHUNK_SIZE=512  # 512 for speed, 1024 for quality
```

### Qdrant Configuration (qdrant-config.yaml)

```yaml
service:
  host: 0.0.0.0
  http_port: 6333
  grpc_port: 6334

storage:
  storage_path: /qdrant/storage
  snapshots_path: /qdrant/snapshots
  
  # Performance tuning
  performance:
    max_search_threads: 4
    max_optimization_threads: 2
  
  # Enable quantization for memory efficiency
  quantization:
    rescore: true
    oversampling: 2.0

  # WAL settings for durability
  wal:
    wal_capacity_mb: 32
    wal_segments_ahead: 0

# Enable API key authentication
api_key: ${QDRANT_API_KEY}

# CORS for web clients (adjust as needed)
cors:
  allow_origins:
    - "https://yourapp.com"
  allow_methods:
    - GET
    - POST
  allow_headers:
    - "*"
```

## Security Hardening

### 1. Enable Authentication

Add to `main.py`:

```python
from fastapi import Security, HTTPException
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
import os

security = HTTPBearer()
API_KEY = os.getenv("API_KEY", "")

async def verify_token(credentials: HTTPAuthorizationCredentials = Security(security)):
    if credentials.credentials != API_KEY:
        raise HTTPException(status_code=401, detail="Invalid API key")
    return credentials.credentials

# Update endpoints
@app.post("/ingest")
def ingest_endpoint(doc: DocumentIn, token: str = Security(verify_token)):
    # ... existing code

@app.post("/query", response_model=list[SearchResult])
def query_endpoint(query: QueryIn, token: str = Security(verify_token)):
    # ... existing code
```

### 2. Add Rate Limiting

```python
from slowapi import Limiter, _rate_limit_exceeded_handler
from slowapi.util import get_remote_address
from slowapi.errors import RateLimitExceeded

limiter = Limiter(key_func=get_remote_address)
app.state.limiter = limiter
app.add_exception_handler(RateLimitExceeded, _rate_limit_exceeded_handler)

@app.post("/ingest")
@limiter.limit("10/minute")
def ingest_endpoint(request: Request, doc: DocumentIn):
    # ... existing code

@app.post("/query")
@limiter.limit("30/minute")
def query_endpoint(request: Request, query: QueryIn):
    # ... existing code
```

### 3. Input Validation

```python
from pydantic import validator

class DocumentIn(BaseModel):
    doc_id: str
    text: str
    
    @validator('text')
    def text_not_empty(cls, v):
        if not v or len(v.strip()) == 0:
            raise ValueError('Text cannot be empty')
        if len(v) > 1_000_000:  # 1MB limit
            raise ValueError('Text too large (max 1MB)')
        return v
    
    @validator('doc_id')
    def doc_id_valid(cls, v):
        if not v or len(v) > 255:
            raise ValueError('Invalid doc_id')
        return v
```

### 4. HTTPS/TLS

Use a reverse proxy (Nginx, Traefik, or cloud load balancer):

```nginx
server {
    listen 443 ssl http2;
    server_name docs-service.yourapp.com;

    ssl_certificate /etc/ssl/certs/cert.pem;
    ssl_certificate_key /etc/ssl/private/key.pem;

    location / {
        proxy_pass http://docs-service:8000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

## Monitoring

### Prometheus Metrics

Add to `requirements.txt`:
```
prometheus-fastapi-instrumentator
```

Add to `main.py`:
```python
from prometheus_fastapi_instrumentator import Instrumentator

app = FastAPI(...)

Instrumentator().instrument(app).expose(app)
```

### Grafana Dashboard

Key metrics to monitor:
- Request rate (by endpoint)
- Response time (P50, P95, P99)
- Error rate
- CPU and memory usage
- Qdrant storage size
- Embedding generation time

### Logging

Configure structured logging:

```python
import logging
import sys
from pythonjsonlogger import jsonlogger

logHandler = logging.StreamHandler(sys.stdout)
formatter = jsonlogger.JsonFormatter()
logHandler.setFormatter(formatter)
logger = logging.getLogger()
logger.addHandler(logHandler)
logger.setLevel(logging.INFO)
```

## Backup and Recovery

### Qdrant Snapshots

```bash
# Create snapshot
curl -X POST http://localhost:6333/collections/netgpt_documents/snapshots

# List snapshots
curl http://localhost:6333/collections/netgpt_documents/snapshots

# Download snapshot
curl http://localhost:6333/collections/netgpt_documents/snapshots/{snapshot_name} \
  -o snapshot.tar

# Restore from snapshot
curl -X PUT http://localhost:6333/collections/netgpt_documents/snapshots/upload \
  -F "snapshot=@snapshot.tar"
```

### Automated Backups

```bash
#!/bin/bash
# backup-qdrant.sh

COLLECTION="netgpt_documents"
BACKUP_DIR="/backups/qdrant"
DATE=$(date +%Y%m%d_%H%M%S)

# Create snapshot
SNAPSHOT=$(curl -s -X POST http://qdrant:6333/collections/$COLLECTION/snapshots | jq -r '.result.name')

# Download snapshot
curl -s http://qdrant:6333/collections/$COLLECTION/snapshots/$SNAPSHOT \
  -o $BACKUP_DIR/$COLLECTION-$DATE.tar

# Upload to S3 (optional)
aws s3 cp $BACKUP_DIR/$COLLECTION-$DATE.tar s3://your-bucket/qdrant-backups/

# Clean old local backups (keep last 7 days)
find $BACKUP_DIR -name "*.tar" -mtime +7 -delete
```

Add to cron:
```
0 2 * * * /path/to/backup-qdrant.sh
```

## Performance Tuning

### 1. Optimize Qdrant

```yaml
# In qdrant-config.yaml
storage:
  performance:
    max_search_threads: 8  # Increase for more CPU
    max_optimization_threads: 4
  
  # Use quantization to reduce memory
  quantization:
    scalar:
      type: int8
      quantile: 0.99
```

### 2. Batch Processing

For large ingestion jobs, use async processing:

```python
from fastapi import BackgroundTasks

@app.post("/ingest/async")
async def ingest_async(doc: DocumentIn, background_tasks: BackgroundTasks):
    background_tasks.add_task(ingest_document, doc.doc_id, doc.text)
    return {"status": "queued", "doc_id": doc.doc_id}
```

### 3. Caching

Add Redis for frequent queries:

```python
import redis
import json

redis_client = redis.Redis(host='redis', port=6379, decode_responses=True)
CACHE_TTL = 3600  # 1 hour

@app.post("/query")
def query_endpoint(query: QueryIn):
    cache_key = f"query:{hash(query.query)}"
    
    # Check cache
    cached = redis_client.get(cache_key)
    if cached:
        return json.loads(cached)
    
    # Query and cache
    results = query_text(query.query)
    redis_client.setex(cache_key, CACHE_TTL, json.dumps([r.dict() for r in results]))
    return results
```

## Disaster Recovery

### High Availability Setup

```yaml
# Multi-region deployment
Region 1 (Primary):
  - Docs Service (3 replicas)
  - Qdrant (cluster: 3 nodes)
  
Region 2 (Standby):
  - Docs Service (2 replicas)
  - Qdrant (cluster: 3 nodes, async replication)

Load Balancer:
  - Primary: Region 1
  - Failover: Region 2 (if Region 1 unhealthy)
```

### Recovery Procedures

1. **Service Crash**: Kubernetes/Docker restarts automatically
2. **Data Corruption**: Restore from latest Qdrant snapshot
3. **Region Failure**: Failover to standby region
4. **Complete Data Loss**: Re-ingest from source documents (if available)

## Cost Optimization

### 1. Right-sizing

- Start small: 2 CPU, 4GB RAM per service
- Monitor utilization
- Scale up based on actual usage

### 2. Embedding Model Selection

| Model | Speed | Quality | Memory |
|-------|-------|---------|--------|
| bge-small | Fast | Good | Low |
| bge-base | Medium | Better | Medium |
| bge-large | Slow | Best | High |

For production: start with `bge-small`, upgrade if quality insufficient.

### 3. Qdrant Optimization

- Enable quantization (reduces memory by 4x)
- Use disk storage for cold data
- Implement data retention policy

## Deployment Checklist

- [ ] Build and test Docker image
- [ ] Configure environment variables
- [ ] Set up Qdrant instance
- [ ] Enable authentication
- [ ] Enable rate limiting
- [ ] Configure SSL/TLS
- [ ] Set up monitoring
- [ ] Configure logging
- [ ] Set up automated backups
- [ ] Test failover procedures
- [ ] Document runbooks
- [ ] Load test the service
- [ ] Security audit
- [ ] Cost analysis

## Troubleshooting

### High Latency

1. Check Qdrant response time
2. Monitor embedding generation time
3. Check network latency
4. Verify resource limits not hit
5. Consider caching frequently queried items

### High Memory Usage

1. Check Qdrant collection size
2. Enable quantization in Qdrant
3. Reduce `CHUNK_SIZE`
4. Use smaller embedding model
5. Implement data retention

### High Error Rate

1. Check service logs
2. Verify Qdrant connectivity
3. Check resource limits
4. Monitor disk space
5. Verify API authentication

## Support and Maintenance

### Regular Tasks

- **Daily**: Monitor metrics, check logs
- **Weekly**: Review error rates, check backup success
- **Monthly**: Review costs, optimize resources
- **Quarterly**: Security audit, dependency updates

### Upgrade Procedure

1. Test new version in staging
2. Create backup
3. Deploy to one instance
4. Monitor for issues
5. Gradual rollout to all instances
6. Rollback if issues detected

## Conclusion

This deployment guide covers production deployment of the document service with:
- High availability
- Security hardening
- Monitoring and observability
- Backup and recovery
- Performance optimization
- Cost optimization

Follow the checklist and adapt to your specific infrastructure and requirements.

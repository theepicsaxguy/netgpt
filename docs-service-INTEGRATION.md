# Integration Guide: Document Ingestion Service with NetGPT

This document explains how to integrate the document-embedding microservice with the existing NetGPT system.

## Architecture Overview

The document service is a standalone Python/FastAPI microservice that:
- Runs in its own Docker container
- Exposes REST API endpoints
- Communicates with other services via HTTP/JSON
- Uses Qdrant for vector storage and semantic search

## Integration Points

### 1. Service Communication

Other NetGPT services (e.g., .NET backend) can call the document service via HTTP:

**C# Example (using HttpClient):**

```csharp
public class DocumentServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public DocumentServiceClient(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _baseUrl = config["DocumentService:BaseUrl"] ?? "http://docs-service:8000";
    }

    public async Task<IngestResponse> IngestDocumentAsync(string docId, string text)
    {
        var request = new { doc_id = docId, text = text };
        var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/ingest", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IngestResponse>();
    }

    public async Task<List<SearchResult>> QueryDocumentsAsync(string query)
    {
        var request = new { query = query };
        var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/query", request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<SearchResult>>();
    }

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

public class IngestResponse
{
    public string Status { get; set; }
    public int ChunksIngested { get; set; }
}

public class SearchResult
{
    public string DocId { get; set; }
    public string Chunk { get; set; }
    public double Score { get; set; }
}
```

**Registration in .NET Startup/Program.cs:**

```csharp
builder.Services.AddHttpClient<DocumentServiceClient>();
builder.Services.AddSingleton<DocumentServiceClient>();
```

### 2. Docker Compose Integration

Add the document service to your existing `docker-compose.yml`:

```yaml
services:
  # ... existing services ...

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
    volumes:
      - qdrant-storage:/qdrant/storage
    restart: unless-stopped
    networks:
      - netgpt-network

volumes:
  qdrant-storage:
```

### 3. Environment Configuration

Add to your `.env` file:

```env
# Document Service Configuration
DOCUMENT_SERVICE_URL=http://docs-service:8000
QDRANT_URL=http://qdrant:6333
QDRANT_API_KEY=
EMBEDDING_MODEL=BAAI/bge-small-en-v1.5
COLLECTION_NAME=netgpt_documents
CHUNK_SIZE=512
```

### 4. Use Cases

#### Use Case 1: Ingesting Conversation History

When a conversation ends or reaches a certain length, ingest it for future reference:

```csharp
public async Task ArchiveConversationAsync(Conversation conversation)
{
    var text = string.Join("\n\n", conversation.Messages.Select(m => 
        $"{m.Role}: {m.Content}"));
    
    await _documentServiceClient.IngestDocumentAsync(
        docId: $"conversation_{conversation.Id}",
        text: text
    );
}
```

#### Use Case 2: Context Retrieval for RAG

Before generating a response, retrieve relevant context:

```csharp
public async Task<string> GetRelevantContextAsync(string userQuery)
{
    var results = await _documentServiceClient.QueryDocumentsAsync(userQuery);
    
    // Take top 3 most relevant chunks
    var context = string.Join("\n\n", 
        results.Take(3).Select(r => r.Chunk));
    
    return context;
}

public async Task<string> GenerateResponseWithContextAsync(string userQuery)
{
    var context = await GetRelevantContextAsync(userQuery);
    
    var prompt = $@"
Context from previous conversations:
{context}

User question: {userQuery}

Please provide a helpful response based on the context above.";
    
    // Send to AI agent with context
    return await _aiService.GenerateResponseAsync(prompt);
}
```

#### Use Case 3: Document Knowledge Base

Allow users to upload documents that can be queried later:

```csharp
public async Task<string> IngestUserDocumentAsync(string userId, string fileName, string content)
{
    var docId = $"user_{userId}_doc_{fileName}";
    var result = await _documentServiceClient.IngestDocumentAsync(docId, content);
    return $"Ingested {result.ChunksIngested} chunks from {fileName}";
}

public async Task<List<SearchResult>> SearchUserDocumentsAsync(string userId, string query)
{
    // Query all documents
    var allResults = await _documentServiceClient.QueryDocumentsAsync(query);
    
    // Filter to user's documents only
    return allResults
        .Where(r => r.DocId.StartsWith($"user_{userId}_"))
        .ToList();
}
```

### 5. API Gateway Integration

If using an API gateway, add routes:

```yaml
routes:
  - path: /api/documents/ingest
    method: POST
    upstream: http://docs-service:8000/ingest
    
  - path: /api/documents/query
    method: POST
    upstream: http://docs-service:8000/query
    
  - path: /api/documents/health
    method: GET
    upstream: http://docs-service:8000/health
```

### 6. Health Checks

Add to your monitoring/health check system:

```csharp
public class DocumentServiceHealthCheck : IHealthCheck
{
    private readonly DocumentServiceClient _client;

    public DocumentServiceHealthCheck(DocumentServiceClient client)
    {
        _client = client;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var isHealthy = await _client.IsHealthyAsync();
            return isHealthy 
                ? HealthCheckResult.Healthy("Document service is responsive")
                : HealthCheckResult.Unhealthy("Document service is not responding");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Document service health check failed", ex);
        }
    }
}
```

Register in Startup:

```csharp
builder.Services.AddHealthChecks()
    .AddCheck<DocumentServiceHealthCheck>("document-service");
```

## Testing Integration

### Manual Testing

```bash
# 1. Start all services
docker-compose up -d

# 2. Test document ingestion
curl -X POST http://localhost:8000/ingest \
  -H "Content-Type: application/json" \
  -d '{
    "doc_id": "test_doc_1",
    "text": "This is a test document about artificial intelligence and machine learning."
  }'

# 3. Test querying
curl -X POST http://localhost:8000/query \
  -H "Content-Type: application/json" \
  -d '{
    "query": "machine learning"
  }'

# 4. Check health
curl http://localhost:8000/health
```

### Integration Tests

```csharp
[Fact]
public async Task Should_Ingest_And_Query_Document()
{
    // Arrange
    var docId = $"test_{Guid.NewGuid()}";
    var text = "The quick brown fox jumps over the lazy dog.";
    
    // Act - Ingest
    var ingestResult = await _documentServiceClient.IngestDocumentAsync(docId, text);
    
    // Assert - Ingest
    Assert.Equal("success", ingestResult.Status);
    Assert.True(ingestResult.ChunksIngested > 0);
    
    // Act - Query
    var queryResults = await _documentServiceClient.QueryDocumentsAsync("brown fox");
    
    // Assert - Query
    Assert.NotEmpty(queryResults);
    Assert.Contains(queryResults, r => r.DocId == docId);
    Assert.All(queryResults, r => Assert.True(r.Score > 0));
}
```

## Performance Considerations

1. **Batch Processing**: For ingesting multiple documents, consider batching requests
2. **Caching**: Cache frequent queries in Redis
3. **Async Processing**: Use message queues for large document ingestion
4. **Connection Pooling**: Reuse HTTP connections with HttpClient
5. **Timeout Configuration**: Set appropriate timeouts for embedding/search operations

## Security Considerations

1. **Authentication**: Add JWT/OAuth validation if needed
2. **Rate Limiting**: Implement rate limiting to prevent abuse
3. **Input Validation**: Validate document size and content
4. **API Key**: Use `QDRANT_API_KEY` for Qdrant authentication in production
5. **Network Isolation**: Keep document service in private network, expose via gateway only

## Monitoring

Key metrics to monitor:

- Request latency (ingest, query)
- Document ingestion rate
- Query success rate
- Qdrant storage size
- Embedding generation time
- Error rates

## Troubleshooting

### Service Not Starting

```bash
# Check logs
docker logs netgpt-docs-service

# Common issues:
# - Missing dependencies: rebuild image
# - Qdrant not accessible: check network configuration
# - Port already in use: change port mapping
```

### Slow Performance

- Check embedding model size (smaller = faster)
- Verify Qdrant is running and responsive
- Monitor CPU/memory usage
- Consider reducing `CHUNK_SIZE` for faster ingestion

### Empty Query Results

- Verify documents were ingested successfully
- Check Qdrant collection exists: `http://localhost:6333/collections`
- Ensure query text is meaningful (not too short)
- Verify embedding model is the same for ingestion and querying

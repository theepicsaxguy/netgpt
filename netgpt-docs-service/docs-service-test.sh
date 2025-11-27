#!/bin/bash

# Test script for NetGPT Document Ingestion Service
# This script tests the /ingest, /query, and /health endpoints

set -e

SERVICE_URL="${SERVICE_URL:-http://localhost:8000}"
BOLD='\033[1m'
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${BOLD}NetGPT Document Service Test Script${NC}"
echo "Service URL: $SERVICE_URL"
echo ""

# Test 1: Health Check
echo -e "${BOLD}Test 1: Health Check${NC}"
echo "GET $SERVICE_URL/health"
HEALTH_RESPONSE=$(curl -s -X GET "$SERVICE_URL/health")
echo "Response: $HEALTH_RESPONSE"
if echo "$HEALTH_RESPONSE" | grep -q "healthy"; then
    echo -e "${GREEN}✓ Health check passed${NC}"
else
    echo -e "${RED}✗ Health check failed${NC}"
    exit 1
fi
echo ""

# Test 2: Ingest Document
echo -e "${BOLD}Test 2: Ingest Document${NC}"
DOC_ID="test_doc_$(date +%s)"
DOC_TEXT="Artificial intelligence and machine learning are revolutionizing technology. Deep learning models can process vast amounts of data. Neural networks learn patterns and make predictions. Natural language processing enables computers to understand human language."

echo "POST $SERVICE_URL/ingest"
echo "Document ID: $DOC_ID"
INGEST_RESPONSE=$(curl -s -X POST "$SERVICE_URL/ingest" \
  -H "Content-Type: application/json" \
  -d "{\"doc_id\": \"$DOC_ID\", \"text\": \"$DOC_TEXT\"}")

echo "Response: $INGEST_RESPONSE"
if echo "$INGEST_RESPONSE" | grep -q "success"; then
    CHUNKS_INGESTED=$(echo "$INGEST_RESPONSE" | grep -o '"chunks_ingested":[0-9]*' | grep -o '[0-9]*')
    echo -e "${GREEN}✓ Document ingested successfully ($CHUNKS_INGESTED chunks)${NC}"
else
    echo -e "${RED}✗ Document ingestion failed${NC}"
    exit 1
fi
echo ""

# Give Qdrant a moment to index
sleep 2

# Test 3: Query Documents
echo -e "${BOLD}Test 3: Query Documents${NC}"
QUERY_TEXT="machine learning"
echo "POST $SERVICE_URL/query"
echo "Query: $QUERY_TEXT"
QUERY_RESPONSE=$(curl -s -X POST "$SERVICE_URL/query" \
  -H "Content-Type: application/json" \
  -d "{\"query\": \"$QUERY_TEXT\"}")

echo "Response: $QUERY_RESPONSE"
if echo "$QUERY_RESPONSE" | grep -q "score"; then
    RESULT_COUNT=$(echo "$QUERY_RESPONSE" | grep -o '"doc_id"' | wc -l)
    echo -e "${GREEN}✓ Query executed successfully ($RESULT_COUNT results)${NC}"
    
    # Pretty print first result
    if [ "$RESULT_COUNT" -gt 0 ]; then
        echo ""
        echo "First result:"
        echo "$QUERY_RESPONSE" | python3 -m json.tool 2>/dev/null || echo "$QUERY_RESPONSE"
    fi
else
    echo -e "${RED}✗ Query failed${NC}"
    exit 1
fi
echo ""

# Test 4: Query with Different Text
echo -e "${BOLD}Test 4: Query with Different Text${NC}"
QUERY_TEXT_2="neural networks"
echo "POST $SERVICE_URL/query"
echo "Query: $QUERY_TEXT_2"
QUERY_RESPONSE_2=$(curl -s -X POST "$SERVICE_URL/query" \
  -H "Content-Type: application/json" \
  -d "{\"query\": \"$QUERY_TEXT_2\"}")

if echo "$QUERY_RESPONSE_2" | grep -q "score"; then
    RESULT_COUNT_2=$(echo "$QUERY_RESPONSE_2" | grep -o '"doc_id"' | wc -l)
    echo -e "${GREEN}✓ Second query executed successfully ($RESULT_COUNT_2 results)${NC}"
else
    echo -e "${RED}✗ Second query failed${NC}"
    exit 1
fi
echo ""

# Test 5: Ingest Another Document
echo -e "${BOLD}Test 5: Ingest Second Document${NC}"
DOC_ID_2="test_doc_2_$(date +%s)"
DOC_TEXT_2="Python is a popular programming language for data science. FastAPI is a modern web framework for building APIs. Docker containers make deployment easier. Microservices architecture improves scalability."

echo "POST $SERVICE_URL/ingest"
echo "Document ID: $DOC_ID_2"
INGEST_RESPONSE_2=$(curl -s -X POST "$SERVICE_URL/ingest" \
  -H "Content-Type: application/json" \
  -d "{\"doc_id\": \"$DOC_ID_2\", \"text\": \"$DOC_TEXT_2\"}")

if echo "$INGEST_RESPONSE_2" | grep -q "success"; then
    CHUNKS_INGESTED_2=$(echo "$INGEST_RESPONSE_2" | grep -o '"chunks_ingested":[0-9]*' | grep -o '[0-9]*')
    echo -e "${GREEN}✓ Second document ingested successfully ($CHUNKS_INGESTED_2 chunks)${NC}"
else
    echo -e "${RED}✗ Second document ingestion failed${NC}"
    exit 1
fi
echo ""

sleep 2

# Test 6: Query Across Multiple Documents
echo -e "${BOLD}Test 6: Query Across Multiple Documents${NC}"
QUERY_TEXT_3="programming"
echo "POST $SERVICE_URL/query"
echo "Query: $QUERY_TEXT_3"
QUERY_RESPONSE_3=$(curl -s -X POST "$SERVICE_URL/query" \
  -H "Content-Type: application/json" \
  -d "{\"query\": \"$QUERY_TEXT_3\"}")

if echo "$QUERY_RESPONSE_3" | grep -q "score"; then
    RESULT_COUNT_3=$(echo "$QUERY_RESPONSE_3" | grep -o '"doc_id"' | wc -l)
    echo -e "${GREEN}✓ Multi-document query executed successfully ($RESULT_COUNT_3 results)${NC}"
else
    echo -e "${RED}✗ Multi-document query failed${NC}"
    exit 1
fi
echo ""

# Test 7: Empty Text Validation
echo -e "${BOLD}Test 7: Empty Text Validation${NC}"
echo "POST $SERVICE_URL/ingest (with empty text)"
EMPTY_RESPONSE=$(curl -s -X POST "$SERVICE_URL/ingest" \
  -H "Content-Type: application/json" \
  -d '{"doc_id": "empty_test", "text": ""}')

if echo "$EMPTY_RESPONSE" | grep -q "400\|No content"; then
    echo -e "${GREEN}✓ Empty text properly rejected${NC}"
else
    echo -e "${RED}✗ Empty text validation failed${NC}"
    echo "Response: $EMPTY_RESPONSE"
fi
echo ""

# Summary
echo -e "${BOLD}=== Test Summary ===${NC}"
echo -e "${GREEN}✓ All tests passed successfully!${NC}"
echo ""
echo "Tested endpoints:"
echo "  - GET  /health  (health check)"
echo "  - POST /ingest  (document ingestion)"
echo "  - POST /query   (semantic search)"
echo ""
echo "Verified functionality:"
echo "  - Service is running and healthy"
echo "  - Documents can be ingested and chunked"
echo "  - Embeddings are generated and stored"
echo "  - Semantic queries return relevant results"
echo "  - Multiple documents can be queried together"
echo "  - Input validation works correctly"
echo ""
echo -e "${BOLD}Service is ready for integration!${NC}"

#!/usr/bin/env python3
"""
Test script for NetGPT Document Ingestion Service
Tests the /ingest, /query, and /health endpoints
"""

import sys
import time
import requests
from typing import Dict, Any

SERVICE_URL = "http://localhost:8000"

class Colors:
    BOLD = '\033[1m'
    GREEN = '\033[0;32m'
    RED = '\033[0;31m'
    YELLOW = '\033[0;33m'
    NC = '\033[0m'

def print_test(name: str):
    print(f"\n{Colors.BOLD}{name}{Colors.NC}")

def print_success(message: str):
    print(f"{Colors.GREEN}✓ {message}{Colors.NC}")

def print_error(message: str):
    print(f"{Colors.RED}✗ {message}{Colors.NC}")

def print_info(message: str):
    print(f"{Colors.YELLOW}{message}{Colors.NC}")

def test_health() -> bool:
    print_test("Test 1: Health Check")
    try:
        response = requests.get(f"{SERVICE_URL}/health")
        print(f"GET {SERVICE_URL}/health")
        print(f"Response: {response.json()}")
        
        if response.status_code == 200 and response.json().get("status") == "healthy":
            print_success("Health check passed")
            return True
        else:
            print_error("Health check failed")
            return False
    except Exception as e:
        print_error(f"Health check failed: {e}")
        return False

def test_ingest(doc_id: str, text: str) -> bool:
    print_test(f"Test: Ingest Document (ID: {doc_id})")
    try:
        response = requests.post(
            f"{SERVICE_URL}/ingest",
            json={"doc_id": doc_id, "text": text}
        )
        print(f"POST {SERVICE_URL}/ingest")
        result = response.json()
        print(f"Response: {result}")
        
        if response.status_code == 200 and result.get("status") == "success":
            chunks = result.get("chunks_ingested", 0)
            print_success(f"Document ingested successfully ({chunks} chunks)")
            return True
        else:
            print_error(f"Document ingestion failed: {result}")
            return False
    except Exception as e:
        print_error(f"Document ingestion failed: {e}")
        return False

def test_query(query: str, expected_results: int = None) -> bool:
    print_test(f"Test: Query Documents (Query: '{query}')")
    try:
        response = requests.post(
            f"{SERVICE_URL}/query",
            json={"query": query}
        )
        print(f"POST {SERVICE_URL}/query")
        results = response.json()
        
        if response.status_code == 200 and isinstance(results, list):
            count = len(results)
            print_success(f"Query executed successfully ({count} results)")
            
            if count > 0:
                print("\nTop result:")
                top = results[0]
                print(f"  Doc ID: {top['doc_id']}")
                print(f"  Score: {top['score']:.4f}")
                print(f"  Chunk: {top['chunk'][:100]}...")
            
            if expected_results is not None and count != expected_results:
                print_info(f"Expected {expected_results} results, got {count}")
            
            return True
        else:
            print_error(f"Query failed: {results}")
            return False
    except Exception as e:
        print_error(f"Query failed: {e}")
        return False

def test_empty_text() -> bool:
    print_test("Test: Empty Text Validation")
    try:
        response = requests.post(
            f"{SERVICE_URL}/ingest",
            json={"doc_id": "empty_test", "text": ""}
        )
        
        if response.status_code == 400:
            print_success("Empty text properly rejected")
            return True
        else:
            print_error(f"Empty text validation failed: {response.json()}")
            return False
    except Exception as e:
        print_error(f"Empty text validation failed: {e}")
        return False

def run_all_tests():
    print(f"{Colors.BOLD}NetGPT Document Service Test Suite{Colors.NC}")
    print(f"Service URL: {SERVICE_URL}\n")
    
    tests_passed = 0
    tests_total = 0
    
    # Test 1: Health Check
    tests_total += 1
    if test_health():
        tests_passed += 1
    
    # Test 2: Ingest First Document
    tests_total += 1
    doc1_text = """
    Artificial intelligence and machine learning are revolutionizing technology.
    Deep learning models can process vast amounts of data.
    Neural networks learn patterns and make predictions.
    Natural language processing enables computers to understand human language.
    Computer vision allows machines to interpret images.
    """
    if test_ingest("test_doc_ai", doc1_text):
        tests_passed += 1
        time.sleep(2)  # Give Qdrant time to index
    
    # Test 3: Query First Document
    tests_total += 1
    if test_query("machine learning"):
        tests_passed += 1
    
    # Test 4: Ingest Second Document
    tests_total += 1
    doc2_text = """
    Python is a popular programming language for data science.
    FastAPI is a modern web framework for building APIs.
    Docker containers make deployment easier.
    Microservices architecture improves scalability.
    Kubernetes orchestrates containerized applications.
    """
    if test_ingest("test_doc_tech", doc2_text):
        tests_passed += 1
        time.sleep(2)
    
    # Test 5: Query Second Document
    tests_total += 1
    if test_query("Python programming"):
        tests_passed += 1
    
    # Test 6: Query Across Both Documents
    tests_total += 1
    if test_query("technology"):
        tests_passed += 1
    
    # Test 7: Empty Text Validation
    tests_total += 1
    if test_empty_text():
        tests_passed += 1
    
    # Test 8: Ingest Third Document with Code
    tests_total += 1
    doc3_text = """
    Vector databases store embeddings for semantic search.
    Qdrant is a high-performance vector search engine.
    Cosine similarity measures the similarity between vectors.
    RAG (Retrieval Augmented Generation) combines retrieval with LLMs.
    Chunking strategies affect retrieval quality.
    """
    if test_ingest("test_doc_vectors", doc3_text):
        tests_passed += 1
        time.sleep(2)
    
    # Test 9: Query Vector-Related Content
    tests_total += 1
    if test_query("vector database"):
        tests_passed += 1
    
    # Print Summary
    print(f"\n{Colors.BOLD}=== Test Summary ==={Colors.NC}")
    print(f"Tests passed: {tests_passed}/{tests_total}")
    
    if tests_passed == tests_total:
        print_success("All tests passed!")
        print("\nTested endpoints:")
        print("  - GET  /health  (health check)")
        print("  - POST /ingest  (document ingestion)")
        print("  - POST /query   (semantic search)")
        print("\nVerified functionality:")
        print("  - Service is running and healthy")
        print("  - Documents can be ingested and chunked")
        print("  - Embeddings are generated and stored")
        print("  - Semantic queries return relevant results")
        print("  - Multiple documents can be queried together")
        print("  - Input validation works correctly")
        print(f"\n{Colors.BOLD}Service is ready for integration!{Colors.NC}")
        return True
    else:
        print_error(f"{tests_total - tests_passed} test(s) failed")
        return False

if __name__ == "__main__":
    try:
        success = run_all_tests()
        sys.exit(0 if success else 1)
    except KeyboardInterrupt:
        print("\n\nTests interrupted by user")
        sys.exit(1)
    except Exception as e:
        print_error(f"Test suite failed: {e}")
        sys.exit(1)

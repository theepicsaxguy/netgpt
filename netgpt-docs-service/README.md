
````markdown
# NetGPT Document Ingestion Service

A Python + FastAPI microservice for ingesting text documents, generating embeddings with FastEmbed, storing vectors in Qdrant, and answering semantic queries.

Directory structure:

```
netgpt-docs-service/
├── Dockerfile
├── requirements.txt
├── docker-compose.yml
├── .gitignore
├── organize-docs-service.sh
└── app/
    ├── __init__.py
    ├── main.py
    ├── config.py
    ├── models.py
    └── services.py
```

Quick start (Docker):

```bash
cd netgpt-docs-service
docker-compose up -d --build
# then:
curl http://localhost:8000/health
```

Qdrant configuration
--------------------

This service reads Qdrant connection settings from environment variables. You can provide them via a `.env` file in the `netgpt-docs-service/` folder (an example `.env.example` is included).

Important variables:

- `QDRANT_URL`: Qdrant HTTP endpoint (e.g. https://<cluster>.us-east.aws.cloud.qdrant.io:6333)
- `QDRANT_API_KEY`: API key for Qdrant Cloud (optional for self-hosted Qdrant)
- `COLLECTION_NAME`: default collection name used by the service
- `USE_MOCK_QDRANT`: set to `1` to use an in-process mock client for local testing

Admin endpoints
---------------

The service exposes a few admin endpoints to manage Qdrant collections:

- POST /admin/ensure-collection?name=NAME&dim=384 - Ensure the named collection exists (creates it if missing)
- GET /admin/collections - List collections visible to the client
- DELETE /admin/collections/{name} - Delete a named collection
- GET /admin/client-info - Returns minimal metadata about the internal Qdrant client (does not return API keys)

These are intended for operational convenience; secure them appropriately before exposing in production.

Ingest & verify with FastEmbed (example)
---------------------------------------

The service uses `fastembed` to produce embeddings. Below are two common ways to run and verify ingestion:

1) Docker (recommended for local end-to-end):

    - Start the service with Qdrant in docker-compose (this image includes Qdrant):

```bash
cd netgpt-docs-service
docker-compose up -d --build
```

    - Ingest a document into a collection named `my_collection`:

```bash
curl -X POST "http://localhost:8000/ingest" \
  -H "Content-Type: application/json" \
  -d '{"doc_id":"doc1","text":"This is a test document to index with embeddings.","collection":"my_collection"}'
```

    - Expected response (example):

```json
{"status":"success","chunks_ingested":3}
```

    - Query the collection:

```bash
curl -X POST "http://localhost:8000/query" \
  -H "Content-Type: application/json" \
  -d '{"query":"test document","collection":"my_collection"}'
```

    - Expected response (example):

```json
[
  {"doc_id":"doc1","chunk":"This is a test document to index with embeddings.","score":0.9}
]
```

2) Virtualenv (if you prefer not to use Docker):

```bash
python3 -m venv venv
source venv/bin/activate
pip install -r requirements.txt
# Set QDRANT_URL to your qdrant instance (e.g. http://localhost:6333)
export QDRANT_URL=http://localhost:6333
export COLLECTION_NAME=netgpt_documents  # optional
uvicorn app.main:app --reload --host 0.0.0.0 --port 8000
```

Then use the same curl commands above to ingest and query. If using Qdrant Cloud, set `QDRANT_URL` and `QDRANT_API_KEY` in `.env` or the environment.

Notes
-----
- The service will create the target collection automatically when a document is ingested with a `collection` parameter that does not exist. The collection vector size will be set based on the embedding dimensionality produced by `fastembed` for the first chunk.
- Make sure your `EMBEDDING_MODEL` is supported by `fastembed` and available in the execution environment.
- Secure admin endpoints (`/admin/*`) before exposing the service publicly.

Development (local):

```bash
python3 -m venv venv
source venv/bin/activate
pip install -r requirements.txt
uvicorn app.main:app --reload --host 0.0.0.0 --port 8000
```

Tests are available under `tests/document-service/` and a small organization script `organize-docs-service.sh` exists to move prefixed files into this layout if they are still present in the repository root.

For full documentation see `docs/document-service/` in the repository.

-- NetGPT Team

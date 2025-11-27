
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

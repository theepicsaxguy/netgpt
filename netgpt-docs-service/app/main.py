# Import pre-configuration early to set env vars before native libs are loaded.
# Use package-qualified import to work reliably when running as a package.
try:
    from app.preconfig import configure_from_env  # side-effect: sets env vars
except Exception:
    # Fallback to local import when running as a script from inside the app folder
    from preconfig import configure_from_env  # side-effect: sets env vars
from fastapi import FastAPI, HTTPException
import sys
from pathlib import Path

# Ensure the project root (parent of this `app` folder) is on sys.path so the
# package imports like `app.models` work when running `python main.py` from
# inside the `app/` directory.
project_root = Path(__file__).resolve().parent.parent
if str(project_root) not in sys.path:
    sys.path.insert(0, str(project_root))

# Support running this file both as a package (python -m app.main or
# uvicorn app.main:app) and as a script (python main.py). When run as a
# script, the package name `app` may not be importable without adjusting
# sys.path as above. Try the package imports first, then fall back to
# local imports if necessary.
try:
    from app.models import DocumentIn, QueryIn, SearchResult
    from app.services import ingest_document, query_text
except ImportError:
    # Fallback for script execution where the current directory is the package folder
    from models import DocumentIn, QueryIn, SearchResult
    from services import ingest_document, query_text

app = FastAPI(
    title="NetGPT Document Ingestion Service",
    description="Ingest text docs, chunk and embed with FastEmbed, store in Qdrant, and query by semantic similarity."
)

@app.post("/ingest")
def ingest_endpoint(doc: DocumentIn):
    count = ingest_document(doc.doc_id, doc.text)
    if count == 0:
        raise HTTPException(status_code=400, detail="No content to ingest.")
    return {"status": "success", "chunks_ingested": count}

@app.post("/query", response_model=list[SearchResult])
def query_endpoint(query: QueryIn):
    results = query_text(query.query, top_k=5)
    return results

@app.get("/health")
def health():
    return {"status": "healthy"}


if __name__ == "__main__":
    # Allow quick local testing with: python main.py
    try:
        import uvicorn
    except Exception:
        print("uvicorn is not installed. To run the server use: uvicorn app.main:app --reload\nOr install uvicorn: pip install uvicorn")
    else:
        # Run the ASGI app directly. Pass an import string so --reload works.
        uvicorn.run("app.main:app", host="0.0.0.0", port=8000, reload=True)

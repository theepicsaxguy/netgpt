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
    # Admin helpers
    from app.services import ensure_collection, list_collections, delete_collection, get_raw_client
except ImportError:
    # Fallback for script execution where the current directory is the package folder
    from models import DocumentIn, QueryIn, SearchResult
    from services import ingest_document, query_text
    from services import ensure_collection, list_collections, delete_collection, get_raw_client

app = FastAPI(
    title="NetGPT Document Ingestion Service",
    description="Ingest text docs, chunk and embed with FastEmbed, store in Qdrant, and query by semantic similarity."
)

@app.post("/ingest")
def ingest_endpoint(doc: DocumentIn):
    try:
        count = ingest_document(doc.doc_id, doc.text, collection=doc.collection)
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))
    if count == 0:
        raise HTTPException(status_code=400, detail="No content to ingest.")
    return {"status": "success", "chunks_ingested": count}

@app.post("/query", response_model=list[SearchResult])
def query_endpoint(query: QueryIn):
    try:
        results = query_text(query.query, top_k=5, collection=query.collection)
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))
    return results

@app.get("/health")
def health():
    return {"status": "healthy"}


@app.post("/admin/ensure-collection")
def admin_ensure_collection(name: str, dim: int = 384):
    ok = ensure_collection(name, dim)
    if not ok:
        raise HTTPException(status_code=500, detail="Failed to ensure collection")
    return {"status": "ok", "collection": name}


@app.get("/admin/collections")
def admin_list_collections():
    cols = list_collections()
    return {"collections": cols}


@app.delete("/admin/collections/{name}")
def admin_delete_collection(name: str):
    ok = delete_collection(name)
    if not ok:
        raise HTTPException(status_code=500, detail="Failed to delete collection")
    return {"status": "deleted", "collection": name}


@app.get("/admin/client-info")
def admin_client_info():
    client = get_raw_client()
    info = {"type": client.__class__.__name__}
    # Avoid leaking credentials; include only safe metadata
    try:
        if hasattr(client, "url"):
            info["url"] = getattr(client, "url")
    except Exception:
        pass
    return info


if __name__ == "__main__":
    # Allow quick local testing with: python main.py
    try:
        import uvicorn
    except Exception:
        print("uvicorn is not installed. To run the server use: uvicorn app.main:app --reload\nOr install uvicorn: pip install uvicorn")
    else:
        # Run the ASGI app directly. Pass an import string so --reload works.
        uvicorn.run("app.main:app", host="0.0.0.0", port=8000, reload=True)

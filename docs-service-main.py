from fastapi import FastAPI, HTTPException
from .models import DocumentIn, QueryIn, SearchResult
from .services import ingest_document, query_text

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

from pydantic import BaseModel

class DocumentIn(BaseModel):
    doc_id: str
    text: str
    # Optional target collection for the document. If not provided, the
    # service will use the COLLECTION_NAME from config if set; otherwise the
    # caller must provide a collection parameter to endpoints.
    collection: str | None = None

class QueryIn(BaseModel):
    query: str
    # Optional collection to query against. If not provided, default COLLECTION_NAME
    # is used if present.
    collection: str | None = None

class SearchResult(BaseModel):
    doc_id: str
    chunk: str
    score: float

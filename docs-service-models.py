from pydantic import BaseModel

class DocumentIn(BaseModel):
    doc_id: str
    text: str

class QueryIn(BaseModel):
    query: str

class SearchResult(BaseModel):
    doc_id: str
    chunk: str
    score: float

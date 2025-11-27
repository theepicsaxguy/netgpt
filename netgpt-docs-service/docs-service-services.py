import logging
from chonkie import RecursiveChunker
from fastembed import TextEmbedding
from qdrant_client import QdrantClient
from qdrant_client.models import VectorParams, Distance, PointStruct
from .config import QDRANT_URL, QDRANT_API_KEY, COLLECTION_NAME, CHUNK_SIZE, EMBEDDING_MODEL
from .models import SearchResult

logger = logging.getLogger("docservice")
logging.basicConfig(level=logging.INFO)

client = QdrantClient(url=QDRANT_URL, api_key=QDRANT_API_KEY)

embedder = TextEmbedding(model=EMBEDDING_MODEL)

def setup_collection(dim: int):
    if not client.collection_exists(COLLECTION_NAME):
        logger.info(f"Creating Qdrant collection '{COLLECTION_NAME}' (dim={dim})")
        client.create_collection(
            collection_name=COLLECTION_NAME,
            vectors_config=VectorParams(size=dim, distance=Distance.COSINE)
        )

def ingest_document(doc_id: str, text: str) -> int:
    chunker = RecursiveChunker(chunk_size=CHUNK_SIZE, tokenizer="gpt2")
    chunks = chunker(text)
    texts = [chunk.text for chunk in chunks]
    if not texts:
        return 0

    embeddings = list(embedder.embed(texts))
    dim = embeddings[0].shape[0]
    setup_collection(dim)

    payloads = []
    vectors = []
    for chunk, vector in zip(chunks, embeddings):
        payloads.append({"doc_id": doc_id, "chunk": chunk.text})
        vectors.append(vector.tolist())

    client.upload_collection(
        collection_name=COLLECTION_NAME,
        vectors=vectors,
        payload=payloads
    )

    logger.info(f"Ingested document '{doc_id}' with {len(chunks)} chunks.")
    return len(chunks)

def query_text(query: str, top_k: int = 5):
    q_vec = next(embedder.embed([query]))
    hits = client.search(
        collection_name=COLLECTION_NAME,
        query_vector=q_vec.tolist(),
        limit=top_k
    )
    results = []
    for hit in hits:
        payload = hit.payload or {}
        results.append(SearchResult(
            doc_id=str(payload.get("doc_id", "")),
            chunk=str(payload.get("chunk", "")),
            score=hit.score or 0.0
        ))
    logger.info(f"Query '{query}' returned {len(results)} hits.")
    return results

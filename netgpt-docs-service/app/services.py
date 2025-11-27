import logging
from chonkie import RecursiveChunker
# Heavy native libs are imported lazily inside initializer functions below
VectorParams = None
Distance = None
PointStruct = None
# Resilient imports so the package can be executed as a module or as a script
try:
    from app.config import QDRANT_URL, QDRANT_API_KEY, COLLECTION_NAME, CHUNK_SIZE, EMBEDDING_MODEL
except ImportError:
    from config import QDRANT_URL, QDRANT_API_KEY, COLLECTION_NAME, CHUNK_SIZE, EMBEDDING_MODEL

try:
    from app.models import SearchResult
except ImportError:
    from models import SearchResult

logger = logging.getLogger("docservice")
logging.basicConfig(level=logging.INFO)

# Lazy-initialized clients to avoid importing heavy native libs at module import
_client = None
_embedder = None

def get_client():
    global _client
    if _client is None:
        # import qdrant_client when first needed
        from qdrant_client import QdrantClient
        from qdrant_client.models import VectorParams as _VectorParams, Distance as _Distance, PointStruct as _PointStruct
        # expose model classes locally for use in setup_collection
        globals()["VectorParams"] = _VectorParams
        globals()["Distance"] = _Distance
        globals()["PointStruct"] = _PointStruct
        _client = QdrantClient(url=QDRANT_URL, api_key=QDRANT_API_KEY)
    return _client

def get_embedder():
    global _embedder
    if _embedder is None:
        # import fastembed (and underlying native libs) when first needed
        from fastembed import TextEmbedding
        _embedder = TextEmbedding(model=EMBEDDING_MODEL)
    return _embedder

def setup_collection(dim: int):
    client = get_client()
    # VectorParams and Distance are injected into globals when the client is
    # first created by get_client(). Ensure they exist now.
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

    embedder = get_embedder()
    embeddings = list(embedder.embed(texts))
    dim = embeddings[0].shape[0]
    setup_collection(dim)

    payloads = []
    vectors = []
    for chunk, vector in zip(chunks, embeddings):
        payloads.append({"doc_id": doc_id, "chunk": chunk.text})
        vectors.append(vector.tolist())

    client = get_client()
    client.upload_collection(
        collection_name=COLLECTION_NAME,
        vectors=vectors,
        payload=payloads
    )

    logger.info(f"Ingested document '{doc_id}' with {len(chunks)} chunks.")
    return len(chunks)

def query_text(query: str, top_k: int = 5):
    embedder = get_embedder()
    q_vec = next(embedder.embed([query]))
    client = get_client()
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

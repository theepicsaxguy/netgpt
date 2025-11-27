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
import os
USE_MOCK_QDRANT = os.getenv("USE_MOCK_QDRANT", "0") == "1"

class _MockQdrantClient:
    def __init__(self):
        self._store = []

    def collection_exists(self, collection_name: str) -> bool:
        # For demo purposes, always assume collection exists
        return True

    def create_collection(self, collection_name: str, vectors_config=None):
        # no-op for mock
        return None

    def upload_collection(self, collection_name: str, vectors, payload):
        for v, p in zip(vectors, payload):
            self._store.append({"vector": v, "payload": p})

    def search(self, collection_name: str, query_vector, limit=5):
        # Return simple mock hits with decreasing score
        hits = []
        for i, item in enumerate(self._store[:limit]):
            class _Hit:
                def __init__(self, payload, score):
                    self.payload = payload
                    self.score = score
            hits.append(_Hit(item["payload"], 1.0 - i * 0.1))
        return hits

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

        # QdrantClient supports api_key parameter for cloud use. If provided,
        # pass it through. Also allow using local in-memory or http endpoints.
        if USE_MOCK_QDRANT:
            _client = _MockQdrantClient()
        else:
            # QdrantClient will accept None for api_key; in cloud scenarios set it.
            # If a full URL is provided with https, QdrantClient will use REST transport.
            kwargs = {}
            if QDRANT_API_KEY:
                kwargs["api_key"] = QDRANT_API_KEY
            # Create the client with provided URL and optional api_key
            try:
                _client = QdrantClient(url=QDRANT_URL, **kwargs)
            except TypeError:
                # Older versions may expect 'host'/'port' args; fall back to url-only
                _client = QdrantClient(url=QDRANT_URL)
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
    # If the client is a mock, let it handle itself
    try:
        exists = client.collection_exists(COLLECTION_NAME)
    except Exception:
        # Some client versions expose get_collections; try defensive checks
        try:
            exists = COLLECTION_NAME in [c.name for c in client.get_collections().collections]
        except Exception:
            # If we cannot determine, assume false so creation will be attempted
            exists = False

    if not exists:
        logger.info(f"Creating Qdrant collection '{COLLECTION_NAME}' (dim={dim})")
        client.create_collection(
            collection_name=COLLECTION_NAME,
            vectors_config=VectorParams(size=dim, distance=Distance.COSINE)
        )


def ensure_collection(collection_name: str, dim: int, distance=None):
    """Ensure a collection with given name exists; create if missing.

    Returns True if created or already exists, False on failure.
    """
    client = get_client()
    try:
        if getattr(client, "collection_exists", None):
            if client.collection_exists(collection_name):
                return True
        # Fallback using get_collections
        if getattr(client, "get_collections", None):
            cols = client.get_collections().collections
            if any(c.name == collection_name for c in cols):
                return True
    except Exception:
        # continue to try creating
        pass

    try:
        # Accept either a Distance enum or a string like "COSINE"/"EUCLID"
        if distance is None:
            vec_distance = Distance.COSINE
        else:
            if isinstance(distance, str):
                try:
                    vec_distance = getattr(Distance, distance)
                except Exception:
                    vec_distance = Distance.COSINE
            else:
                vec_distance = distance
        client.create_collection(
            collection_name=collection_name,
            vectors_config=VectorParams(size=dim, distance=vec_distance)
        )
        return True
    except Exception as e:
        logger.exception("Failed to create collection: %s", e)
        return False


def list_collections():
    client = get_client()
    if getattr(client, "get_collections", None):
        try:
            return [c.name for c in client.get_collections().collections]
        except Exception:
            pass
    # If mock or fallback
    try:
        # Some clients may have collections property
        return [c.name for c in client.get_collections().collections]
    except Exception:
        return []


def delete_collection(collection_name: str):
    client = get_client()
    try:
        return client.delete_collection(collection_name=collection_name)
    except Exception:
        logger.exception("Failed to delete collection %s", collection_name)
        return False


def get_raw_client():
    """Return the underlying Qdrant client instance so callers can use the full
    qdrant-client API (admin, snapshots, cluster methods, etc.)."""
    return get_client()

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

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

        # QdrantClient will accept api_key for cloud; pass it if present.
        kwargs = {}
        if QDRANT_API_KEY:
            kwargs["api_key"] = QDRANT_API_KEY
        try:
            _client = QdrantClient(url=QDRANT_URL, **kwargs)
        except TypeError:
            # Older client versions may not accept api_key or url keyword; try url-only
            _client = QdrantClient(QDRANT_URL)
    return _client

def get_embedder():
    global _embedder
    if _embedder is None:
        # import fastembed (and underlying native libs) when first needed
        from fastembed import TextEmbedding
        _embedder = TextEmbedding(model=EMBEDDING_MODEL)
    return _embedder

def setup_collection(collection_name: str, dim: int):
    client = get_client()
    # Defensive check for existence of collection
    try:
        if getattr(client, "collection_exists", None):
            exists = client.collection_exists(collection_name)
        else:
            exists = collection_name in [c.name for c in client.get_collections().collections]
    except Exception:
        exists = False

    if not exists:
        logger.info(f"Creating Qdrant collection '{collection_name}' (dim={dim})")
        client.create_collection(
            collection_name=collection_name,
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

def ingest_document(doc_id: str, text: str, collection: str | None = None) -> int:
    chunker = RecursiveChunker(chunk_size=CHUNK_SIZE, tokenizer="gpt2")
    chunks = chunker(text)
    texts = [chunk.text for chunk in chunks]
    if not texts:
        return 0

    embedder = get_embedder()
    embeddings = list(embedder.embed(texts))
    dim = embeddings[0].shape[0]
    # Determine target collection: request-level, then configured default, else raise
    target = collection or COLLECTION_NAME
    if not target:
        raise ValueError("No target collection provided; set COLLECTION_NAME or pass collection parameter.")

    # Ensure the collection exists before uploading
    setup_collection(target, dim)

    payloads = []
    vectors = []
    for chunk, vector in zip(chunks, embeddings):
        payloads.append({"doc_id": doc_id, "chunk": chunk.text})
        vectors.append(vector.tolist())

    client = get_client()
    client.upload_collection(
        collection_name=target,
        vectors=vectors,
        payload=payloads
    )

    logger.info(f"Ingested document '{doc_id}' with {len(chunks)} chunks.")
    return len(chunks)

def query_text(query: str, top_k: int = 5, collection: str | None = None):
    embedder = get_embedder()
    q_vec = next(embedder.embed([query]))
    client = get_client()
    target = collection or COLLECTION_NAME
    if not target:
        raise ValueError("No target collection provided; set COLLECTION_NAME or pass collection parameter.")

    hits = client.search(
        collection_name=target,
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

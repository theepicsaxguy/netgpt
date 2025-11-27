import os

QDRANT_URL = os.getenv("QDRANT_URL", "https://qdrant.pc-tips.se")
QDRANT_API_KEY = os.getenv("QDRANT_API_KEY")

EMBEDDING_MODEL = os.getenv("EMBEDDING_MODEL", "BAAI/bge-m3")

_col = os.getenv("COLLECTION_NAME", "")
COLLECTION_NAME = _col if _col != "" else None

CHUNK_SIZE = int(os.getenv("CHUNK_SIZE", 512))

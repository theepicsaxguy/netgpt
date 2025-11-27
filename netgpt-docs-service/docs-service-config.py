import os

QDRANT_URL = os.getenv("QDRANT_URL", "http://qdrant.pc-tips.se:6333")
QDRANT_API_KEY = os.getenv("QDRANT_API_KEY")

EMBEDDING_MODEL = os.getenv("EMBEDDING_MODEL", "BAAI/bge-small-en-v1.5")

COLLECTION_NAME = os.getenv("COLLECTION_NAME", "netgpt_documents")

CHUNK_SIZE = int(os.getenv("CHUNK_SIZE", 512))

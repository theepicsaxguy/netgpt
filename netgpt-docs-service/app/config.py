import os

QDRANT_URL = os.getenv("QDRANT_URL", "https://qdrant.pc-tips.se")
QDRANT_API_KEY = os.getenv("QDRANT_API_KEY")

EMBEDDING_MODEL = os.getenv("EMBEDDING_MODEL", "BAAI/bge-m3")

COLLECTION_NAME = os.getenv("COLLECTION_NAME", "netgpt_documents")

CHUNK_SIZE = int(os.getenv("CHUNK_SIZE", 512))

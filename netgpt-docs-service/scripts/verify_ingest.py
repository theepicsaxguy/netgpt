"""Simple verification script to POST an ingest and a query to the docs service.
Run this after the service is running (docker-compose up or uvicorn).
This script uses only the Python stdlib so you can run it inside the container or
in a minimal venv without extra deps.

Usage:
    python3 scripts/verify_ingest.py --host http://localhost:8000 --collection my_collection

The script will POST a sample document to /ingest and then run a query.
"""
import argparse
import json
import sys
from urllib.request import Request, urlopen
from urllib.error import HTTPError, URLError

SAMPLE_TEXT = "This is a small test document for NetGPT docs-service ingestion."


def post_json(url: str, payload: dict) -> dict:
    data = json.dumps(payload).encode("utf-8")
    req = Request(url, data=data, headers={"Content-Type": "application/json"}, method="POST")
    with urlopen(req, timeout=30) as resp:
        return json.load(resp)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--host", default="http://localhost:8000", help="base URL of the docs service")
    parser.add_argument("--collection", default="test_collection", help="collection name to use for ingest/query")
    parser.add_argument("--doc-id", default="verify-doc-1", help="doc id to ingest")
    args = parser.parse_args()

    ingest_url = args.host.rstrip("/") + "/ingest"
    query_url = args.host.rstrip("/") + "/query"

    ingest_payload = {"doc_id": args.doc_id, "text": SAMPLE_TEXT, "collection": args.collection}
    print("Ingesting to:", ingest_url)
    try:
        res = post_json(ingest_url, ingest_payload)
        print("Ingest response:", json.dumps(res, indent=2))
    except HTTPError as e:
        print("Ingest HTTP error:", e.code, e.read().decode(errors="ignore"))
        sys.exit(1)
    except URLError as e:
        print("Ingest failed (connection):", e)
        sys.exit(1)

    query_payload = {"query": "test document", "collection": args.collection}
    print("Querying:", query_url)
    try:
        res = post_json(query_url, query_payload)
        print("Query response:", json.dumps(res, indent=2))
    except HTTPError as e:
        print("Query HTTP error:", e.code, e.read().decode(errors="ignore"))
        sys.exit(1)
    except URLError as e:
        print("Query failed (connection):", e)
        sys.exit(1)


if __name__ == "__main__":
    main()

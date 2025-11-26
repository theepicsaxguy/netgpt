#!/bin/bash

# Script to organize document service files into proper directory structure
# Run this from the repository root

set -e

BOLD='\033[1m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
NC='\033[0m' # No Color

echo -e "${BOLD}NetGPT Document Service File Organization Script${NC}\n"

# Check if we're in the right directory
if [ ! -f "docs-service-main.py" ]; then
    echo -e "${YELLOW}Warning: docs-service-main.py not found in current directory${NC}"
    echo "Please run this script from the repository root where the docs-service-* files are located"
    exit 1
fi

# Create directory structure
echo -e "${BOLD}Creating directory structure...${NC}"
mkdir -p netgpt-docs-service/app
mkdir -p docs/document-service
mkdir -p tests/document-service

echo -e "${GREEN}✓ Directories created${NC}\n"

# Move service files
echo -e "${BOLD}Moving service files...${NC}"
mv docs-service-dockerfile netgpt-docs-service/Dockerfile
mv docs-service-requirements.txt netgpt-docs-service/requirements.txt
mv docs-service-gitignore netgpt-docs-service/.gitignore
mv docs-service-README.md netgpt-docs-service/README.md

echo -e "${GREEN}✓ Root service files moved${NC}"

# Move app files
mv docs-service-main.py netgpt-docs-service/app/main.py
mv docs-service-config.py netgpt-docs-service/app/config.py
mv docs-service-models.py netgpt-docs-service/app/models.py
mv docs-service-services.py netgpt-docs-service/app/services.py
mv docs-service-app-init.py netgpt-docs-service/app/__init__.py

echo -e "${GREEN}✓ App files moved${NC}"

# Move documentation
mv docs-service-SETUP.md docs/document-service/SETUP.md
mv docs-service-INTEGRATION.md docs/document-service/INTEGRATION.md
mv docs-service-ARCHITECTURE.md docs/document-service/ARCHITECTURE.md
mv docs-service-DEPLOYMENT.md docs/document-service/DEPLOYMENT.md
mv docs-service-SUMMARY.md docs/document-service/SUMMARY.md

echo -e "${GREEN}✓ Documentation moved${NC}"

# Move test files
mv docs-service-test.sh tests/document-service/test.sh
mv docs-service-test.py tests/document-service/test.py
chmod +x tests/document-service/test.sh

echo -e "${GREEN}✓ Test files moved${NC}"

# Move docker-compose snippet
mv docs-service-docker-compose-snippet.yml netgpt-docs-service/docker-compose.yml

echo -e "${GREEN}✓ Docker Compose file moved${NC}\n"

# Create a simple docker-compose.yml if needed
echo -e "${BOLD}Creating standalone docker-compose.yml...${NC}"
cat > netgpt-docs-service/docker-compose.yml << 'EOF'
version: '3.8'

services:
  docs-service:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: netgpt-docs-service
    ports:
      - "8000:8000"
    environment:
      - QDRANT_URL=http://qdrant:6333
      - QDRANT_API_KEY=${QDRANT_API_KEY:-}
      - EMBEDDING_MODEL=BAAI/bge-small-en-v1.5
      - COLLECTION_NAME=netgpt_documents
      - CHUNK_SIZE=512
    depends_on:
      - qdrant
    restart: unless-stopped

  qdrant:
    image: qdrant/qdrant:latest
    container_name: netgpt-qdrant
    ports:
      - "6333:6333"
      - "6334:6334"
    volumes:
      - qdrant-storage:/qdrant/storage
    restart: unless-stopped

volumes:
  qdrant-storage:
EOF

echo -e "${GREEN}✓ Docker Compose file created${NC}\n"

# Summary
echo -e "${BOLD}=== Organization Complete ===${NC}\n"

echo "Directory structure:"
echo "netgpt-docs-service/"
echo "├── Dockerfile"
echo "├── requirements.txt"
echo "├── README.md"
echo "├── .gitignore"
echo "├── docker-compose.yml"
echo "└── app/"
echo "    ├── __init__.py"
echo "    ├── main.py"
echo "    ├── config.py"
echo "    ├── models.py"
echo "    └── services.py"
echo ""
echo "docs/document-service/"
echo "├── SETUP.md"
echo "├── INTEGRATION.md"
echo "├── ARCHITECTURE.md"
echo "├── DEPLOYMENT.md"
echo "└── SUMMARY.md"
echo ""
echo "tests/document-service/"
echo "├── test.sh"
echo "└── test.py"
echo ""

echo -e "${BOLD}Next Steps:${NC}"
echo ""
echo "1. Review the service files:"
echo "   cd netgpt-docs-service"
echo ""
echo "2. Start the service with Docker Compose:"
echo "   docker-compose up -d"
echo ""
echo "3. Test the service:"
echo "   ../tests/document-service/test.sh"
echo "   # or"
echo "   python3 ../tests/document-service/test.py"
echo ""
echo "4. Check service health:"
echo "   curl http://localhost:8000/health"
echo ""
echo "5. View API documentation:"
echo "   Open http://localhost:8000/docs in your browser"
echo ""
echo "6. Read the documentation:"
echo "   cat ../docs/document-service/SUMMARY.md"
echo ""
echo -e "${GREEN}All files organized successfully!${NC}"

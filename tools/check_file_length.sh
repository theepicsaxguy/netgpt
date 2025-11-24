#!/usr/bin/env bash
# Check that C# files are <= 200 lines and report failures
set -euo pipefail

ROOT_DIR=$( dirname "$0" )/..
MAX_LINES=200
echo "Checking C# file lengths (max $MAX_LINES lines) under $ROOT_DIR..."
EXIT_CODE=0

# Exclude typical generated and build folders
while IFS= read -r -d '' file; do
  # skip designer or generated files
  case "$file" in
    *".g.cs"|*".designer.cs"|*"/obj/"*|*"/bin/"*|*"/Migrations/"*)
      continue
      ;;
  esac

  lines=$(wc -l < "$file" | tr -d ' ')
  if [ "$lines" -gt "$MAX_LINES" ]; then
    echo "ERROR: $file has $lines lines (max $MAX_LINES)"
    EXIT_CODE=2
  fi
done < <(find "$ROOT_DIR" -type f -name "*.cs" -print0)

if [ "$EXIT_CODE" -ne 0 ]; then
  echo "File length check failed. Split large files to follow 200-line rule."
  exit $EXIT_CODE
fi

echo "All files conform to 200-line limit."
exit 0

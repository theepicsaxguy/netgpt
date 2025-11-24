**Linting & Architecture Rules**

This repository enforces rules to help follow SRP, SOC, DDD and file-size limits.

- **C# (backend)**:
  - Analyzers enabled via `StyleCop.Analyzers` and `Microsoft.CodeAnalysis.NetAnalyzers`.
  - `TreatWarningsAsErrors` is enabled; fix analyzer warnings.
  - `.editorconfig` at repository root configures analysis severities.
  - File length limit: 200 lines (checked by `./tools/check_file_length.sh`).
    - File length limit: 200 lines (checked by `./tools/check_file_length.sh` and enforced at build via `Directory.Build.targets`).
  - DTOs should contain a single type per file (StyleCop rule SA1402 enforced).

- **Frontend (TypeScript/JS)**:
  - ESLint configured in `frontend/.eslintrc.js` with rules to prefer one type per DTO file and `max-lines` of 200.
  - Run `npm run lint` in the `frontend` folder.

How to run checks locally:

```bash
# Backend file length
chmod +x ./tools/check_file_length.sh
./tools/check_file_length.sh

# Build & run analyzers
dotnet build backend/NetGPT.sln

# Frontend lint
cd frontend
npm ci
npm run lint
```

If a file exceeds 200 lines, split it into smaller classes/services or extract helper modules.
import { defineConfig } from 'orval';
import fs from 'fs';
import path from 'path';

// Read runtime.json from the public folder synchronously so orval can use it
const runtimePath = path.resolve(process.cwd(), 'public', 'runtime.json');
let appApiUrl = 'http://localhost:5000';
try {
  const raw = fs.readFileSync(runtimePath, { encoding: 'utf8' });
  const parsed = JSON.parse(raw);
  // If runtime.json contains an empty string for api.app we should fallback to the default.
  const runtimeUrl = parsed?.api?.app?.toString().trim();
  if (runtimeUrl) {
    appApiUrl = runtimeUrl;
  }
} catch (e) {
  // fallback to default
}

export default defineConfig({
  netgpt: {
    input: `${appApiUrl}/swagger/v1/swagger.json`,
    output: {
      mode: 'single',
      target: './src/openapi/generated/api.ts',
      client: 'react-query',
      mock: false,
      prettier: true,
      override: {
        useDates: true,
        mutator: {
          path: './src/openapi/client.ts',
          name: 'customInstance',
        },
        operations: {
          Conversations: {
            query: {
              useQuery: true,
            },
          },
          Messages: {
            query: {
              useQuery: true,
            },
          },
          Health: {
            query: {
              useQuery: true,
            },
          },
        },
      },
    },
  },
});

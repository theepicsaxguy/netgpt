import { defineConfig } from 'orval';
import fs from 'fs';
import path from 'path';

// Read runtime.json from the public folder synchronously so orval can use it
const runtimePath = path.resolve(process.cwd(), 'public', 'runtime.json');
let appApiUrl = 'http://localhost:5000';
try {
  const raw = fs.readFileSync(runtimePath, { encoding: 'utf8' });
  const parsed = JSON.parse(raw);
  appApiUrl = parsed?.api?.app ?? appApiUrl;
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

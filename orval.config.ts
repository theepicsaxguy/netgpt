import { defineConfig } from 'orval';

export default defineConfig({
  netgpt: {
    input: 'http://localhost:5000/swagger/v1/swagger.json',
    output: {
      mode: 'tags-split',
      target: './frontend/src/api/generated',
      schemas: './frontend/src/api/models',
      client: 'react-query',
      mock: false,
      prettier: true,
      override: {
        mutator: {
          path: './frontend/src/api/client.ts',
          name: 'customInstance',
        },
      },
    },
  },
});

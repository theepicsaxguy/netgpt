import { defineConfig } from 'orval';

export default defineConfig({
  netgpt: {
    input: 'http://localhost:8080/swagger/v1/swagger.json',
    output: {
      mode: 'tags-split',
      target: '../netgpt-client/src/api/generated',
      schemas: '../netgpt-client/src/api/models',
      client: 'react-query',
      mock: false,
      prettier: true,
      override: {
        mutator: {
          path: '../netgpt-client/src/api/client.ts',
          name: 'customInstance',
        },
      },
    },
  },
});

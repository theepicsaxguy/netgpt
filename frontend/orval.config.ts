import { defineConfig } from 'orval';

const appApiUrl = require('./public/runtime.json').api.app;

export default defineConfig({
  netgpt: {
    input: `${appApiUrl}/swagger/v1/swagger.json`,
    output: {
      mode: 'single',
      target: './src/api/generated/api.ts',
      client: 'react-query',
      mock: false,
      prettier: true,
      override: {
        useDates: true,
        mutator: {
          path: './src/api/client.ts',
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

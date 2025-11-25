const { defineConfig } = require('orval');

const appApiUrl = require('./public/runtime.json').api.app;

module.exports = defineConfig({
  netgpt: {
    input: `${appApiUrl}/swagger/v1/swagger.json`,
    output: {
      mode: 'single',
      target: './src/api/v1/generated/api.ts',
      client: 'react-query',
      mock: false,
      prettier: true,
      override: {
        useDates: true,
        mutator: {
          path: './src/api/v1/client.ts',
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

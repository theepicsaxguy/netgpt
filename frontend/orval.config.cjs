const { defineConfig } = require('orval');

const appApiUrl = require('./public/runtime.json').api.app;

module.exports = defineConfig({
  netgpt: {
    input: `${appApiUrl}/swagger/v1/swagger.json`,
    output: {
      mode: 'single',
      target: 'src/openapi/generated.ts',
      client: 'react-query',
      mock: false,
      prettier: true,
      override: {
        useDates: true,
        mutator: {
          path: './srcclient.ts',
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

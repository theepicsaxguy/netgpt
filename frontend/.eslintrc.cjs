// .eslintrc.cjs
const localPlugin = require('./eslint-rules/index.cjs');

module.exports = {
  root: true,
  parser: '@typescript-eslint/parser',
  parserOptions: {
    ecmaVersion: 2024,
    sourceType: 'module',
    project: './tsconfig.json',
  },
  plugins: {
    '@typescript-eslint': require('@typescript-eslint/eslint-plugin'),
    'local': localPlugin
  }, // Load local plugin
  env: {
    browser: true,
    node: true,
    es6: true,
  },
  extends: [
    'eslint:recommended',
    'plugin:@typescript-eslint/recommended'
  ],
  rules: {
    // Enforce single exported type/interface/class per file
    'local/single-type-per-file': 'error', // Use rule from local plugin

    // Encourage SRP / small files: max 200 lines
    'max-lines': ['error', { max: 200, skipBlankLines: true, skipComments: true }],

    // DDD and SRP related suggestions
    'no-restricted-syntax': ['error',
      {
        selector: "ExportDefaultDeclaration",
        message: 'Avoid default exports — prefer named exports for clarity in DDD and SRP.'
      }
    ],

    // TypeScript specific best practices
    '@typescript-eslint/explicit-module-boundary-types': 'error',
    '@typescript-eslint/no-explicit-any': 'error',
    '@typescript-eslint/consistent-type-definitions': ['error', 'interface']
  },
  overrides: [
    {
      files: ['src/api/v1/generated/**'],
      rules: {
        'max-lines': 'off',
        'local/single-type-per-file': 'off',
        '@typescript-eslint/consistent-type-definitions': 'off',
        '@typescript-eslint/explicit-module-boundary-types': 'off'
      }
    },
    {
      files: ['*.tsx'],
      rules: {
        'no-restricted-syntax': 'off' // Allow default exports in React components
      }
    }
  ]
};

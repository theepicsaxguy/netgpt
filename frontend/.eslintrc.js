module.exports = {
  root: true,
  parser: '@typescript-eslint/parser',
  parserOptions: {
    ecmaVersion: 2024,
    sourceType: 'module',
    project: './tsconfig.json',
  },
  plugins: ['@typescript-eslint'],
  // Expose local rules under "local-rules" namespace
  settings: { 'local-rules': require('./eslint-rules') },
  env: {
    browser: true,
    node: true,
    es6: true,
  },
  extends: [
    'eslint:recommended',
    'plugin:@typescript-eslint/recommended'
  ],
  plugins: ['max-classes-per-file'],
  rules: {
    // Enforce single exported type/interface/class per DTO file
    'local-rules/single-type-per-file': ['error', { rule: settings['local-rules'] }],

    // Encourage SRP / small files: max 200 lines
    'max-lines': ['error', { max: 200, skipBlankLines: true, skipComments: true }],

    // DDD and SRP related suggestions
    'no-restricted-syntax': ['error',
      // discourage default exports for clearer named aggregates
      {
        selector: "ExportDefaultDeclaration",
        message: 'Avoid default exports — prefer named exports for clarity in DDD and SRP.'
      }
    ],

    // TypeScript specific best practices
    '@typescript-eslint/explicit-module-boundary-types': 'warn',
    '@typescript-eslint/no-explicit-any': 'warn',
    '@typescript-eslint/consistent-type-definitions': ['error', 'interface']
  },
  overrides: [
    {
      files: ['*.dto.ts', '*.dto.tsx', '*.dto.jsx', '*.dto.js'],
      rules: {
        // stricter for DTO files
        'local-rules/single-type-per-file': 'error',
        'max-lines': ['error', { max: 120 }]
      }
    }
  ]
};

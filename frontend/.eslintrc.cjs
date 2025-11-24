module.exports = {
  root: true,
  parser: '@typescript-eslint/parser',
  parserOptions: {
    ecmaVersion: 2024,
    sourceType: 'module',
    project: './tsconfig.json',
  },
  plugins: ['@typescript-eslint'],
  env: {
    browser: true,
    node: true,
    es6: true,
  },
  extends: [
    'eslint:recommended',
    'plugin:@typescript-eslint/recommended'
  ],
  plugins: ['@typescript-eslint'],
  rules: {
    // Enforce single exported type/interface/class per DTO file
    // 'single-type-per-file': 'error',

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
      files: ['src/api/generated/**'],
      rules: {
        'max-lines': 'off',
        '@typescript-eslint/consistent-type-definitions': 'off',
        '@typescript-eslint/explicit-module-boundary-types': 'off'
      }
    }
  ]
};

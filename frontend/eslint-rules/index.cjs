// ./eslint-rules/index.cjs
const singleTypePerFileRule = require('./single-type-per-file.cjs');

module.exports = {
  rules: {
    'single-type-per-file': singleTypePerFileRule,
  },
};

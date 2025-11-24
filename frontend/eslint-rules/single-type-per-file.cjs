// ./eslint-rules/single-type-per-file.js
module.exports = {
  meta: {
    type: 'problem',
    docs: {
      description: 'Enforce a single exported type/interface/class per file.',
      category: 'Best Practices',
      recommended: true,
    },
    fixable: null,
    schema: [],
    messages: {
      multipleExports: 'File must export only one type, interface, or class.',
    },
  },
  create(context) {
    let exportCount = 0;
    const sourceCode = context.sourceCode || context.getSourceCode();

    return {
      // Counts the number of top-level type/interface/class exports
      'ExportNamedDeclaration > TSInterfaceDeclaration|ExportNamedDeclaration > TSTypeAliasDeclaration|ExportNamedDeclaration > ClassDeclaration'(node) {
        exportCount++;
        if (exportCount > 1) {
          context.report({
            node,
            messageId: 'multipleExports',
          });
        }
      },
    };
  },
};

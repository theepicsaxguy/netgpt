module.exports = {
  meta: {
    type: 'suggestion',
    docs: {
      description: 'Allow only one top-level type/interface/class/enum per file (DTO files).',
      category: 'Best Practices',
    },
    schema: [],
  },
  create(context) {
    let count = 0;

    function checkNode(node) {
      // Count declarations that represent types/value objects
      if (
        node.type === 'TSInterfaceDeclaration' ||
        node.type === 'TSTypeAliasDeclaration' ||
        node.type === 'ClassDeclaration' ||
        node.type === 'TSEnumDeclaration'
      ) {
        // if it's exported or file name contains dto, consider it a DTO-type
        const isExported = node.declare === true || (node.parent && node.parent.type === 'ExportNamedDeclaration');
        if (isExported) count++;
      }
    }

    return {
      Program() {
        count = 0;
      },
      'TSInterfaceDeclaration': checkNode,
      'TSTypeAliasDeclaration': checkNode,
      'ClassDeclaration': checkNode,
      'TSEnumDeclaration': checkNode,
      'Program:exit'() {
        if (count > 1) {
          context.report({
            loc: { line: 1, column: 0 },
            message: 'Files should expose at most one exported type/interface/class/enum (DTOs follow single responsibility). Found ' + count + '.',
          });
        }
      }
    };
  }
};

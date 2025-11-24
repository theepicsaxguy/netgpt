using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NetGPT.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SingleTypePerDtoAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "NG001";
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            "DTO should contain a single top-level type",
            "File '{0}' contains {1} top-level types; DTO files should contain a single type",
            "Design",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
        }

        private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
        {
            var root = context.Tree.GetRoot(context.CancellationToken);
            // Check if path contains DTOs folder - skip otherwise
            var path = context.Tree.FilePath ?? string.Empty;
            if (!path.Contains("/DTOs/") && !path.Contains("\\DTOs\\"))
                return;

            int topLevelCount = 0;
            foreach (var node in root.ChildNodes())
            {
                if (node is NamespaceDeclarationSyntax ns)
                {
                    foreach (var member in ns.Members)
                    {
                        if (member is TypeDeclarationSyntax || member is RecordDeclarationSyntax || member is EnumDeclarationSyntax)
                            topLevelCount++;
                    }
                }
                else if (node is TypeDeclarationSyntax || node is RecordDeclarationSyntax || node is EnumDeclarationSyntax)
                {
                    topLevelCount++;
                }
            }

            if (topLevelCount > 1)
            {
                var diag = Diagnostic.Create(Rule, Location.Create(context.Tree, new Microsoft.CodeAnalysis.Text.TextSpan(0, 0)), path, topLevelCount);
                context.ReportDiagnostic(diag);
            }
        }
    }
}

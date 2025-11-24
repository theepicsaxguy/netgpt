using Microsoft.CodeAnalysis.Diagnostics;

namespace NetGPT.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SingleTypePerDtoAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "NG001";
        private static readonly DiagnosticDescriptor Rule = new(
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
            SyntaxNode root = context.Tree.GetRoot(context.CancellationToken);
            // Check if path contains DTOs folder - skip otherwise
            string path = context.Tree.FilePath ?? string.Empty;
            if (!path.Contains("/DTOs/") && !path.Contains("\\DTOs\\"))
            {
                return;
            }

            int topLevelCount = 0;
            foreach (SyntaxNode node in root.ChildNodes())
            {
                if (node is NamespaceDeclarationSyntax ns)
                {
                    foreach (MemberDeclarationSyntax member in ns.Members)
                    {
                        if (member is TypeDeclarationSyntax or RecordDeclarationSyntax or EnumDeclarationSyntax)
                        {
                            topLevelCount++;
                        }
                    }
                }
                else if (node is TypeDeclarationSyntax or RecordDeclarationSyntax or EnumDeclarationSyntax)
                {
                    topLevelCount++;
                }
            }

            if (topLevelCount > 1)
            {
                Diagnostic diag = Diagnostic.Create(Rule, Location.Create(context.Tree, new Microsoft.CodeAnalysis.Text.TextSpan(0, 0)), path, topLevelCount);
                context.ReportDiagnostic(diag);
            }
        }
    }
}

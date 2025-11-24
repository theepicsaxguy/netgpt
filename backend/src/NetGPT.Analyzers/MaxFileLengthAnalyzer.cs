// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NetGPT.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MaxFileLengthAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "NG002";
        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            "File exceeds max allowed lines",
            "File '{0}' has {1} lines which exceeds the max of {2}",
            "Readability",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        private const int DefaultMaxLines = 200;

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
        }

        private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
        {
            string path = context.Tree.FilePath ?? string.Empty;
            if (path.Contains("/obj/") || path.Contains("/bin/") || path.EndsWith(".g.cs") || path.EndsWith(".designer.cs"))
            {
                return;
            }

            try
            {
                int lines = 0;
                using (StreamReader r = new(path))
                {
                    while (r.ReadLine() != null)
                    {
                        lines++;
                    }
                }
                if (lines > DefaultMaxLines)
                {
                    Diagnostic diag = Diagnostic.Create(Rule, Location.Create(context.Tree, new Microsoft.CodeAnalysis.Text.TextSpan(0, 0)), path, lines, DefaultMaxLines);
                    context.ReportDiagnostic(diag);
                }
            }
            catch
            {
                // ignore IO errors
            }
        }
    }
}

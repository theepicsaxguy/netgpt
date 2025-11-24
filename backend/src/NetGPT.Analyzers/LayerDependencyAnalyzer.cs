using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LayerDependencyAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "DDD002";
    private const string DomainNamespace = "NetGPT.Domain";
    private const string ApplicationNamespace = "NetGPT.Application";
    private const string InfrastructureNamespace = "NetGPT.Infrastructure";
    private const string ApiNamespace = "NetGPT.API";

    // List of external SDKs strictly forbidden in Domain/Application (Infra concerns)
    // Add any specific SDKs you want to ban here.
    private static readonly string[] BannedInfrastructureSdks = new[]
    {
        "Microsoft.EntityFrameworkCore", // Persistence details
        "Microsoft.AspNetCore",          // Web concerns
        "StackExchange.Redis",           // Caching impl
        "Azure",                         // Cloud specific
        "Amazon",                        // Cloud specific
        "Dapper",                        // Data access
        "RestSharp",                     // HTTP impl
        "System.Data.SqlClient"          // Database specific
    };

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Architectural Layer Violation",
        "Layer '{0}' cannot reference '{1}'. Violation of DDD Architecture.",
        "Architecture",
        DiagnosticSeverity.Error, // Make this an Error to break the build
        isEnabledByDefault: true,
        description: "Enforces strict direction of dependencies between DDD layers.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Check 'using' directives (e.g., "using NetGPT.Infrastructure;")
        context.RegisterSyntaxNodeAction(AnalyzeUsingDirective, SyntaxKind.UsingDirective);

        // Check fully qualified names in code (e.g., "var x = new NetGPT.Infrastructure.MyClass()")
        context.RegisterSyntaxNodeAction(AnalyzeQualifiedName, SyntaxKind.QualifiedName);
    }

    private void AnalyzeUsingDirective(SyntaxNodeAnalysisContext context)
    {
        var usingDirective = (UsingDirectiveSyntax)context.Node;
        var importedNamespace = usingDirective.Name.ToString();

        CheckDependency(context, usingDirective.GetLocation(), importedNamespace);
    }

    private void AnalyzeQualifiedName(SyntaxNodeAnalysisContext context)
    {
        var qualifiedName = (QualifiedNameSyntax)context.Node;
        var referencedNamespace = qualifiedName.ToString();

        // We only care about the root/prefix of the qualified name
        CheckDependency(context, qualifiedName.GetLocation(), referencedNamespace);
    }

    private void CheckDependency(SyntaxNodeAnalysisContext context, Location location, string referencedNamespace)
    {
        // 1. Determine which layer (project) the current file belongs to
        // We can guess this from the file path or the namespace defined in the file.
        // Using declared namespace is safer.
        var currentNamespace = GetCurrentNamespace(context.Node);

        if (string.IsNullOrEmpty(currentNamespace)) return;

        // 2. Apply Rules based on Current Layer

        // === DOMAIN LAYER RULES ===
        if (currentNamespace.StartsWith(DomainNamespace))
        {
            // Domain cannot reference App, Infra, API, or External Infra SDKs
            if (referencedNamespace.StartsWith(ApplicationNamespace) ||
                referencedNamespace.StartsWith(InfrastructureNamespace) ||
                referencedNamespace.StartsWith(ApiNamespace) ||
                IsBannedSdk(referencedNamespace))
            {
                ReportViolation(context, location, "Domain", referencedNamespace);
            }
        }

        // === APPLICATION LAYER RULES ===
        else if (currentNamespace.StartsWith(ApplicationNamespace))
        {
            // Application cannot reference Infra, API, or External Infra SDKs
            if (referencedNamespace.StartsWith(InfrastructureNamespace) ||
                referencedNamespace.StartsWith(ApiNamespace) ||
                IsBannedSdk(referencedNamespace))
            {
                ReportViolation(context, location, "Application", referencedNamespace);
            }
        }
    }

    private bool IsBannedSdk(string ns)
    {
        return BannedInfrastructureSdks.Any(banned => ns.StartsWith(banned));
    }

    private void ReportViolation(SyntaxNodeAnalysisContext context, Location location, string currentLayer, string violations)
    {
        var diagnostic = Diagnostic.Create(Rule, location, currentLayer, violations);
        context.ReportDiagnostic(diagnostic);
    }

    private string GetCurrentNamespace(SyntaxNode node)
    {
        // Traverse up to find the Namespace declaration
        var namespaceDecl = node.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault();
        return namespaceDecl?.Name.ToString();
    }
}
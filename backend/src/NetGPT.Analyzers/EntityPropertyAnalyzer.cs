using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EntityPropertyAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "DDD001";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Domain Encapsulation Violation",
        "Property '{0}' in Entity '{1}' must have a private or protected setter",
        "DomainDesign",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Domain entities should encapsulate state. Use methods to change state, not public setters.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // We hook into the "Property Declaration" syntax
        context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
    }

    private void AnalyzeProperty(SyntaxNodeAnalysisContext context)
    {
        var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;
        var classDeclaration = propertyDeclaration.Parent as ClassDeclarationSyntax;

        if (classDeclaration == null) return;

        // 1. CHECK: Is this an Entity?
        // We use the SemanticModel to check inheritance accurately.
        // Adjust "BaseEntity" or "IEntity" to match your actual Domain base class.
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
        if (classSymbol == null || !IsEntity(classSymbol)) return;

        // 2. CHECK: Does it have a setter?
        var setAccessor = propertyDeclaration.AccessorList?.Accessors
            .FirstOrDefault(a => a.IsKind(SyntaxKind.SetAccessorDeclaration) || a.IsKind(SyntaxKind.InitAccessorDeclaration));

        if (setAccessor == null) return; // Read-only properties are fine

        // 3. CHECK: Is the setter public?
        // If there are no modifiers on the "set", it takes the visibility of the property (usually public).
        // If there are modifiers, we check if 'private' or 'protected' is missing.
        bool isPropertyPublic = propertyDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
        bool hasRestrictedModifier = setAccessor.Modifiers.Any(m =>
            m.IsKind(SyntaxKind.PrivateKeyword) ||
            m.IsKind(SyntaxKind.ProtectedKeyword));

        // If the property is public, and the setter has no restrictions (meaning it is also public), flag it.
        if (isPropertyPublic && !hasRestrictedModifier)
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                setAccessor.GetLocation(),
                propertyDeclaration.Identifier.Text,
                classDeclaration.Identifier.Text);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private bool IsEntity(INamedTypeSymbol symbol)
    {
        // Check if the class is in the Domain Aggregates namespace
        return symbol.ContainingNamespace?.ToDisplayString() == "NetGPT.Domain.Aggregates";
    }
}
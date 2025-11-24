using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EntityPropertyCodeFixProvider)), Shared]
public class EntityPropertyCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(EntityPropertyAnalyzer.DiagnosticId);

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the syntax node
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var accessor = root.FindNode(diagnosticSpan).FirstAncestorOrSelf<AccessorDeclarationSyntax>();

        if (accessor == null) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Make setter private",
                createChangedDocument: c => MakePrivateAsync(context.Document, accessor, c),
                equivalenceKey: "MakePrivate"),
            diagnostic);
    }

    private async Task<Document> MakePrivateAsync(Document document, AccessorDeclarationSyntax accessor, CancellationToken cancellationToken)
    {
        // Create a "private" token
        var privateToken = SyntaxFactory.Token(SyntaxKind.PrivateKeyword);

        // Add "private" to the existing modifiers (or create new list)
        var newModifiers = accessor.Modifiers.Add(privateToken);

        // Create new accessor with the private modifier
        var newAccessor = accessor.WithModifiers(newModifiers);

        // Replace the old node with the new node
        var root = await document.GetSyntaxRootAsync(cancellationToken);
        var newRoot = root.ReplaceNode(accessor, newAccessor);

        return document.WithSyntaxRoot(newRoot);
    }
}
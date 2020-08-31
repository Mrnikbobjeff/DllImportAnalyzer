using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DllImportDefaultsAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DllImportDefaultsAnalyzerCodeFixProvider)), Shared]
    public class DllImportDefaultsAnalyzerCodeFixProvider : CodeFixProvider
    {
        private const string title = "Remove parameter";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DllImportDefaultsAnalyzerAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            foreach(var diagnostic in context.Diagnostics)
            {
                var diagnosticSpan = diagnostic.Location.SourceSpan;

                var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<AttributeArgumentSyntax>().First();

                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: title,
                        createChangedSolution: c => RemoveArgument(context.Document, declaration, c),
                        equivalenceKey: title),
                    diagnostic);
            }
        }

        private async Task<Solution> RemoveArgument(Document document, AttributeArgumentSyntax argument, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = root.RemoveNode(argument, SyntaxRemoveOptions.KeepNoTrivia);
            var originalSolution = document.Project.Solution;
            return originalSolution.WithDocumentSyntaxRoot(document.Id, newRoot);
        }
    }
}

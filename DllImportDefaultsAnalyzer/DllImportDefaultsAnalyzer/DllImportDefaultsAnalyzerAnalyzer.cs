using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DllImportDefaultsAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DllImportDefaultsAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DllImportDefaultsAnalyzer";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Styling";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSymbol, SyntaxKind.Attribute);
        }

        static bool IsDefaultValue(string parameter, ExpressionSyntax expression)
        {
            switch (parameter)
            {
                case "BestFitMapping":
                case "ExactSpelling":
                case "PreserveSig":
                case "SetLastError":
                case "ThrowOnUnmappableChar":
                    return expression.Kind() == SyntaxKind.FalseLiteralExpression;
                case "CallingConvention":
                    return expression is MemberAccessExpressionSyntax memberAccess && memberAccess.Name.Identifier.ValueText.Equals("WinApi");
                case "EntryPoint":
                    return expression.Kind() == SyntaxKind.NullLiteralExpression;
                case "CharSet":
                    return expression is MemberAccessExpressionSyntax memberAccessCharset && memberAccessCharset.Name.Identifier.ValueText.Equals("Ansi");
                default:
                    return false; //Unreachable unless other parameters are added
            }
        }

        private static void AnalyzeSymbol(SyntaxNodeAnalysisContext context)
        {
            var attribute = (AttributeSyntax)context.Node;
            if (!attribute.Name.ToFullString().Equals("DllImport"))
                return;

            var arguments = attribute.ArgumentList.Arguments;
            foreach(var arg in arguments.Skip(1))
            {
                if (IsDefaultValue(arg.NameEquals.Name.Identifier.ValueText, arg.Expression))
                {
                    var diagnostic = Diagnostic.Create(Rule, arg.GetLocation(), arg.NameEquals.Name.Identifier.ValueText);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

// Based on https://github.com/aspnet/Extensions/pull/345

namespace gfoidl.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PubternalityAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
        //---------------------------------------------------------------------
        public PubternalityAnalyzer()
        {
            this.SupportedDiagnostics = ImmutableArray.Create(new[]
            {
                PubturnalityDescriptors.GF0001
            });
        }
        //---------------------------------------------------------------------
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(analysisContext =>
            {
                analysisContext.RegisterSyntaxNodeAction(AnalyzeTypeUsage, SyntaxKind.IdentifierName);
            });
        }
        //---------------------------------------------------------------------
        private static void AnalyzeTypeUsage(SyntaxNodeAnalysisContext syntaxContext)
        {
            IdentifierNameSyntax identifier = syntaxContext.Node as IdentifierNameSyntax;

            TypeInfo symbolInfo = ModelExtensions.GetTypeInfo(syntaxContext.SemanticModel, identifier, syntaxContext.CancellationToken);
            ITypeSymbol type    = symbolInfo.Type;

            if (type == null)
                return;

            if (!IsInternal(type.ContainingNamespace))
            {
                // don't care about non-pubternal type references
                return;
            }

            SyntaxNode parent = identifier.Parent;
            if (parent.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                return;

            if (!syntaxContext.ContainingSymbol.ContainingAssembly.Equals(type.ContainingAssembly))
                syntaxContext.ReportDiagnostic(Diagnostic.Create(
                    PubturnalityDescriptors.GF0001,
                    identifier.GetLocation(),
                    type.ToDisplayString()));
        }
        //---------------------------------------------------------------------
        private static bool IsInternal(INamespaceSymbol ns)
        {
            while (ns != null)
            {
                if (ns.Name == "Internal")
                    return true;

                ns = ns.ContainingNamespace;
            }

            return false;
        }
    }
}

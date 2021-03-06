﻿using System.Collections.Immutable;
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
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationStartAnalysisContext =>
            {
                compilationStartAnalysisContext.RegisterSyntaxNodeAction(AnalyzeTypeUsage, SyntaxKind.IdentifierName);
            });
        }
        //---------------------------------------------------------------------
        private static void AnalyzeTypeUsage(SyntaxNodeAnalysisContext syntaxContext)
        {
            IdentifierNameSyntax identifier = syntaxContext.Node as IdentifierNameSyntax;
            ITypeSymbol type                = GetTypeInfo(syntaxContext, identifier);

            if (type == null) return;

            if (!IsNamespaceInternal(type.ContainingNamespace))
            {
                // don't care about non-pubternal type references
                return;
            }

            SyntaxNode parent = identifier.Parent;
            if (parent.IsKind(SyntaxKind.SimpleMemberAccessExpression))
            {
                var memberAccess = parent as MemberAccessExpressionSyntax;

                if (memberAccess.OperatorToken.IsKind(SyntaxKind.DotToken))
                {
                    ExpressionSyntax exp = memberAccess.Expression;
                    ITypeSymbol type1    = GetTypeInfo(syntaxContext, exp);

                    if (type1 != null && !IsNamespaceInternal(type1.ContainingNamespace))
                    {
                        return;
                    }
                }
            }

            if (syntaxContext.ContainingSymbol.ContainingAssembly != type.ContainingAssembly)
            {
                syntaxContext.ReportDiagnostic(Diagnostic.Create(
                    PubturnalityDescriptors.GF0001,
                    identifier.GetLocation(),
                    type.ToDisplayString()));
            }
        }
        //---------------------------------------------------------------------
        private static ITypeSymbol GetTypeInfo(SyntaxNodeAnalysisContext syntaxContext, SyntaxNode node)
        {
            TypeInfo symbolInfo = syntaxContext.SemanticModel.GetTypeInfo(node, syntaxContext.CancellationToken);

            return symbolInfo.Type;
        }
        //---------------------------------------------------------------------
        private static bool IsNamespaceInternal(INamespaceSymbol ns)
        {
            bool isInternal = false;
            bool isGfoidl   = false;

            while (ns != null)
            {
                if (ns.Name == "Internal")
                {
                    isInternal = true;
                }

                if (ns.Name == "gfoidl")
                {
                    isGfoidl = true;
                }

                ns = ns.ContainingNamespace;
            }

            return isInternal && isGfoidl;
        }
    }
}

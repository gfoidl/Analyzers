using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;

// Based on https://github.com/aspnet/Extensions/pull/345

namespace gfoidl.Analyzers.Tests
{
    public abstract class DiagnosticVerifier
    {
        protected static string s_defaultFilePathPrefix = "Test";
        protected static string s_testProjectName       = "TestProject";
        //---------------------------------------------------------------------
        protected Solution Solution { get; set; }
        //---------------------------------------------------------------------
        protected async Task<Diagnostic[]> GetDiagnosticsAsync(
            Document[] documents,
            DiagnosticAnalyzer analyzer,
            string[] additionalEnabledDiagnostics = null)
        {
            var projects = new HashSet<Project>();
            foreach (Document document in documents)
                projects.Add(document.Project);

            var diagnostics = new List<Diagnostic>();

            foreach (Project project in projects)
            {
                Compilation compilation = await project.GetCompilationAsync();

                // Enable any additional diagnostics
                CompilationOptions options = compilation.Options;

                if (additionalEnabledDiagnostics?.Length > 0)
                {
                    options = compilation.Options
                        .WithSpecificDiagnosticOptions(additionalEnabledDiagnostics.ToDictionary(s => s, _ => ReportDiagnostic.Info));
                }

                CompilationWithAnalyzers compilationWithAnalyzers = compilation
                    .WithOptions(options)
                    .WithAnalyzers(ImmutableArray.Create(analyzer));

                ImmutableArray<Diagnostic> diags = await compilationWithAnalyzers.GetAllDiagnosticsAsync();

                foreach (Diagnostic diag in diags)
                    TestContext.WriteLine("Diagnostics: " + diag);

                Assert.False(diags.Any(d => d.Id == "AD0001"));

                // Filter out non-error diagnostics not produced by our analyzer
                // We want to KEEP errors because we might have written bad code. But sometimes we leave warnings in to make the
                // test code more convenient
                diags = diags.Where(d =>
                    d.Severity == DiagnosticSeverity.Error
                    || analyzer.SupportedDiagnostics.Any(s => s.Id.Equals(d.Id)))
                    .ToImmutableArray();

                foreach (Diagnostic diag in diags)
                {
                    if (diag.Location == Location.None || diag.Location.IsInMetadata)
                    {
                        diagnostics.Add(diag);
                    }
                    else
                    {
                        foreach (Document document in documents)
                        {
                            SyntaxTree tree = await document.GetSyntaxTreeAsync();

                            if (tree == diag.Location.SourceTree)
                                diagnostics.Add(diag);
                        }
                    }
                }
            }

            return diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
        }
        //---------------------------------------------------------------------
        protected Project CreateProject(params string[] sources)
        {
            string fileNamePrefix = s_defaultFilePathPrefix;

            ProjectId projectId = ProjectId.CreateNewId(debugName: s_testProjectName);

            this.Solution = this.Solution ?? new AdhocWorkspace().CurrentSolution;

            CSharpCompilationOptions compilationOptions =
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
#if DEBUG
                .WithOptimizationLevel(OptimizationLevel.Debug);
#else
                .WithOptimizationLevel(OptimizationLevel.Release);
#endif

            this.Solution = this.Solution
                .AddProject(projectId, s_testProjectName, s_testProjectName, LanguageNames.CSharp)
                .WithProjectCompilationOptions(projectId, compilationOptions)
                .AddMetadataReference(projectId, MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

            int count = 0;
            foreach (string source in sources)
            {
                var newFileName = fileNamePrefix + count;

                TestContext.WriteLine("Adding file: " + newFileName + Environment.NewLine + source);

                DocumentId documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
                this.Solution = this.Solution.AddDocument(documentId, newFileName, SourceText.From(source));

                count++;
            }

            return this.Solution.GetProject(projectId);
        }
    }
}

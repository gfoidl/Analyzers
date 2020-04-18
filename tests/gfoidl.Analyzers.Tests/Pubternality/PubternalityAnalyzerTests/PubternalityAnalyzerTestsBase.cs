using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace gfoidl.Analyzers.Tests.PubternalityAnalyzerTests
{
    public abstract class PubternalityAnalyzerTestsBase : DiagnosticVerifier
    {
        protected Task<Diagnostic[]> GetDiagnosticsWithProjectReference(string code, string referenceProjectCode)
        {
            Project library     = this.CreateProject(referenceProjectCode);
            Project mainProject = this.CreateProject(code).AddProjectReference(new ProjectReference(library.Id));

            return this.GetDiagnosticsAsync(mainProject.Documents.ToArray(), new PubternalityAnalyzer());
        }
    }
}

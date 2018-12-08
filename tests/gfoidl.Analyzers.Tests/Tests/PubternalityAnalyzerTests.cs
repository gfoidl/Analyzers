using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace gfoidl.Analyzers.Tests
{
    [TestFixture]
    public class PubternalityAnalyzerTests : DiagnosticVerifier
    {
        [Test]
        public async Task Public_type_used___no_warning_or_error()
        {
            string library = @"
namespace MyLib.Services
{
    public class Bar
    {
        public void Do() { }
    }
}";

            TestSource code = TestSource.Read(@"
using MyLib.Services;

namespace MyProgram
{
    internal class Worker
    {
        private /*MM*/Bar _bar = new Bar();
    }
}");
            Diagnostic[] diagnostics = await this.GetDiagnosticsWithProjectReference(code.Source, library);

            Assert.AreEqual(0, diagnostics.Length);
        }
        //---------------------------------------------------------------------
        [Test]
        public async Task Issue_1_public_type_declared_used_to_hide_pubternals___no_warning_or_error()
        {
            string library = @"
namespace MyLib
{
    using MyLib.Internal;

    public abstract class Base
    {
        private static readonly Bar s_bar = new Bar();

        public static Bar Default => s_bar;

        public abstract void Do();
    }
}

namespace MyLib.Internal
{
    public sealed class Bar : Base
    {
        public override void Do() { }
    }
}";

            TestSource code = TestSource.Read(@"
using MyLib;

namespace MyProgram
{
    internal class Worker
    {
        public void Work()
        {
            Base.Default.Do();
        }
    }
}");
            Diagnostic[] diagnostics = await this.GetDiagnosticsWithProjectReference(code.Source, library);

            Assert.AreEqual(0, diagnostics.Length);
        }
        //---------------------------------------------------------------------
        [Test]
        public async Task Pubternal_type_used___warning_GF0001()
        {
            string library = @"
namespace MyLib.Internal
{
    public class Bar
    {
        public void Do() { }
    }
}";

            TestSource code = TestSource.Read(@"
using MyLib.Internal;

namespace MyProgram
{
    internal class Worker
    {
        private /*MM*/Bar _bar = new Bar();
    }
}");
            Diagnostic[] diagnostics    = await this.GetDiagnosticsWithProjectReference(code.Source, library);
            DiagnosticLocation expected = code.DefaultMarkerLocation;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, diagnostics.Length);

                Diagnostic diagnostic = diagnostics[0];
                Assert.AreEqual("GF0001", diagnostic.Id);
                Assert.IsTrue(diagnostic.Location.IsInSource);
                Assert.That(diagnostic.Location, Is.EqualTo(expected));
            });
        }
        //---------------------------------------------------------------------
        [Test]
        public async Task Pubternal_type_in_nested_namespace___warning_GF0001()
        {
            string library = @"
namespace MyLib.Internal.Services
{
    public class Bar
    {
        public void Do() { }
    }
}";

            TestSource code = TestSource.Read(@"
using MyLib.Internal.Services;

namespace MyProgram
{
    internal class Worker
    {
        private /*MM*/Bar _bar = new Bar();
    }
}");
            Diagnostic[] diagnostics    = await this.GetDiagnosticsWithProjectReference(code.Source, library);
            DiagnosticLocation expected = code.DefaultMarkerLocation;

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, diagnostics.Length);

                Diagnostic diagnostic = diagnostics[0];
                Assert.AreEqual("GF0001", diagnostic.Id);
                Assert.IsTrue(diagnostic.Location.IsInSource);
                Assert.That(diagnostic.Location, Is.EqualTo(expected));
            });
        }
        //---------------------------------------------------------------------
        private Task<Diagnostic[]> GetDiagnosticsWithProjectReference(string code, string referenceProjectCode)
        {
            Project library     = this.CreateProject(referenceProjectCode);
            Project mainProject = this.CreateProject(code).AddProjectReference(new ProjectReference(library.Id));

            return this.GetDiagnosticsAsync(mainProject.Documents.ToArray(), new PubternalityAnalyzer());
        }
    }
}

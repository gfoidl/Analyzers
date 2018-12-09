using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace gfoidl.Analyzers.Tests.Tests.PubternalityAnalyzerTests
{
    [TestFixture]
    public  class PublicTypes : PubternalityAnalyzerTestsBase
    {
        [Test]
        public async Task Field___no_warning_or_error()
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
        private Bar _bar = new Bar();
    }
}");
            Diagnostic[] diagnostics = await this.GetDiagnosticsWithProjectReference(code.Source, library);

            Assert.AreEqual(0, diagnostics.Length);
        }
        //---------------------------------------------------------------------
        [Test]
        public async Task Local___no_warning_or_error()
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
        public void Do()
        {
            Bar bar = new Bar();
        }
    }
}");
            Diagnostic[] diagnostics = await this.GetDiagnosticsWithProjectReference(code.Source, library);

            Assert.AreEqual(0, diagnostics.Length);
        }
        //---------------------------------------------------------------------
        [Test]
        public async Task Local_declared_as_var___no_warning_or_error()
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
        public void Do()
        {
            var bar = new Bar();
        }
    }
}");
            Diagnostic[] diagnostics = await this.GetDiagnosticsWithProjectReference(code.Source, library);

            Assert.AreEqual(0, diagnostics.Length);
        }
        //---------------------------------------------------------------------
        [Test]
        public async Task Issue_1_used_to_hide_pubternals___no_warning_or_error()
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
    }
}

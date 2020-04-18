﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Framework;

namespace gfoidl.Analyzers.Tests.PubternalityAnalyzerTests
{
    [TestFixture]
    public class PubternalTypesNotInNsGfoidl : PubternalityAnalyzerTestsBase
    {
        [Test]
        public async Task Field___warning_GF0001()
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
            Diagnostic[] diagnostics = await this.GetDiagnosticsWithProjectReference(code.Source, library);

            Assert.AreEqual(0, diagnostics.Length);
        }
        //---------------------------------------------------------------------
        [Test]
        public async Task Pubternal_type_in_nested_namespace_field___warning_GF0001()
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
            Diagnostic[] diagnostics = await this.GetDiagnosticsWithProjectReference(code.Source, library);

            Assert.AreEqual(0, diagnostics.Length);
        }
        //---------------------------------------------------------------------
        [Test]
        public async Task Local___warning_GF0001()
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
        public void Do()
        {
            /*MM*/Bar bar = new Bar();
        }
    }
}");
            Diagnostic[] diagnostics = await this.GetDiagnosticsWithProjectReference(code.Source, library);

            Assert.AreEqual(0, diagnostics.Length);
        }
        //---------------------------------------------------------------------
        [Test]
        public async Task Local_declared_with_var___warning_GF0001()
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
        public void Do()
        {
            /*MM*/var bar = new Bar();
        }
    }
}");
            Diagnostic[] diagnostics = await this.GetDiagnosticsWithProjectReference(code.Source, library);

            Assert.AreEqual(0, diagnostics.Length);
        }
        //---------------------------------------------------------------------
        [Test]
        public async Task Issue_1_used_to_hide_pubternals___warning_GF0001()
        {
            string library = @"
namespace MyLib.Internal
{
    public abstract class Base
    {
        private static readonly Bar s_bar = new Bar();

        public static Bar Default => s_bar;

        public abstract void Do();
    }

    public sealed class Bar : Base
    {
        public override void Do() { }
    }
}";

            TestSource code = TestSource.Read(@"
using MyLib.Internal;

namespace MyProgram
{
    internal class Worker
    {
        public void Work()
        {
            /*M0*/Base./*M1*/Default.Do();
        }
    }
}", "M0", "M1");

            Diagnostic[] diagnostics = await this.GetDiagnosticsWithProjectReference(code.Source, library);

            Assert.AreEqual(0, diagnostics.Length);
        }
    }
}

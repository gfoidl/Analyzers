using Microsoft.CodeAnalysis;

namespace gfoidl.Analyzers
{
    internal static class PubturnalityDescriptors
    {
        public static readonly DiagnosticDescriptor GF0001 = new DiagnosticDescriptor(
            "GF0001",
            "Cross assembly pubternal reference",
            "Cross assembly pubternal type ('{0}') reference. This type may be changed to 'internal', so don't use it.",
            "Usage",
            DiagnosticSeverity.Warning, true);
    }
}

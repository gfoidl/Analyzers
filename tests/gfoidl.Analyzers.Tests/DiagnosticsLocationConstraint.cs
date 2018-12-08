using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework.Constraints;

namespace gfoidl.Analyzers.Tests
{
    public class DiagnosticsLocationConstraint : Constraint
    {
        private readonly object _expected;
        //---------------------------------------------------------------------
        public DiagnosticsLocationConstraint(object expected) => _expected = expected;
        //---------------------------------------------------------------------
        public override ConstraintResult ApplyTo<TActual>(TActual actual)
        {
            Location location           = actual    as Location;
            DiagnosticLocation expected = _expected as DiagnosticLocation;

            if (location == null)
            {
                this.Description = $"{nameof(actual)} is not a {nameof(Location)}";
                return new ConstraintResult(this, actual, ConstraintStatus.Failure);
            }

            if (expected == null)
            {
                this.Description = $"{nameof(expected)} is not a {nameof(DiagnosticLocation)}";
                return new ConstraintResult(this, expected, ConstraintStatus.Failure);
            }

            FileLinePositionSpan actualSpan = location.GetLineSpan();
            LinePosition actualLinePosition = actualSpan.StartLinePosition;

            if (actualLinePosition.Line > 0 && actualLinePosition.Line + 1 != expected.Line)
            {
                this.Description = $"Expected diagnostic to be on line \"{expected.Line}\" was actually on line \"{actualLinePosition.Line + 1}\"";
                return new ConstraintResult(this, actual, ConstraintStatus.Failure);
            }

            if (actualLinePosition.Character > 0 && actualLinePosition.Character + 1 != expected.Column)
            {
                this.Description = $"Expected diagnostic to start at column \"{expected.Column}\" was actually on column \"{actualLinePosition.Character + 1}\"";
                return new ConstraintResult(this, actual, ConstraintStatus.Failure);
            }

            return new ConstraintResult(this, actual, ConstraintStatus.Success);
        }
    }
    //-------------------------------------------------------------------------
    public class Is : NUnit.Framework.Is
    {
        public static new DiagnosticsLocationConstraint EqualTo(object expected)
        {
            return new DiagnosticsLocationConstraint(expected);
        }
    }
}

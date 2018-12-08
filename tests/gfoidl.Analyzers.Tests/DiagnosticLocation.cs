using System;

namespace gfoidl.Analyzers.Tests
{
    public class DiagnosticLocation
    {
        public int Line   { get; }
        public int Column { get; }
        //---------------------------------------------------------------------
        public DiagnosticLocation(int line, int column)
        {
            if (line   < -1) throw new ArgumentOutOfRangeException(nameof(line)  , "line must be >= -1");
            if (column < -1) throw new ArgumentOutOfRangeException(nameof(column), "column must be >= -1");

            this.Line   = line;
            this.Column = column;
        }
    }
}

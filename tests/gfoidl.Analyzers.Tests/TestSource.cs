using System;
using System.Collections.Generic;
using System.Linq;

namespace gfoidl.Analyzers.Tests
{
    public class TestSource
    {
        private const string MarkerStart = "/*MM";
        private const string MarkerEnd   = "*/";
        //---------------------------------------------------------------------
        public IDictionary<string, DiagnosticLocation> MarkerLocations { get; }
        public DiagnosticLocation DefaultMarkerLocation                { get; private set; }
        public string Source                                           { get; private set; }
        //---------------------------------------------------------------------
        public TestSource()
        {
            this.MarkerLocations = new Dictionary<string, DiagnosticLocation>(StringComparer.Ordinal);
        }
        //---------------------------------------------------------------------
        public static TestSource Read(string rawSource, params string[] markerNames)
        {
            string[] markerStarts;

            if (markerNames.Length == 0)
                markerStarts = new[] { MarkerStart };
            else
                markerStarts = markerNames.Select(m => $"/*{m}").ToArray();

            var testInput = new TestSource();

            string[] lines = rawSource.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; ++i)
            {
                string line = lines[i];

                foreach (string markerStart in markerStarts)
                {
                    int markerStartIndex = line.IndexOf(markerStart, StringComparison.Ordinal);

                    if (markerStartIndex == -1) continue;

                    int markerEndIndex = line.IndexOf(MarkerEnd, markerStartIndex, StringComparison.Ordinal);
                    string markerName  = line.Substring(markerStartIndex + 2, markerEndIndex - markerStartIndex - 2);
                    var markerLocation = new DiagnosticLocation(i + 1, markerStartIndex + 1);

                    if (testInput.DefaultMarkerLocation == null)
                        testInput.DefaultMarkerLocation = markerLocation;

                    testInput.MarkerLocations.Add(markerName, markerLocation);
                    line = line.Substring(0, markerStartIndex) + line.Substring(markerEndIndex + MarkerEnd.Length);
                }

                lines[i] = line;
            }

            testInput.Source = string.Join(Environment.NewLine, lines);

            return testInput;
        }
    }
}

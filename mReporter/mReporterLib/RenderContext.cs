using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{

    public class RenderContext
    {
        internal Report Report { get; private set; }


        public RenderContext(Report report)
        {
            this.Report = report;
            _currentOutputLine = null;
            _outputLines = new List<OutputLine>();
        }

        OutputLine _currentOutputLine;
        List<OutputLine> _outputLines;
        internal List<OutputLine> OutputLines { get { return _outputLines; } }

        internal IEnumerable<OutputLine> GetLines(OutputLine parent)
        {
            foreach (var oLine in _outputLines.Where(r => r.Parent == parent)) {
                yield return oLine;
            }
        }

        internal OutputLine AddToOutput(ReportItem item, List<string> data)
        {
            var oLine = new OutputLine(_currentOutputLine, item, data);
            _outputLines.Add(oLine);
            return oLine;
        }

        internal OutputLine AddToOutput(ReportItem item, string data)
        {
            var oLine = new OutputLine(_currentOutputLine, item, data);
            _outputLines.Add(oLine);
            return oLine;
        }

        internal void SetOutputParent(OutputLine newOutputParent)
        {
            _currentOutputLine = newOutputParent;
        }

        /// <summary>
        /// Helper method to simplify creation of code sequences
        /// </summary>
        internal char[] CreateCode(params int[] codes)
        {
            var result = new char[codes.Length];
            for (int i = 0; i < codes.Length; i++) result[i] = (char)codes[i];
            return result;
        }


    }

}

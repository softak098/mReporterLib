using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace mReporterLib
{

    /// <summary>
    /// Represents line with text and additional lines with text
    /// </summary>
    public class LineElement : OutputElement
    {
        List<OutputLineLine> _lines;

        internal List<OutputLineLine> Lines => _lines ?? (_lines = new List<OutputLineLine>());
        internal override int LineCount => _lines.Count;

        OutputLineLine _currentLine;
        private readonly RenderContext _context;

        /// <summary>
        /// Controls if output builder inserts LF character after the line, Default=TRUE
        /// </summary>
        public bool AppendNewLine { get; set; } = true;

        public LineElement(RenderContext context)
        {
            _context = context;
        }

        internal void AppendLine()
        {
            _currentLine = new OutputLineLine();
            Lines.Add(_currentLine);
        }

        internal void Append(string data)
        {
            if (_currentLine == null) AppendLine();
            _currentLine.Append(data);
        }

        /// <summary>
        /// Appends string data to line and applies esc codes to it
        /// </summary>
        internal void Append(string data, params EscCode[] codes)
        {
            if (_currentLine == null) AppendLine();
            _currentLine.Append(data, codes);
        }

        /// <summary>
        /// Appends esc code to line
        /// </summary>
        internal void Append(EscCode code)
        {
            _currentLine.Append(code);
        }

        /// <summary>
        /// Apply esc code(s) on every line
        /// </summary>
        internal void Apply(params EscCode[] codes)
        {
            Lines.ForEach(l => l.Apply(codes));
        }

        public override void WriteTo(Stream stream, Encoding textEncoding)
        {
            Lines.ForEach(l => {

                l.WriteTo(stream, textEncoding);

                if (AppendNewLine)
                    _context.Report.Dialect.NewLine.WriteTo(stream, textEncoding);

            });
        }

        public void WriteTo(Stream stream, Encoding textEncoding, int from, int count)
        {
            count = from + count;
            while (from < LineCount && from < count) {

                Lines[from].WriteTo(stream, textEncoding);

                if (AppendNewLine)
                    _context.Report.Dialect.NewLine.WriteTo(stream, textEncoding);

                from++;
            }
        }


    }


}

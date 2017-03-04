using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace mReporterLib
{


    public class LineElement : OutputElement
    {

        List<OutputLineLine> _lines;
        internal List<OutputLineLine> Lines
        {
            get
            {
                if (_lines == null) _lines = new List<OutputLineLine>();
                return _lines;
            }
        }

        OutputLineLine _currentLine;


        /// <summary>
        /// Controls if output builder inserts LF character after the line, Default=TRUE
        /// </summary>
        public bool AppendNewLine { get; set; } = true;

        internal override int LineCount
        {
            get
            {
                return _lines.Count;
            }
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


        }

        public override void WriteTo(Stream stream, Encoding textEncoding)
        {
        }

        public void WriteTo(Stream stream, Encoding textEncoding, int from, int count)
        {


        }


    }


}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace mReporterLib
{

    class OutputLineLine
    {
        List<OutputElement> _elements;
        internal int LineHeight;

        public OutputLineLine(int lineHeight = 1)
        {
            _elements = new List<OutputElement>();
            LineHeight = lineHeight;
        }

        internal void Append(OutputElement element)
        {
            _elements.Add(element);
        }

        internal void Append(string value)
        {
            _elements.Add(new TextElement(value));
        }

        internal void Append(string value, params EscCode[] codes)
        {
            _elements.Add(new TextElement(value, codes));
        }

        internal void Append(EscCode code)
        {
            _elements.Add(code);
        }

    }



    public class TextElement : OutputElement
    {
        string _data;

        public TextElement(string data)
        {
            _data = data;
        }

        public TextElement(string data, params EscCode[] codes)
        {
            _data = data;
        }

        public override void WriteTo(Stream stream, Encoding textEncoding)
        {
            if (string.IsNullOrWhiteSpace(_data)) return;
            var buffer = textEncoding.GetBytes(_data);
            stream.Write(buffer, 0, buffer.Length);
        }
    }


}

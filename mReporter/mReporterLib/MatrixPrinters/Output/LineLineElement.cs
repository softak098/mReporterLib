using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace mReporterLib
{

    class OutputLineLine : IWriteToStream
    {
        List<EscCode> _codes;
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

        internal void Append(EscCodePair code)
        {
            _elements.Add(code);
        }

        internal void Apply(EscCode[] codes)
        {
            if (_codes == null) _codes = new List<EscCode>(codes);
            else {
                _codes.AddRange(codes);
            }
        }

        public void WriteTo(Stream stream, Encoding textEncoding)
        {
            if (_codes != null) {

                _codes.ForEach(c => c?.WriteTo(stream, textEncoding, false));
                _elements.ForEach(e => e.WriteTo(stream, textEncoding));
                _codes.ForEach(c => c?.WriteTo(stream, textEncoding, true));

            }
            else _elements.ForEach(e => e.WriteTo(stream, textEncoding));
        }

    }



    public class TextElement : OutputElement
    {
        string _data;
        List<EscCode> _codes;

        public TextElement(string data)
        {
            _data = data;
        }

        public TextElement(string data, params EscCode[] codes)
        {
            _data = data;
            _codes = new List<EscCode>(codes);
        }

        public override void WriteTo(Stream stream, Encoding textEncoding)
        {
            if (_data == null) return;

            if (_codes != null) {

                _codes.ForEach(c => c?.WriteTo(stream, textEncoding, false));

                var buffer = textEncoding.GetBytes(_data);
                stream.Write(buffer, 0, buffer.Length);

                _codes.ForEach(c => c?.WriteTo(stream, textEncoding, true));

            }
            else {
                var buffer = textEncoding.GetBytes(_data);
                stream.Write(buffer, 0, buffer.Length);
            }

        }
    }


}

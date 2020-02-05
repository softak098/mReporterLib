using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace mReporterLib
{

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

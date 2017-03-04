using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace mReporterLib
{

    public class BinaryData : OutputElement
    {
        private MemoryStream _stream;

        public BinaryData(MemoryStream ms)
        {
            this._stream = ms;
        }

        public override void WriteTo(Stream stream, Encoding textEncoding)
        {
            _stream?.WriteTo(stream);
        }

    }

}

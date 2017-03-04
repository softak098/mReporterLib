using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace mReporterLib
{

    public class EscCode : OutputElement
    {
        byte[] _code;

        public EscCode(params byte[] codes)
        {
            _code = codes;
        }

        public override void WriteTo(Stream stream, Encoding textEncoding)
        {
            stream.Write(_code, 0, _code.Length);
        }

        public virtual void WriteTo(Stream stream, Encoding textEncoding, bool endSequence)
        {
            if (!endSequence) WriteTo(stream, textEncoding);
        }
    }


    public class EscCodePair : EscCode
    {
        public EscCode Start;
        public EscCode End;

        public EscCodePair(EscCode start, EscCode end)
        {
            Start = start; End = end;
        }

        public override void WriteTo(Stream stream, Encoding textEncoding)
        {
            Start?.WriteTo(stream, textEncoding);
            End?.WriteTo(stream, textEncoding);
        }

        public override void WriteTo(Stream stream, Encoding textEncoding, bool endSequence)
        {
            if (endSequence) End?.WriteTo(stream, textEncoding);
            else Start?.WriteTo(stream, textEncoding);
        }

    }

}

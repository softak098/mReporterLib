using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace mReporterLib
{

    public class EscCode : OutputElement
    {
        public string Start;
        public string End;

        public EscCode(string start, string end)
        {
            Start = start; End = end;
        }

        public string Apply(string source)
        {
            return string.Concat(Start ?? "", source, End ?? "");
        }

        public string Apply(EscCode code)
        {
            return this.Apply(code.Start ?? "" + code.End ?? "");
        }

        public string Apply(params int[] codes)
        {
            var code = CreateCode(codes);
            return Apply(code);
        }

        internal static EscCode CreateCode(params int[] codes)
        {
            var result = new char[codes.Length];
            for (int i = 0; i < codes.Length; i++) result[i] = (char)codes[i];
            return new EscCode(new string(result), null);
        }

        public override string ToString()
        {
            return string.Concat(Start ?? "", End ?? "");
        }

        public override void WriteTo(Stream stream, Encoding textEncoding)
        {
            throw new NotImplementedException();
        }
    }

}

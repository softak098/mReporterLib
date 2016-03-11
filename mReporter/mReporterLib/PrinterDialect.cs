using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{

    public class EscCode
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
    }

    public static class SequenceExtensions
    {
        public static string ApplyEscCode(this EscCode s, string value)
        {
            if (s == null) return value;
            return s.Apply(value);
        }

        public static string ApplyEscCode(this string s, params EscCode[] seq)
        {
            if (s == null) return null;
            StringBuilder sb = new StringBuilder();
            foreach (var item in seq) if (item != null) sb.Append(item.Start ?? "");
            sb.Append(s);
            foreach (var item in seq) if (item != null) sb.Append(item.End ?? "");
            return sb.ToString();
        }

    }

    /// <summary>
    /// Base class for all printer dialects, defines control codes for formatting, etc.
    /// </summary>
    public class PrinterDialect
    {
        public virtual EscCode FontStyle(FontStyle style) { return null; }

        public virtual EscCode Reset() { return new EscCode("\x1b@", null); }

        public virtual EscCode FormFeed() { return new EscCode("\x0C", null); }
        public virtual EscCode PrintStyle(PrintStyle style) { return null; }
    }


    public class ESCPDialect : PrinterDialect
    {
        public override EscCode FontStyle(FontStyle style)
        {
            switch (style) {
                case mReporterLib.FontStyle.Emphasized: return new EscCode("\u001bE", "\u001bF");
                case mReporterLib.FontStyle.Underline:
                    return new EscCode("\x1B-1", "\x1B-0");
                case mReporterLib.FontStyle.Italic:
                    return new EscCode("\x1B4", "\x1B5");
                default:
                    break;
            }
            return null;
        }

        public override EscCode PrintStyle(PrintStyle style)
        {
            switch (style) {
                case mReporterLib.PrintStyle.Pica:return new EscCode("\x1BP", null);
                case mReporterLib.PrintStyle.Elite:
                    return new EscCode("\x1BM", null);
                case mReporterLib.PrintStyle.Condensed:
                    return new EscCode("\x0F", "\x12");
                default:
                    break;
            }
            return null;
        }
    }


    public class ESCPosDialect : PrinterDialect
    {
        public override EscCode FontStyle(FontStyle style)
        {
            switch (style) {
                case mReporterLib.FontStyle.Emphasized:
                    return new EscCode("\u001bE1", "\u001bE0");
                case mReporterLib.FontStyle.Underline:
                    return new EscCode("\x1B-1", "\x1B-0");
                case mReporterLib.FontStyle.UnderlineDouble: return new EscCode("\x1B-2", "\x1B-0");
                case mReporterLib.FontStyle.Inverse:
                    return new EscCode("\u001dB1", "\u001dB0");
                default:
                    break;
            }
            return null;
        }

    }

}

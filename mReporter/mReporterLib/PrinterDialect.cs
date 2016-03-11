using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{

    public class Sequence
    {
        public string Start;
        public string End;

        public Sequence(string start, string end)
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
        public static string ApplySequence(this Sequence s, string value)
        {
            if (s == null) return value;
            return s.Apply(value);
        }

        public static string ApplySequence(this string s, params Sequence[] seq)
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
        public virtual Sequence FontStyleSequence(FontStyle style) { return null; }

        public virtual Sequence Reset() { return new Sequence("\x1b@", null); }

        public virtual Sequence FormFeed() { return new Sequence("\x0C", null); }
        public virtual Sequence PrintStyleSequence(PrintStyle style) { return null; }
    }


    public class ESCPDialect : PrinterDialect
    {
        public override Sequence FontStyleSequence(FontStyle style)
        {
            switch (style) {
                case FontStyle.Emphasized: return new Sequence("\u001bE", "\u001bF");
                case FontStyle.Underline:
                    return new Sequence("\x1B-1", "\x1B-0");
                case FontStyle.Italic:
                    return new Sequence("\x1B4", "\x1B5");
                default:
                    break;
            }
            return null;
        }

        public override Sequence PrintStyleSequence(PrintStyle style)
        {
            switch (style) {
                case PrintStyle.Pica:return new Sequence("\x1BP", null);
                case PrintStyle.Elite:
                    return new Sequence("\x1BM", null);
                case PrintStyle.Condensed:
                    return new Sequence("\x0F", "\x12");
                default:
                    break;
            }
            return null;
        }
    }


    public class ESCPosDialect : PrinterDialect
    {
        public override Sequence FontStyleSequence(FontStyle style)
        {
            switch (style) {
                case FontStyle.Emphasized:
                    return new Sequence("\u001bE1", "\u001bE0");
                case FontStyle.Underline:
                    return new Sequence("\x1B-1", "\x1B-0");
                case FontStyle.UnderlineDouble: return new Sequence("\x1B-2", "\x1B-0");
                case FontStyle.Inverse:
                    return new Sequence("\u001dB1", "\u001dB0");
                default:
                    break;
            }
            return null;
        }

    }

}

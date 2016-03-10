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

        public Sequence(string start,string end)
        {
            Start = start;End = end;
        }
    }

    /// <summary>
    /// Base class for all printer dialects, defines control codes for formatting, etc.
    /// </summary>
    public abstract class PrinterDialect
    {

        public abstract Sequence FontStyleSequence(FontStyle style);
    }


    public class ESCPDialect : PrinterDialect
    {
        public override Sequence FontStyleSequence(FontStyle style)
        {
            switch (style) {
                case FontStyle.Emhasized:return new Sequence("\u001bE", "\u001bF");
                case FontStyle.Underline:
                    break;
                case FontStyle.Italic:
                    break;
                case FontStyle.Inverse:
                    break;
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
                case FontStyle.Normal:
                    break;
                case FontStyle.Emhasized:
                    return new Sequence("\u001bE1", "\u001bE0");
                case FontStyle.Underline:
                    break;
                case FontStyle.Italic:
                    break;
                case FontStyle.Inverse:
                    return new Sequence("\u001dB1", "\u001dB0");
                default:
                    break;
            }
            return null;
        }
    }

}

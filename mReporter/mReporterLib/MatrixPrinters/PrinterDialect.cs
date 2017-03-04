using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{

    public enum PrinterModel
    {
        /// <summary>
        /// Generic Epson compatible printers with ESC/P support
        /// </summary>
        EpsonGeneric = 0,
        /// <summary>
        /// Epson ESC/POS + compatible printers
        /// </summary>
        EpsonPosGeneric,
        /// <summary>
        /// OEM ZJ-5802 thermal printers + compatible devices
        /// </summary>
        ZJ5802
    }


    /// <summary>
    /// Base class for all printer dialects, defines control codes for formatting, etc.
    /// </summary>
    public class PrinterDialect
    {
        static readonly EscCode _alignLeft = new EscCode(27, (byte)'a', (byte)'0');
        static readonly EscCode _alignRight = new EscCode(27, (byte)'a', (byte)'2');
        static readonly EscCode _alignCenter = new EscCode(27, (byte)'a', (byte)'1');
        static readonly EscCode _alignJustify = new EscCode(27, (byte)'a', (byte)'3');

        static readonly EscCode _lineFeed = new EscCode(10);

        public PrinterModel PrinterModel { get; set; }

        public PrinterDialect(PrinterModel model)
        {
            PrinterModel = model;
        }

        public virtual EscCodePair FontStyle(FontStyle style) { return null; }

        public virtual EscCode Reset() { return new EscCode(27, (byte)'@'); }

        public virtual EscCode FormFeed() { return new EscCode(12); }
        public virtual EscCode LineFeed => _lineFeed;

        public virtual EscCodePair PrintStyle(PrintStyle style) { return null; }
        public virtual EscCode Align(Align style)
        {
            switch (style) {
                default: return null;
                case mReporterLib.Align.Left: return _alignLeft;
                case mReporterLib.Align.Right: return _alignRight;
                case mReporterLib.Align.Center: return _alignCenter;
                case mReporterLib.Align.Justify: return _alignJustify;
            }
        }
    }


    public class ESCPDialect : PrinterDialect
    {

        public ESCPDialect(PrinterModel model) : base(model)
        {
        }

        public override EscCodePair FontStyle(FontStyle style)
        {
            /*
            switch (style) {
                case mReporterLib.FontStyle.Emphasized: return _fontStyleEmphasized;
                case mReporterLib.FontStyle.Underline:return _fontStyleUnderline;
                case mReporterLib.FontStyle.UnderlineDouble: return _fontStyleUnderlineDouble;
                case mReporterLib.FontStyle.Italic:
                    return new EscCodePair("\x1B4", "\x1B5");
                default:
                    break;
            }
            */
            return null;
        }

        public override EscCodePair PrintStyle(PrintStyle style)
        {
            /*
            switch (style) {
                case mReporterLib.PrintStyle.Pica: return new EscCodePair("\x1BP", null);
                case mReporterLib.PrintStyle.Elite:
                    return new EscCodePair("\x1BM", null);
                case mReporterLib.PrintStyle.Condensed:
                    return new EscCodePair("\x0F", "\x12");
                default:
                    break;
            }
            */
            return null;
        }
    }


    public class ESCPosDialect : PrinterDialect
    {
        static readonly EscCodePair _fontStyleEmphasized = new EscCodePair(new EscCode(27, 69, 1), new EscCode(27, 69, 0));
        static readonly EscCodePair _fontStyleUnderline = new EscCodePair(new EscCode(27, 45, 1), new EscCode(27, 45, 0));
        static readonly EscCodePair _fontStyleUnderlineDouble = new EscCodePair(new EscCode(27, 45, 2), new EscCode(27, 45, 0));
        static readonly EscCodePair _fontStyleInverse = new EscCodePair(new EscCode(29, 66, 1), new EscCode(29, 66, 0));

        public ESCPosDialect(PrinterModel model) : base(model)
        {
        }

        public override EscCodePair FontStyle(FontStyle style)
        {
            switch (style) {
                case mReporterLib.FontStyle.Emphasized: return _fontStyleEmphasized;
                case mReporterLib.FontStyle.Underline: return _fontStyleUnderline;
                case mReporterLib.FontStyle.UnderlineDouble: return _fontStyleUnderlineDouble;
                case mReporterLib.FontStyle.Inverse: return _fontStyleInverse;
                default:
                    break;
            }
            return null;
        }

    }

}

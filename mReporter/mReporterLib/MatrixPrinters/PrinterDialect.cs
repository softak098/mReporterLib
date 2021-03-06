﻿using System;
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
        ZJ5802,
        /// <summary>
        /// Star Micronics printers with StartLine/T emulation
        /// </summary>
        StarLineT
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
        static readonly EscCode _formFeed = new EscCode(12);
        static readonly EscCode _cr = new EscCode(13);
        static readonly EscCode _newLine = new EscCode(13, 10);

        public PrinterModel PrinterModel { get; set; }

        List<EscCode> _extraResetCodes;

        public PrinterDialect(PrinterModel model)
        {
            PrinterModel = model;
            _extraResetCodes = new List<EscCode>() { new EscCode(27, (byte)'@') };
        }

        public PrinterDialect(PrinterModel model, params EscCode[] extraResetCodes) : this(model)
        {
            if (extraResetCodes is null) {
                throw new ArgumentNullException(nameof(extraResetCodes));
            }

            _extraResetCodes.AddRange(extraResetCodes);
        }

        public virtual EscCodePair FontStyle(FontStyle style) { return null; }
        public virtual List<EscCode> Reset => _extraResetCodes;

        public virtual EscCode FormFeed => _formFeed;
        public virtual EscCode LineFeed => _lineFeed;
        public virtual EscCode CarriageReturn => _cr;
        public virtual EscCode NewLine => _newLine;

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

        public virtual EscCode FontType(FontType type)
        {
            return null;
        }

    }


    public class ESCPDialect : PrinterDialect
    {
        static readonly EscCodePair _fontStyleEmphasized = new EscCodePair(new EscCode(27, 69), new EscCode(27, 70));
        static readonly EscCodePair _fontStyleUnderline = new EscCodePair(new EscCode(27, 45, 49), new EscCode(27, 45, 48));
        static readonly EscCodePair _fontStyleUnderlineDouble = new EscCodePair(new EscCode(27, 45, 2), new EscCode(27, 45, 0));
        static readonly EscCodePair _fontStyleItalic = new EscCodePair(new EscCode(27, 52), new EscCode(27, 53));

        static readonly EscCodePair _printStylePica = new EscCodePair(new EscCode(27, 80), null);
        static readonly EscCodePair _printStyleElite = new EscCodePair(new EscCode(27, 77), null);
        static readonly EscCodePair _printStyleCondensed = new EscCodePair(new EscCode(15), new EscCode(18));

        public ESCPDialect(PrinterModel model) : base(model)
        {
        }
        public ESCPDialect(PrinterModel model, params EscCode[] extraResetCodes) : base(model, extraResetCodes) { }


        public override EscCodePair FontStyle(FontStyle style)
        {
            switch (style) {
                case mReporterLib.FontStyle.Emphasized: return _fontStyleEmphasized;
                case mReporterLib.FontStyle.Underline: return _fontStyleUnderline;
                case mReporterLib.FontStyle.UnderlineDouble: return _fontStyleUnderlineDouble;
                case mReporterLib.FontStyle.Italic: return _fontStyleItalic;
                default: break;
            }
            return null;
        }

        public override EscCodePair PrintStyle(PrintStyle style)
        {
            switch (style) {
                case mReporterLib.PrintStyle.Pica: return _printStylePica;
                case mReporterLib.PrintStyle.Elite: return _printStyleElite;
                case mReporterLib.PrintStyle.Condensed: return _printStyleCondensed;
                default: break;
            }
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
                default: break;
            }
            return null;
        }

        public override EscCode FontType(FontType type)
        {
            return new EscCode(27, 77, (byte)type);
        }

        static readonly EscCodePair _printStyleDoubleHeight = new EscCodePair(new EscCode(29, 33, 1), new EscCode(29, 33, 0));
        static readonly EscCodePair _printStyleDoubleWidth = new EscCodePair(new EscCode(29, 33, 16), new EscCode(29, 33, 0));

        public override EscCodePair PrintStyle(PrintStyle style)
        {
            EscCodePair result = null;
            if (style.HasFlag(mReporterLib.PrintStyle.DoubleHeight)) {
                result = _printStyleDoubleHeight;
            }
            if (style.HasFlag(mReporterLib.PrintStyle.DoubleWidth)) {
                if (result == null) result = _printStyleDoubleWidth;
                else {
                    result = new EscCodePair(new EscCode(29, 33, 17), new EscCode(29, 33, 0));
                }
            }
            return result;
        }


    }

    public class StarLineDialect : PrinterDialect
    {
        static readonly EscCodePair _fontStyleEmphasized = new EscCodePair(new EscCode(27, 69), new EscCode(27, 70));
        static readonly EscCodePair _fontStyleUnderline = new EscCodePair(new EscCode(27, 45, 1), new EscCode(27, 45, 0));
        static readonly EscCodePair _fontStyleUpperline = new EscCodePair(new EscCode(27, 95, 1), new EscCode(27, 95, 0));
        static readonly EscCodePair _fontStyleInverse = new EscCodePair(new EscCode(27, 52), new EscCode(27, 53));

        static readonly EscCode _alignLeft = new EscCode(27, 29, 97, 0);
        static readonly EscCode _alignRight = new EscCode(27, 29, 97, 2);
        static readonly EscCode _alignCenter = new EscCode(27, 29, 97, 1);

        static readonly EscCodePair _printStylePitch12 = new EscCodePair(new EscCode(27, 77), new EscCode(27, 77));
        static readonly EscCodePair _printStylePitch15 = new EscCodePair(new EscCode(27, 80), new EscCode(27, 77));
        static readonly EscCodePair _printStylePitch16 = new EscCodePair(new EscCode(27, 58), new EscCode(27, 77));
        static readonly EscCodePair _printStyleDoubleHeight = new EscCodePair(new EscCode(27, 104, 1), new EscCode(27, 104, 0));
        static readonly EscCodePair _printStyleDoubleWidth = new EscCodePair(new EscCode(27, 87, 1), new EscCode(27, 87, 0));

        public StarLineDialect() : base(PrinterModel.StarLineT) { }

        public StarLineDialect(PrinterModel model) : base(model)
        {
        }

        public override EscCode FontType(FontType type)
        {
            return new EscCode(27, 30, 70, (byte)type);
        }

        public override EscCodePair FontStyle(FontStyle style)
        {
            switch (style) {
                case mReporterLib.FontStyle.Emphasized: return _fontStyleEmphasized;
                case mReporterLib.FontStyle.Underline: return _fontStyleUnderline;
                case mReporterLib.FontStyle.Upperline: return _fontStyleUpperline;
                case mReporterLib.FontStyle.Inverse: return _fontStyleInverse;
                default: return null;
            }
        }

        public override EscCode Align(Align style)
        {
            switch (style) {
                default: return null;
                case mReporterLib.Align.Left: return _alignLeft;
                case mReporterLib.Align.Right: return _alignRight;
                case mReporterLib.Align.Center: return _alignCenter;
            }
        }

        public override EscCodePair PrintStyle(PrintStyle style)
        {
            switch (style) {
                case mReporterLib.PrintStyle.Pitch12: return _printStylePitch12;
                case mReporterLib.PrintStyle.Pitch15: return _printStylePitch15;
                case mReporterLib.PrintStyle.Pitch16: return _printStylePitch16;
                case mReporterLib.PrintStyle.DoubleHeight: return _printStyleDoubleHeight;
                case mReporterLib.PrintStyle.DoubleWidth: return _printStyleDoubleWidth;
                default: break;
            }
            return null;
        }

    }

}

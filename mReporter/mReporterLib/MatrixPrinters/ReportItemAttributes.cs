using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{

    public enum Align { AsBefore, Left, Right, Center, Justify }

    public enum FontStyle { Normal, Emphasized, Underline, UnderlineDouble, Upperline, Italic, Inverse }

    public enum PrintStyle
    {
        /// <summary>
        /// Do not set line print style, leave setting from lines before
        /// </summary>
        AsBefore, Pica, Elite, Condensed, Pitch12, Pitch15, Pitch16, DoubleHeight, DoubleWidth
    }

    public enum FontType : byte
    {
        A = 0, B = 1, C = 2, OCR = 16
    }

}

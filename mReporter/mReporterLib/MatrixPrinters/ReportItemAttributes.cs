using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{

    public enum Align { AsBefore, Left, Right, Center, Justify }

    public enum FontStyle { Normal, Emphasized, Underline, UnderlineDouble, Upperline, Italic, Inverse }

    [Flags]
    public enum PrintStyle : int
    {
        /// <summary>
        /// Do not set line print style, leave setting from lines before
        /// </summary>
        AsBefore = 0,
        Pica = 1, Elite = 2, Condensed = 4, Pitch12 = 8, Pitch15 = 16, Pitch16 = 32, DoubleHeight = 64, DoubleWidth = 128
    }

    public enum FontType : byte
    {
        A = 0, B = 1, C = 2, OCR = 16
    }

}

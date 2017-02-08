using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib.LabelPrinters
{

    /// <summary>
    /// Type of media in printer
    /// </summary>
    public enum MediaType
    {
        /// <summary>
        /// Media in printer are separated labels
        /// </summary>
        Label,
        /// <summary>
        /// Continous paper
        /// </summary>
        Continuous
    }

    /// <summary>
    /// Target output printer language
    /// </summary>
    public enum PrinterLanguage
    {
        /// <summary>
        /// Eltron Printer Language 2
        /// </summary>
        EPL2,
        /// <summary>
        /// Zebra Printer Language 2
        /// </summary>
        ZPL2
    }

    public enum TextFont
    {
        Bitmap1, Bitmap2, Bitmap3, Bitmap4, Bitmap5
    }

    public enum TextRotation
    {
        Normal, Rotate90, Rotate180, Rotate270
    }

}

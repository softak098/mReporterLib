using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib.MatrixPrinters
{

    public class PrinterCapability
    {
        public int Dpi { get; set; }
        public int PrintWidth { get; set; }
        public int PrintWidthInDots => (int)(PrintWidth / 2.54 * Dpi);


    }

}

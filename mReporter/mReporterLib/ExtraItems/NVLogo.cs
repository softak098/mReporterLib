using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{
    /// <summary>
    /// Special item to print logo stored in POS printer memory
    /// </summary>
    public class NVLogo : ReportItem
    {

        public NVLogo() : base(ReportItemType.UserDefined)
        {
        }

        public override OutputLine Render(RenderContext context)
        {
            var data = new List<string>();
            data.Add("\x1Cp\x01\x00");
            return context.AddToOutput(this, data);
        }
    }

}

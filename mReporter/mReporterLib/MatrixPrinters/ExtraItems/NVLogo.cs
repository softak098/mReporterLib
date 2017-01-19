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
        public Align LogoAlign { get; set; }


        public NVLogo() : base(ReportItemType.UserDefined)
        {
            this.LogoAlign = Align.Center;
        }

        public override OutputLine Render(RenderContext context)
        {
            var data = new List<string>();
            data.Add(context.Report.Dialect.Align(this.LogoAlign).Apply("\x1Cp\x01\x00"));

            var line = context.AddToOutput(this, data);
            line.AppendNewLine = false;

            return line;
        }
    }

}

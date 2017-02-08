using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{
    /// <summary>
    /// Controls line spacing for next items
    /// </summary>
    public class LineSpacing : ReportItem
    {
        /// <summary>
        /// 0 = default
        /// </summary>
        public byte LineSpace;

        public LineSpacing(byte lineSpace) : base(ReportItemType.UserDefined)
        {
            this.LineSpace = lineSpace;
        }

        public override OutputLine Render(RenderContext context)
        {
            OutputLine l;
            if (LineSpace == 0) {

                l = context.AddToOutput(this, EscCode.CreateCode(27, 50));
            }
            else {
                l = context.AddToOutput(this, EscCode.CreateCode(27, 51, LineSpace));

            }
            l.AppendNewLine = false;
            return l;
        }


    }

}

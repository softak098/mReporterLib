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
        /// NULL = default
        /// </summary>
        public byte? LineSpace;

        public LineSpacing(byte? lineSpace) : base(ReportItemType.UserDefined)
        {
            this.LineSpace = lineSpace;
        }

        public override OutputLine Render(RenderContext context)
        {
            OutputLine l;
            if (LineSpace == null) {

                l = context.AddToOutput(this, EscCode.CreateCode(27, 50));
            }
            else {
                l = context.AddToOutput(this, EscCode.CreateCode(27, 51, LineSpace.Value));

            }
            l.AppendNewLine = false;
            return l;
        }


    }

}

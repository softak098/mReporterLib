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

        public override void Render(RenderContext context)
        {
            if (LineSpace == null) {
                context.AddToOutput(this, new EscCode(27, 50));
            }
            else {
                context.AddToOutput(this,new EscCode(27, 51, LineSpace.Value));
            }
        }


    }

}

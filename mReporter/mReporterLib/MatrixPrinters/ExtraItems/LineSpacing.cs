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
            /*
            OutputLine l = context.CreateOutputLine(this);
            if (LineSpace == null) {

                l.Append(EscCode.CreateCode(27, 50));
            }
            else {
                l.Append(EscCode.CreateCode(27, 51, LineSpace.Value));

            }
            l.AppendNewLine = false;

            context.AddToOutput(l);
            */
        }


    }

}

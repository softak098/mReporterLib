﻿using System;
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

                l = context.AddToOutput(this, RenderContext.CreateCode(27, 50).ToString());
            }
            else {
                l = context.AddToOutput(this, RenderContext.CreateCode(27, 51, LineSpace).ToString());

            }
            l.AppendNewLine = false;
            return l;
        }


    }

}

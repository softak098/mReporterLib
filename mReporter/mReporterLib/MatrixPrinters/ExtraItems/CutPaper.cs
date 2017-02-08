using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{
    public class CutPaper : ReportItem
    {
        public CutPaper() : base(ReportItemType.UserDefined)
        { }

        public override OutputLine Render(RenderContext context)
        {
            var l=context.AddToOutput(this, EscCode.CreateCode(29, 86, 0));
            l.AppendNewLine = false;
            return l;
        }
    }
}

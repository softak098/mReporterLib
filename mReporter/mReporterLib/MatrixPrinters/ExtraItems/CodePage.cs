using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{

    public class CodePage : ReportItem
    {
        int _codePage = 0;

        public CodePage(int codePage) : base(ReportItemType.UserDefined) {
            _codePage = codePage;
        }

        public override OutputLine Render(RenderContext context)
        {
            var line = context.AddToOutput(this, EscCode.CreateCode(27, 116, _codePage));
            line.AppendNewLine = false;
            return line;
        }
    }

}

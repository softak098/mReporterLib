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

        public override void Render(RenderContext context)
        {
            /*
            var line = context.CreateOutputLine(this);
            line.Append(EscCode.CreateCode(27, 116, _codePage));
            line.AppendNewLine = false;

            context.AddToOutput(line);
            */
        }
    }

}

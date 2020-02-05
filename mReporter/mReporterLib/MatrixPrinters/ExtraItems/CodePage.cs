using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{

    public class CodePage : ReportItem
    {
        byte _codePage = 0;

        public CodePage(byte codePage) : base(ReportItemType.UserDefined)
        {
            _codePage = codePage;
        }

        public override void Render(RenderContext context)
        {
            if (context.Report.Dialect is StarLineDialect) {

                context.AddToOutput(this, new EscCode(27, 29, 116, _codePage));

            }
            else {
                if (context.Report.Dialect.PrinterModel == PrinterModel.EpsonGeneric) {
                    context.AddToOutput(this, new EscCode(27, 82, _codePage));

                }
                else {
                    context.AddToOutput(this, new EscCode(27, 116, _codePage));
                }

            }
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{

    public enum CutPaperMode : byte
    {
        Full = 0, Partial = 1, FeedAndFull = 2, FeedAndPartial = 3
    }

    public class CutPaper : ReportItem
    {
        CutPaperMode _mode;

        public CutPaper(CutPaperMode mode) : base(ReportItemType.UserDefined)
        {
            _mode = mode;
        }

        public override void Render(RenderContext context)
        {
            if (context.Report.Dialect is StarLineDialect) {

                context.AddToOutput(this, new EscCode(27, 100, (byte)_mode));

            }
            else {
                context.AddToOutput(this, new EscCode(29, 86, (byte)_mode));
            }
        }
    }
}

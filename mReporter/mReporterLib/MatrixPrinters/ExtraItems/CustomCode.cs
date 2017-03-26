using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{

    public class CustomCode : ReportItem
    {
        EscCode[] _codes;

        public CustomCode(params EscCode[] codes) : base(ReportItemType.UserDefined)
        {
            _codes = codes;
        }

        public override void Render(RenderContext context)
        {
            foreach (var item in _codes) {
                context.AddToOutput(this, item);
            }
        }

    }
}

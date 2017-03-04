using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{

    public class Image : ReportItem
    {

        public Image(string fileName) : base(ReportItemType.UserDefined)
        {

        }

        public override void Render(RenderContext context)
        {
            throw new NotImplementedException();
        }

    }

}

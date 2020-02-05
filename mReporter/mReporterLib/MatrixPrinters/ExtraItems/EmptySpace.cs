using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{

    public enum EmptySpaceType
    {
        Line, Dot
    }

    public class EmptySpace : ReportItem
    {
        EmptySpaceType _spaceType;
        byte _space;

        public EmptySpace(EmptySpaceType spaceType, byte space) : base(ReportItemType.UserDefined)
        {
            _spaceType = spaceType;
            _space = space;
        }

        public override void Render(RenderContext context)
        {
            if (context.Report.PageHeight > 0)
                throw new ArgumentOutOfRangeException(nameof(context.Report.PageHeight), "EmptySpace item can not be used for paged report.");

            if (context.Report.Dialect is StarLineDialect) {

                if (_spaceType == EmptySpaceType.Line)
                    context.AddToOutput(this, new EscCode(27, 97, _space));

                else context.AddToOutput(this, new EscCode(27, 73, _space));

            }
            else {
                if (_spaceType == EmptySpaceType.Line) 
                    context.AddToOutput(this, new EscCode(27, 100, _space));

                else context.AddToOutput(this, new EscCode(27, 74, _space));
            }
        }

    }

}

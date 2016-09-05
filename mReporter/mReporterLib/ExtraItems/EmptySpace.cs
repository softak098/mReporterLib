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

    public class EmptySpace: ReportItem
    {
        EmptySpaceType _spaceType;
        byte _space;

        public EmptySpace(EmptySpaceType spaceType,byte space): base(ReportItemType.UserDefined)
        {
            _spaceType = spaceType;
            _space = space;
        }

        public override OutputLine Render(RenderContext context)
        {
            string r;

            if (_spaceType == EmptySpaceType.Line) {
                r = new string(RenderContext.CreateCode(27, 100, _space));
            }
            else {
                r = new string(RenderContext.CreateCode(27, 74, _space));
            }

            var line = context.AddToOutput(this, r);
            line.AppendNewLine = false;

            return line;
        }
    }

}

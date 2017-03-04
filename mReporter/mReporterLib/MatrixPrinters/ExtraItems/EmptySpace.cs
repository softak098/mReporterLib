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

        public override void Render(RenderContext context)
        {
            /*
            EscCode r;

            if (_spaceType == EmptySpaceType.Line) {
                r = EscCode.CreateCode(27, 100, _space);
            }
            else {
                r = EscCode.CreateCode(27, 74, _space);
            }

            var line = context.CreateOutputLine(this);
            line.AppendNewLine = false;
            line.Append(r);

            context.AddToOutput(line);
            */
        }

    }

}

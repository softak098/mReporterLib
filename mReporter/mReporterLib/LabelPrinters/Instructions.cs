using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib.LabelPrinters
{

    class Instruction
    {

    }

    class PositionInstruction : Instruction
    {
        LabelPosition _position;

        public PositionInstruction(LabelPosition position)
        {
            _position = position;
        }

    }

    class TextInstruction : Instruction
    {



    }



}

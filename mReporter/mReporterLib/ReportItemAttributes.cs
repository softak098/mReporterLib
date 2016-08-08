﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{

    public enum Align { AsBefore, Left, Right, Center, Justify }

    public enum FontStyle { Normal, Emphasized, Underline, UnderlineDouble, Italic, Inverse }

    public enum PrintStyle {
        /// <summary>
        /// Do not set line print style, leave setting from lines before
        /// </summary>
        AsBefore, Pica, Elite, Condensed }

}

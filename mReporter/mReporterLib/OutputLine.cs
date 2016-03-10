﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{
    public class OutputLine
    {
        List<string> _data;
        internal List<string> GeneratedLines
        {
            get
            { return _data; }
        }

        public OutputLine Parent { get;private set; }

        public ReportItem SourceReportItem { get; set; }

        public OutputLine(OutputLine parent)
        {
            this.Parent = parent;
        }

        public OutputLine(OutputLine parent, ReportItem item, List<string> data):this(parent)
        {
            SourceReportItem = item;

            _data = data;

        }

    }

}
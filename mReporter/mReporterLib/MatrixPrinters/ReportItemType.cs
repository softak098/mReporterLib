using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{
    public enum ReportItemType
    {
        ReportHeader, PageHeader, Header, Detail, Footer, PageFooter, ReportFooter, Group,
        /// <summary>
        /// Mark this item has no functional relation to library itself
        /// </summary>
        UserDefined
    }

}

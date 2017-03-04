using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace mReporterLib
{

    public interface IWriteToStream
    {
        void WriteTo(Stream stream, Encoding textEncoding);
    }

    public abstract class OutputElement : IWriteToStream
    {
        internal OutputElement Parent { get; set; }
        internal ReportItem SourceReportItem { get; set; }

        /// <summary>
        /// Specifies how many lines this element occupies
        /// </summary>
        internal virtual int LineCount => 0;

        public virtual void WriteTo(Stream stream, Encoding textEncoding)
        {
            throw new NotImplementedException();
        }
    }



}

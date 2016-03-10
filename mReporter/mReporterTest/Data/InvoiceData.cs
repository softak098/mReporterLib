using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mReporterTest.Data
{
    class InvoiceData
    {
        public InvoiceHeaderData Header { get; set; }
        public IEnumerable<InvoiceDetail> Lines { get; set; }

        internal void CreateData()
        {
            Header = new InvoiceHeaderData();
            Header.CreateData();


        }
    }

    class InvoiceDetail
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IEnumerable<Batch> Batches { get; set; }
    }

    class Batch
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public float Quantity { get; set; }
        public string Unit { get; set; }
    }

}

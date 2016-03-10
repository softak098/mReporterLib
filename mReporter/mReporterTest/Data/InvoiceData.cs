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
        public List<InvoiceDetail> Lines { get; set; }

        internal void CreateData()
        {
            Header = new InvoiceHeaderData();
            Header.CreateData();

            Lines = new List<InvoiceDetail>();
            for (int i = 0; i < 10; i++) {
                InvoiceDetail id = new InvoiceDetail();
                id.CreateData(i);
                Lines.Add(id);
            }
        }
    }

    class InvoiceDetail
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Quantity { get; set; }
        public string Unit { get; set; }
        public List<Batch> Batches { get; set; }

        internal void CreateData(int i)
        {
            Name = "Product " + i;
            Code = "X2E" + i;
            Description = "this is a description of product " + i;
            Quantity = i * 1.87;
            Unit = "m";

            Batches = new List<Batch>();
            for (int j = 0; j < 5; j++) {
                var b = new Batch() {
                    Name=(i*66724531).ToString(),
                    Code=i.ToString(),
                     Quantity=i*1.32,
                     Unit="#"
                };
                Batches.Add(b);

            }
        }
    }

    class Batch
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public double Quantity { get; set; }
        public string Unit { get; set; }
    }

}

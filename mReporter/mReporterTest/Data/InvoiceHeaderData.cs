using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mReporterTest.Data
{

    class InvoiceHeaderData
    {
        public Address Customer { get; set; }

        public Address DeliveryAddress { get; set; }

        public Address Supplier { get; set; }

        internal void CreateData()
        {
            Supplier = new Address();
            Supplier.CreateData();

        }
    }

}

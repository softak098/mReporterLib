using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mReporterTest.Data
{
    class Address
    {
        public string CompanyName { get; set; }
        public string Street { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        public override string ToString()
        {
            return string.Format("{0}\n{1}\n{2} {3}\n{4}", CompanyName, Street, ZipCode, City, Country);
        }

        internal void CreateData()
        {
            CompanyName = "Claus&Co";
            Street = "Polar 21";
            ZipCode = "100 00";
            City = "North Pole";
            Country = "North Sea";
        }
    }
}

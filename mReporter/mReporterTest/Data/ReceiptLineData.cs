﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mReporterTest.Data
{
    class ReceiptLineData
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }

        public override string ToString()
        {
            return string.Format("Product={0}, Price={1}", ProductName, Price);
        }

        internal static IEnumerable<ReceiptLineData> CreateData()
        {
            List<ReceiptLineData> result = new List<ReceiptLineData>();

            for (int i = 0; i < 5; i++) {

                result.Add(new ReceiptLineData {
                    ProductName = "Bread " + i,
                    Quantity = i+3,
                    Price = i * 13.13
                });


            }

            return result;
        }
    }
}

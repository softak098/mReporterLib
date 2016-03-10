using Microsoft.VisualStudio.TestTools.UnitTesting;
using mReporterLib;
using mReporterTest.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mReporterLib.Tests
{
    [TestClass()]
    public class ReportTests
    {
        [TestMethod()]
        public void ReportTest()
        {

            Report rpt = new Report(new ESCPosDialect());
            rpt.PageHeight = 0;

            rpt.AddItem(new NVLogo());

            rpt.AddItem(new Line(ReportItemType.ReportHeader) {

                Template = "Report Header [1]"

            });

            int sumQuantity = 0;
            double sumPrice = 0;

            var masterDetailGroup = new Group<ReceiptLineData>();
            masterDetailGroup.DataSource = ReceiptLineData.CreateData();

            masterDetailGroup.AddItem(new Line(ReportItemType.Header) {
                Style= FontStyle.Inverse,
                Template = "Product              Q         P"

            });

            masterDetailGroup.AddItem(new Line(ReportItemType.Detail) {

                Template = "__________________ ___ _________",
                GetData = (e) => {
                    var d = e.Data as ReceiptLineData;

                    if (e.Index == 0) {
                        e.Result.Value = d.ProductName;
                    }
                    else if (e.Index == 1) {

                        e.Result.Value = d.Quantity.ToString();
                        e.Result.Alignment = Align.Right;

                        sumQuantity += d.Quantity;

                    }
                    else if (e.Index == 2) {
                        e.Result.Value = d.Price.ToString("N2");
                        e.Result.Alignment = Align.Right;

                        sumPrice += d.Price;
                    }
                }
            });

            masterDetailGroup.AddItem(new Line(ReportItemType.Footer) {

                Template = "--------------------------------"

            });

            // add detail group to report
            rpt.AddItem(masterDetailGroup);
            // add footer with summary
            rpt.AddItem(new Line(ReportItemType.ReportFooter) {

                Template = "Total            : ___ _________\n\n",
                GetData = e=> {

                    if (e.Index == 0) e.Result.Value = sumQuantity.ToString();
                    else if (e.Index == 1) {
                        e.Result.Value = sumPrice.ToString("N2");
                        e.Result.Alignment = Align.Right;
                        e.Result.Style = FontStyle.Emhasized;
                    }

                }

            });

            // finaly render and build output
            var rContext = rpt.Render();
            var pBuilder = rpt.BuildPages(rContext);
            Console.WriteLine(pBuilder.Output);

            using (var fs = new FileStream(@"C:\TEMP\data.prn", FileMode.Create)) {
                using (var sw = new StreamWriter(fs, Encoding.ASCII)) {
                    sw.Write(pBuilder.Output);
                    sw.Close();
                }
            }

            //var printer = new SerialPrinter(pBuilder.Output, Encoding.ASCII);
            //printer.Print("COM9");


        }

        [TestMethod()]
        public void RenderTest()
        {

        }

        [TestMethod()]
        public void BuildPagesTest()
        {

        }
    }
}
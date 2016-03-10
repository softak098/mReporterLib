using Microsoft.VisualStudio.TestTools.UnitTesting;
using mReporterLib;
using mReporterTest.Data;
using System;
using System.Collections.Generic;
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

            Report rpt = new Report();
            rpt.PageHeight = 0;

            rpt.AddItem(new Line(ReportItemType.ReportHeader) {

                Template = "Report Header [1]"

            });

            rpt.AddItem(new Line(ReportItemType.PageHeader) {

                Template = "Page header on all pages.... page $P of $T"

            });

            rpt.AddItem(new Line(ReportItemType.PageFooter) {

                Template = "\nPage foooooooooter on all pages.... page $P of $T"

            });


            var masterDetailGroup = new Group<ReceiptLineData>();
            masterDetailGroup.DataSource = ReceiptLineData.CreateData();

            masterDetailGroup.AddItem(new Line(ReportItemType.Header) {

                Template = "Product           Q         Price"

            });

            masterDetailGroup.AddItem(new Line(ReportItemType.Detail) {

                Template = "_______________ ___ _____",
                GetData = (e) => {
                    var d = e.Data as ReceiptLineData;

                    if (e.Index == 0) {
                        e.Result.Value = d.ProductName;
                    }
                    else if (e.Index == 1) {

                        e.Result.Value = d.Quantity.ToString();
                        e.Result.Alignment = Align.Right;

                    }
                    else if (e.Index == 2) {
                        e.Result.Value = d.Price.ToString("N2");
                        e.Result.Alignment = Align.Right;

                    }
                }
            });

            masterDetailGroup.AddItem(new Line(ReportItemType.Footer) {

                Template = "-------------------------------"

            });

            // add detail group to report
            rpt.AddItem(masterDetailGroup);
            // add footer with summary
            rpt.AddItem(new Line(ReportItemType.ReportFooter) {

                Template = "Report footer is rendered once on last report page [2]"

            });

            // finaly render and build output
            var rContext = rpt.Render();
            var pBuilder = rpt.BuildPages(rContext);
            Console.WriteLine(pBuilder.Output);

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
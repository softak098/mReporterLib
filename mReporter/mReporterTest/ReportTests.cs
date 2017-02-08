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

            Report rpt = new Report(new ESCPosDialect(PrinterModel.EpsonPosGeneric));
            rpt.PageHeight = 0;

            //rpt.AddItem(new NVLogo());

            rpt.AddItem(new Line(ReportItemType.ReportHeader) {

                Template = "Hlavička stránky [1]",
                Alignment = Align.Left

            });


            int sumQuantity = 0;
            double sumPrice = 0;

            var masterDetailGroup = new Group<ReceiptLineData>();
            masterDetailGroup.DataSource = ReceiptLineData.CreateData();

            masterDetailGroup.AddItem(new Line(ReportItemType.Header) {
                Style = FontStyle.Inverse,
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

                Template = "Total            : ___ _________",
                GetData = e => {

                    if (e.Index == 0) e.Result.Value = sumQuantity.ToString();
                    else if (e.Index == 1) {
                        e.Result.Value = sumPrice.ToString("N2");
                        e.Result.Alignment = Align.Right;
                        e.Result.Style = FontStyle.Emphasized;
                    }

                }

            });

            // barcode line test
            var barcodeItem = new Barcode() {
                BarcodeType = BarcodeType.ITF,
                Data= "43657621",
            };
            //rpt.AddItem(barcodeItem);

            rpt.AddItem(new EmptySpace(EmptySpaceType.Dot, 15));

            //rpt.AddItem(new QRCode() { Data = "www.hradboskovice.cz" });


            rpt.AddItem(new Line(ReportItemType.Footer) {
                 Alignment= Align.Center,
                 RepeatStaticItems=true,
                  
                Template = "╔═══════════════════════╗\n| _____________________ \u2524",
                GetData = e => {

                    if (e.Index == 0) {
                        e.Result.Value = "Dekujeme a tesime se na Vasi dalsi navstevu.";
                        e.Result.WordWrap = true;
                        e.Result.Alignment = Align.Center;
                    }

                }

            });

            rpt.AddItem(new EmptySpace(EmptySpaceType.Line, 2));
            rpt.AddItem(new CutPaper());

            //rpt.AddItem(new Barcode() { BarcodeType = BarcodeType.EAN13, BarcodeData = "5032037076982" });
            //rpt.AddItem(new Barcode() { BarcodeType = BarcodeType.EAN8, BarcodeData = "5032370" });


            // finaly render and build output
            var rContext = rpt.Render();
            var pBuilder = rpt.BuildPages(rContext);

            using (var fs = new FileStream(@"C:\TEMP\data.prn", FileMode.Create)) {
                using (var sw = new StreamWriter(fs, Encoding.ASCII)) {
                    sw.Write(pBuilder.Output);
                    sw.Close();
                }
            }

            /*
            var printer = new SerialPrinter(pBuilder.Output, Encoding.ASCII);
            printer.Print("COM9");
            */

            byte[] data = Encoding.GetEncoding(852).GetBytes(pBuilder.Output);

            //RawPrinterHelper.SendToPrinter("POS-58", data);


            //byte[] printData = Encoding.ASCII.GetBytes(pBuilder.Output);
            //RawPrinterHelper.SendToPrinter(@"Star TSP600 Cutter (TSP643)", data);


        }

        [TestMethod()]
        public void ComplexReportTest()
        {
            var iData = new InvoiceData();
            iData.CreateData();


            Report rpt = new Report(new ESCPDialect(PrinterModel.EpsonGeneric));
            rpt.PageHeight = 66;

            rpt.AddItem(new Line(ReportItemType.ReportHeader) {
                PrintStyle = PrintStyle.Elite,
                Template = "+--------------------------------------+---------------------------------------+"
            });

            rpt.AddItem(new Line(ReportItemType.ReportHeader) {
                RepeatStaticItems = true,
                Template = "| ____________________________________ | _____________________________________ |",
                GetData = (e) => {

                    if (e.Index == 0) {

                        e.Result.Value = iData.Header.Supplier.ToString();
                        e.Result.Style = FontStyle.Emphasized;

                    }

                }
            });

            rpt.AddItem(new Line(ReportItemType.ReportHeader) {
                Template = "+--------------------------------------+---------------------------------------+"
            });

            rpt.AddItem(new Line(ReportItemType.PageHeader) {

                Template = @"_______________________________________________________________________________
_______________________________________________________________________________",
                GetData = (e) => {

                    if (e.Index == 0) {
                        e.Result.Value = "Document: " + iData.Header.DocumentNr;
                        e.Result.Alignment = Align.Right;
                    }
                    else {

                        e.Result.Value = "Page: $P / $T";
                        e.Result.Alignment = Align.Right;

                    }

                }

            });

            rpt.AddItem(new Line(ReportItemType.PageFooter) {

                Template = "\n--------------------------------------------------------------------------------\n(c) 2016 Our Company name"

            });


            var detailGroup = new Group<InvoiceDetail>();
            detailGroup.DataSource = iData.Lines;

            detailGroup.AddItem(new Line(ReportItemType.Header) {
                PrintStyle= PrintStyle.Pica,
                RepeatOnNewPage = true,
                Template = "--------------------------------------------------------------------------------"
            });
            detailGroup.AddItem(new Line(ReportItemType.Header) {
                RepeatOnNewPage = true,
                Template = "Product                                                           Quantity  Unit"
            });
            detailGroup.AddItem(new Line(ReportItemType.Header) {
                RepeatOnNewPage = true,
                Template = "--------------------------------------------------------------------------------"
            });

            Line detailGroupDetail;
            detailGroup.AddItem(detailGroupDetail = new Line(ReportItemType.Detail) {
                PageBreakInside=false,
                Template = "_______________________________________________________________ ___________ ____",
                GetData = (e) => {
                    var d = e.Data as InvoiceDetail;

                    if (e.Index == 0) {
                        e.Result.WordWrap = true;
                        e.Result.Value = @"Kupující má právo požádat prodávajícího o zpětný prodej vratných obalů (palety, rámy apod.) a prodávající je tak povinen za stejnou cenu vratné obaly koupit od kupujícího zpět za podmínky, že příslušné obaly budou vráceny prodávajícímu v neporušeném stavu  do 21 dnů od jejich původního obdržení kupujícím od prodávajícího.  
Převzetí dopravcem:  OP/ŘP:                                Podpis:


Převzetí odběratelem: Převzaté zboží odpovídá množství, kvalitě i ostatním podmínkám uvedeným v kupní smlouvě.
Jméno:                               Podpis:                                    Datum:
          



";
                        
                        
                        //string.Concat(d.Code, " ", d.Name, "\n", d.Description, "\n", d.Description, "\n", d.Description);
                    }
                    else if (e.Index == 1) {

                        e.Result.Value = d.Quantity.ToString("N2");
                        e.Result.Alignment = Align.Right;

                    }
                    else if (e.Index == 2) {
                        e.Result.Value = d.Unit;
                    }
                }
            });

            detailGroup.AddItem(new Line(ReportItemType.Detail) {
                Template = "" // empty line between other detail lines
            });

            var batchGroup = new Group<Batch>();
            batchGroup.GetDataSource = () => {

                var data = batchGroup.GetParentData();
                if (data != null) {

                    return (data as InvoiceDetail).Batches;

                }
                return null;
            };

            batchGroup.AddItem(new Line(ReportItemType.Detail) {

                Template = "Batch: ______________________________________ _________ ____",
                GetData = (e) => {
                    var d = e.Data as Batch;

                    if (e.Index == 0) {
                        e.Result.Value = string.Concat(d.Name, " (", d.Code, ")");
                    }
                    else if (e.Index == 1) {

                        e.Result.Value = d.Quantity.ToString("N2");
                        e.Result.Alignment = Align.Right;

                    }
                    else if (e.Index == 2) {
                        e.Result.Value = d.Unit;
                    }
                }
            });
            // register subdetail as child item of master detail
            detailGroupDetail.AddItem(batchGroup);

            // add detail group to report
            rpt.AddItem(detailGroup);

            // finaly render and build output
            var rContext = rpt.Render();
            var pBuilder = rpt.BuildPages(rContext);

            using (var fs = new FileStream(@"C:\TEMP\data2.prn", FileMode.Create)) {
                using (var sw = new StreamWriter(fs, Encoding.ASCII)) {
                    sw.Write(pBuilder.Output);
                    sw.Close();
                }
            }

            /*
            byte[] printData = Encoding.ASCII.GetBytes(pBuilder.Output);
            RawPrinterHelper.SendToPrinter(@"Star TSP600 Cutter (TSP643)", printData);
            */

        }

        [TestMethod()]
        public void EnumeratePrinters()
        {

            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters) {
                Console.WriteLine(printer);
            }

        }

    }
}
﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using mReporterLib;
using mReporterTest.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mReporterTest
{
    [TestClass()]
    public class ReportTests
    {

        [TestMethod()]
        public void EetRevenueTest()
        {

            Report rpt = new Report(new ESCPosDialect(PrinterModel.EpsonPosGeneric)) {
                PageHeight = 0
            };

            rpt.AddItem(new Line(ReportItemType.ReportFooter) {

                Template = "BKP: ________________________________________",
                GetData = e => {

                    e.Result.Value = "F10835FD-664E9E38-07111A35-FE053C13-E5C63B25";
                    e.Result.WordWrap = true;
                }

            });

            rpt.AddItem(new Line(ReportItemType.ReportFooter) {

                Template = "PKP: ________________________________________",
                GetData = e => {

                    e.Result.Value = "BPNsyN3auTjXF8KAlxYvwnLBW8z9WQil8Ur4dY11jUH+54MW+bd6gm5Lpwlt+b0hrctUVnNMhcPBhYXG21v9pahhS7K9eLcrCq+kUL9JowKjxRyBELTcAebuOusc1PAoTbndJBQBvGlpdIbz1oQIk3H2ARoxHCJzIwrpKmD8EVrkzOohWdA/HSD9GGleBRuQxvX0zzCoupYP0fos+DJk7abxOyZxL1fl0+XnhtBvO+BBeomXMANModOSZZXuPZlWegrTDAYtGtpDRBC5INj309GdzP+8PWKlsohKoL7cEi55Dz+Gytj0keyTi1w3028sEHistxH+ZZX+lbQ6+9aUhQ==";
                    e.Result.WordWrap = true;

                }

            });

            rpt.AddItem(new Line(ReportItemType.ReportFooter) {

                Template = "FIK: ________________________________________",
                GetData = e => {

                    e.Result.Value = "2dd0fa0c-ce5b-44ad-8222-a99d26a0b1b8-ff";
                    e.Result.WordWrap = true;

                }

            });

            /*
            rpt.AddItem(new FreeText("demo <u>underlined and<b>bold</b></u>" +
                "<codepage:20>demo11</codepage><barcode:ean13>123456789012</barcode>"));
                */


            rpt.AddItem(new CutPaper(CutPaperMode.FeedAndPartial));
            // finaly render and build output
            var rContext = rpt.Render();
            var pBuilder = rpt.BuildPages(rContext);



            using (var fs = new FileStream(@"C:\TEMP\dataEet.prn", FileMode.Create)) {

                pBuilder.OutputStream.WriteTo(fs);
            }

            /*
            var printer = new SerialPrinter(pBuilder.Output, Encoding.ASCII);
            printer.Print("COM9");
            */


            var data = pBuilder.OutputStream.ToArray();
            RawPrinterHelper.SendToPrinter(@"EPSON TM-T20II Receipt", data);
        }


        [TestMethod()]
        public void ReportTest()
        {

            Report rpt = new Report(new ESCPosDialect(PrinterModel.EpsonPosGeneric));
            rpt.PageHeight = 0;
            rpt.TextEncoding = Encoding.GetEncoding(852);

            rpt.AddItem(new CodePage(18));
            //rpt.AddItem(new LineSpacing(0));

            //rpt.AddItem(new NVLogo());
            rpt.AddItem(new NVLogo(1, NVLogoSize.Normal));

            rpt.AddItem(new Line(ReportItemType.ReportHeader) {

                Template = "Hlavička stránky [1]",
                Alignment = Align.Right

            });


            int sumQuantity = 0;
            double sumPrice = 0;

            var masterDetailGroup = new Group<ReceiptLineData>() {
                DataSource = ReceiptLineData.CreateData()
            };

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

            // add footer with summary
            masterDetailGroup.AddItem(new Line(ReportItemType.Footer) {

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

            // add detail group to report
            rpt.AddItem(masterDetailGroup);

            // barcode line test
            var barcodeItem = new Barcode() {
                BarcodeType = BarcodeType.ITF,
                Data = "43657621",
                //Width = 7,
                //HriPosition= BarcodeHriPosition.Above
            };
            rpt.AddItem(barcodeItem);

            //rpt.AddItem(new EmptySpace(EmptySpaceType.Dot, 15));

            /*
            //rpt.AddItem(new QRCode() { Data = "www.hradboskovice.cz" });
            rpt.AddItem(new Line(ReportItemType.Footer) {
                 Alignment= Align.Right,
                Template = "╔═══════════════════════╗"
            });

            rpt.AddItem(new Line(ReportItemType.Footer) {
                Alignment = Align.Right,
                RepeatStaticItems = true,

                Template = "| _____________________ \u2524",
                GetData = e => {

                    if (e.Index == 0) {
                        e.Result.Value = "Děkujeme a těšíme se na Vaši další návštěvu.";
                        e.Result.WordWrap = true;
                        e.Result.Alignment = Align.Center;
                    }

                }

            });
            rpt.AddItem(new Line(ReportItemType.Footer) {
                Alignment = Align.Right,
                Template = "-═══════════════════════-"
            });
            */

            rpt.AddItem(new Line(ReportItemType.Footer) {
                Template = "12345678901234567890",
            });
            rpt.AddItem(new Line(ReportItemType.Footer) {
                Template = "1234567890",
                PrintStyle = PrintStyle.DoubleWidth
            });
            rpt.AddItem(new Line(ReportItemType.Footer) {
                Template = "12345678901234567890",
                PrintStyle = PrintStyle.DoubleHeight
            });
            rpt.AddItem(new Line(ReportItemType.Footer) {
                Template = "DOUBLE-EVERY",
                PrintStyle = PrintStyle.DoubleHeight | PrintStyle.DoubleWidth
            });


            rpt.AddItem(new Barcode() { BarcodeType = BarcodeType.EAN13, Data = "5032037076982", HriPosition = BarcodeHriPosition.Bellow });
            rpt.AddItem(new Barcode() { BarcodeType = BarcodeType.EAN8, Data = "5032370" });

            rpt.AddItem(new EmptySpace(EmptySpaceType.Dot, 15));
            rpt.AddItem(new Barcode() {
                BarcodeType = BarcodeType.CODE128,
                Data = "{BAPK11-23",
                HriPosition = BarcodeHriPosition.AboveAndBellow,
                Width = 2
            });

            rpt.AddItem(new NVLogo(2, NVLogoSize.Normal) { LogoAlign = Align.Right });

            //rpt.AddItem(new Image(@"E:\IMAGES\_vyrd12_54tropico.jpg"));

            rpt.AddItem(new Line(ReportItemType.Footer) {
                Alignment = Align.Left,
                Template = "Back in text mode....",
            });

            //rpt.AddItem(new CustomCode(new EscCode(29,98,1,29, 33, 65), new EscCode((byte)'Q',10)));

            rpt.AddItem(new EmptySpace(EmptySpaceType.Line, 2));
            rpt.AddItem(new CutPaper(CutPaperMode.FeedAndPartial));

            // finaly render and build output
            var rContext = rpt.Render();
            var pBuilder = rpt.BuildPages(rContext);

            using (var fs = new FileStream(@"C:\TEMP\data_PK.prn", FileMode.Create)) {

                pBuilder.OutputStream.WriteTo(fs);

            }

            var data = pBuilder.OutputStream.ToArray();
            RawPrinterHelper.SendToPrinter(@"TM-T20", data);


            /*
            MobileLPR.LprJob job = new MobileLPR.LprJob();
            job.SetPrinterURI("lpr://10.255.11.19/lpt1");
            job.AddDataFile(data);

            job.SubmitJob();
            job.WaitForCompletion();
            */
            //LPDPrinterHelper helper = new LPDPrinterHelper("10.255.11.19", "lpd1");
            //helper.LPR(data);

            // usbprn:Star TSP600 Cutter (TSP643)
        }

        [TestMethod()]
        public void Image()
        {
            Report rpt = new Report(new StarLineDialect());
            rpt.PageHeight = 0;
            rpt.TextEncoding = Encoding.GetEncoding(852);

            rpt.AddItem(new CodePage(5));

            rpt.AddItem(new Image(@"E:\PROJEKTY\NLMIS\Allegro_Logo.png"));

            // finaly render and build output
            var rContext = rpt.Render();
            var pBuilder = rpt.BuildPages(rContext);

            using (var fs = new FileStream(@"C:\TEMP\data_image.prn", FileMode.Create)) {

                pBuilder.OutputStream.WriteTo(fs);

            }


            var data = pBuilder.OutputStream.ToArray();
            //RawPrinterHelper.SendToPrinter(@"Star TSP600 Cutter (TSP643)", data);
        }

        [TestMethod()]
        public void SendRaw()
        {
            /*

            MobileLPR.LprJob job = new MobileLPR.LprJob();
            job.SetPrinterURI("lpr://10.255.11.19/lpt1");
            //job.AddDataFile(@"C:\TEMP\SP600-LOGO.prn");

            job.SubmitJob();
            job.WaitForCompletion();
            */
        }


        [TestMethod()]
        public void ComplexReportTest()
        {
            var iData = new InvoiceData();
            iData.CreateData();


            Report rpt = new Report(new ESCPDialect(
                PrinterModel.EpsonGeneric,
                new EscCode(27, 116, 18) //, new EscCode(27, 120, 48)
                ));

            rpt.PageHeight = 65;
            rpt.TextEncoding = Encoding.GetEncoding(852);

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

                Template = "_______________________________________________________________________________\n_______________________________________________________________________________",
                GetData = (e) => {

                    if (e.Index == 0) {
                        e.Result.Value = "Document: " + iData.Header.DocumentNr;
                        e.Result.Alignment = Align.Right;
                        e.Result.Style = FontStyle.Emphasized;
                    }
                    else {

                        e.Result.Value = "Page: $P / $T";
                        e.Result.Alignment = Align.Right;

                    }

                }

            });

            rpt.AddItem(new Line(ReportItemType.PageFooter) {

                Template = "\r\n--------------------------------------------------------------------------------\r\n(c) 2020 Our Company name"

            });


            var detailGroup = new Group<InvoiceDetail>();
            detailGroup.DataSource = iData.Lines;

            detailGroup.AddItem(new Line(ReportItemType.Header) {
                PrintStyle = PrintStyle.Pica,
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
                PageBreakInside = false,
                Template = "_______________________________________________________________ __________  ____",
                GetData = (e) => {
                    var d = e.Data as InvoiceDetail;

                    if (e.Index == 0) {
                        e.Result.WordWrap = true;
                        //                        e.Result.Value = @"Kupující má právo požádat prodávajícího o zpětný prodej vratných obalů (palety, rámy apod.) a prodávající je tak povinen za stejnou cenu vratné obaly koupit od kupujícího zpět za podmínky, že příslušné obaly budou vráceny prodávajícímu v neporušeném stavu  do 21 dnů od jejich původního obdržení kupujícím od prodávajícího.  
                        //Převzetí dopravcem:  OP/ŘP:                                Podpis:


                        //Převzetí odběratelem: Převzaté zboží odpovídá množství, kvalitě i ostatním podmínkám uvedeným v kupní smlouvě.
                        //Jméno:                               Podpis:                                    Datum:




                        //";

                        e.Result.Value = string.Concat(d.Code, " ", d.Name, "\r\n", d.Description, "\r\n", d.Description, "\r\n", d.Description);
                    }
                    else if (e.Index == 1) {

                        e.Result.Value = d.Quantity.ToString("N2");
                        e.Result.Alignment = Align.Right;

                    }
                    else if (e.Index == 2) {
                        e.Result.Value = d.Unit;
                        //e.Result.Alignment = Align.Right;
                    }
                }
            });

            detailGroup.AddItem(new Line(ReportItemType.Detail) {
                Template = " " // empty line between other detail lines
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

                Template = "        Batch: ______________________________________ _________ ____",
                GetData = (e) => {
                    var d = e.Data as Batch;

                    if (e.Index == 0) {
                        e.Result.Value = string.Concat(d.Name, " (", d.Code, ")");
                        //e.Result.Value = new string('X', 100);
                        e.Result.WordWrap = true;
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

            //using (var fs = new FileStream(@"C:\TEMP\data_report.prn", FileMode.Create)) {

            //    pBuilder.OutputStream.WriteTo(fs);

            //}

            byte[] printData = pBuilder.OutputStream.ToArray();
            RawPrinterHelper.SendToPrinter(@"OKI MC562(PCL)", printData);
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
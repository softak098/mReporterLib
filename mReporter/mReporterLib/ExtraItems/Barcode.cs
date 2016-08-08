using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{

    public enum BarcodeType
    {
        UPC_A = 65, UPC_E = 66, EAN13 = 67, EAN8 = 68, CODE39 = 69, ITF = 70, CODABAR = 71, CODE93 = 72, CODE128 = 73
    }

    public enum BarcodeHriFont
    {
        A = 0, B = 1
    }

    public enum BarcodeHriPosition
    {
        DoNotPrint = 0, Above = 1, Bellow = 2, AboveAndBellow = 3
    }

    /// <summary>
    /// Produces barcode
    /// </summary>
    public class Barcode : ReportItem
    {
        public Align Alignment { get; set; }
        public BarcodeHriFont HriFont { get; set; }
        public BarcodeHriPosition HriPosition { get; set; }
        public int Height { get; set; }
        public BarcodeType BarcodeType { get; set; }
        /// <summary>
        /// This is only internal property for testing, do not use in production code
        /// </summary>
        public string BarcodeData { get { return _data; } set { this.SetData(value); } }

        public Barcode() : base(ReportItemType.UserDefined)
        {
            Alignment = Align.Center;
            HriFont = BarcodeHriFont.B;
            HriPosition = BarcodeHriPosition.DoNotPrint;
            Height = 50;
            BarcodeType = BarcodeType.CODE39;
        }

        public override OutputLine Render(RenderContext context)
        {
            if (_data == null) return null;

            StringBuilder sb = new StringBuilder();
            sb.Append(context.Report.Dialect.Align(this.Alignment).Start);

            if (HriPosition != BarcodeHriPosition.DoNotPrint) {
                sb.Append(context.CreateCode(29, 102, (int)HriFont));
                sb.Append(context.CreateCode(29, 72, (int)HriPosition));
            }
            // height of the code
            sb.Append(context.CreateCode(29, 104, Math.Min(Height, 255)));
            // and code itself
            sb.Append(context.CreateCode(29, 107,(int)BarcodeType,_data.Length));
            sb.Append(_data.ToCharArray());

            var line = context.AddToOutput(this, sb.ToString());
            line.AppendNewLine = false;

            return line;
        }

        string _data = null;
        public override void SetData(object data)
        {
            _data = null;
            string d = Convert.ToString(data);

            switch (BarcodeType) {
                case BarcodeType.UPC_A:
                    if (CheckIntegerInput(d, 11, 12)) _data = d;
                    break;
                case BarcodeType.UPC_E:
                    if (CheckIntegerInput(d, 11, 12)) _data = d;
                    break;
                case BarcodeType.EAN13:
                    if (CheckIntegerInput(d, 12, 13)) _data = d;
                    break;
                case BarcodeType.EAN8:
                    if (CheckIntegerInput(d, 7, 8)) _data = d;
                    break;
                case BarcodeType.CODE39:
                    break;
                case BarcodeType.ITF:
                    if (CheckIntegerInput(d, 1, int.MaxValue)) {
                        if (d.Length % 2 == 0) _data = d;
                    }
                    break;
                case BarcodeType.CODABAR:
                    break;
                case BarcodeType.CODE93:
                    break;
                case BarcodeType.CODE128:
                    break;
                default:
                    break;
            }

        }

        bool CheckIntegerInput(string data, int minLen, int maxLen)
        {
            bool result = false;
            if (data.Length >= minLen && data.Length <= maxLen) result = true;

            if (result) result = data.All(c => {
                if (c >= 48 && c <= 57) return true;
                return false;
            });

            return result;
        }

    }
}

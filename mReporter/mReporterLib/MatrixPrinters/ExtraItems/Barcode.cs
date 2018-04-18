using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace mReporterLib
{

    public enum BarcodeType : byte
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
    /// Prints barcode (in ESC/POS mode)
    /// </summary>
    public class Barcode : ReportItem
    {
        public Align Alignment { get; set; }
        public BarcodeHriFont HriFont { get; set; }
        public BarcodeHriPosition HriPosition { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public BarcodeType BarcodeType { get; set; }

        public Barcode() : base(ReportItemType.UserDefined)
        {
            Alignment = Align.Center;
            HriFont = BarcodeHriFont.B;
            HriPosition = BarcodeHriPosition.DoNotPrint;
            Height = 50;
            Width = 3;
            BarcodeType = BarcodeType.CODE39;
        }

        public override void Render(RenderContext context)
        {
            if (_data == null) return;
            context.AddToOutput(this, context.Report.Dialect.Align(this.Alignment));

            if (context.Report.Dialect is StarLineDialect) {

                byte bCode = 0;
                switch (BarcodeType) {
                    case BarcodeType.CODABAR: bCode = 255; break;
                    case BarcodeType.CODE128: bCode = 6; break;
                    case BarcodeType.CODE39: bCode = 4; break;
                    case BarcodeType.CODE93: bCode = 7; break;
                    case BarcodeType.EAN13: bCode = 3; break;
                    case BarcodeType.EAN8: bCode = 2; break;
                    case BarcodeType.ITF: bCode = 5; break;
                    case BarcodeType.UPC_A: bCode = 0; break;
                    case BarcodeType.UPC_E: bCode = 1; break;
                    default: bCode = 255; break;
                }

                byte position = 1;
                if (HriPosition != BarcodeHriPosition.DoNotPrint) position = 2;

                if (bCode != 255) {
                    context.AddToOutput(this, new EscCode(27, 98, bCode, position, (byte)Width, (byte)Height));
                    context.AddToOutput(this, new TextElement(_data));
                    context.AddToOutput(this, new EscCode(30));
                }

            }
            else if (context.Report.Dialect is ESCPosDialect) {
                // hri characters
                context.AddToOutput(this, new EscCode(29, 102, (byte)HriFont));
                context.AddToOutput(this, new EscCode(29, 72, (byte)HriPosition));
                // height of the code
                context.AddToOutput(this, new EscCode(29, 104, (byte)Math.Min(Height, 255)));
                // width of the code
                context.AddToOutput(this, new EscCode(29, 119, (byte)Math.Min(Width, 6)));
                // and code itself
                context.AddToOutput(this, new EscCode(29, 107, (byte)BarcodeType, (byte)_data.Length));

                context.AddToOutput(this, new TextElement(_data));
            }
        }

        string _data = null;
        public override object Data
        {
            get {
                return _data;
            }

            set {
                _data = null;
                string d = Convert.ToString(value);

                switch (BarcodeType) {
                    case BarcodeType.UPC_A:
                        if (Barcode.UPC_A.Value.IsMatch(d)) _data = d;
                        break;
                    case BarcodeType.UPC_E:
                        if (UPC_E.Value.IsMatch(d)) _data = d;
                        break;
                    case BarcodeType.EAN13:
                        if (EAN13.Value.IsMatch(d)) _data = d;
                        break;
                    case BarcodeType.EAN8:
                        if (EAN8.Value.IsMatch(d)) _data = d;
                        break;
                    case BarcodeType.CODE39:
                        if (d.Length > 0 && d.Length < 256) if (CODE39.Value.IsMatch(d)) _data = d;
                        break;
                    case BarcodeType.ITF:
                        if (d.Length > 1 && d.Length < 256) if (ITF.Value.IsMatch(d)) _data = d;
                        break;
                    case BarcodeType.CODABAR:
                        if (d.Length > 0 && d.Length < 256) if (CODABAR.Value.IsMatch(d)) _data = d;
                        break;
                    case BarcodeType.CODE93:
                        if (d.Length > 0 && d.Length < 256) if (CODE93.Value.IsMatch(d)) _data = d;
                        break;
                    case BarcodeType.CODE128:
                        if (d.Length > 0 && d.Length < 256) if (CODE128.Value.IsMatch(d)) _data = d;
                        break;
                    default:
                        break;
                }
            }
        }

        static Lazy<Regex> UPC_A = new Lazy<Regex>(() => new Regex("^[0-9]{11,12}$", RegexOptions.Compiled));
        static Lazy<Regex> UPC_E = new Lazy<Regex>(() => new Regex("^([0-9]{6,8}|[0-9]{11,12})$", RegexOptions.Compiled));
        static Lazy<Regex> EAN13 = new Lazy<Regex>(() => new Regex("^[0-9]{12,13}$", RegexOptions.Compiled));
        static Lazy<Regex> EAN8 = new Lazy<Regex>(() => new Regex("^[0-9]{7,8}$", RegexOptions.Compiled));
        static Lazy<Regex> CODE39 = new Lazy<Regex>(() => new Regex(@"^([0-9A-Z \$\%\+\-\.\/]+|\*[0-9A-Z \$\%\+\-\.\/]+\*)$", RegexOptions.Compiled));
        static Lazy<Regex> ITF = new Lazy<Regex>(() => new Regex("^([0-9]{2})+$", RegexOptions.Compiled));
        static Lazy<Regex> CODABAR = new Lazy<Regex>(() => new Regex(@"^[A-Da-d][0-9\$\+\-\.\/\:]+[A-Da-d]$", RegexOptions.Compiled));
        static Lazy<Regex> CODE93 = new Lazy<Regex>(() => new Regex(@"^[\x00-\x7F]+", RegexOptions.Compiled));
        static Lazy<Regex> CODE128 = new Lazy<Regex>(() => new Regex(@"^\{[A-C][\x00-\x7F]+", RegexOptions.Compiled));
    }
}

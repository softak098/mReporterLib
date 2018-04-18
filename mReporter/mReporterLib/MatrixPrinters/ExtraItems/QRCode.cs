using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{

    public enum QRModel
    {
        Model1 = 49, Model2 = 50, Micro = 51
    }

    public enum QRErrorCorrectionLevel
    {
        L = 48, M = 49, Q = 50, H = 51
    }

    public class QRCode : ReportItem
    {
        public QRModel Model { get; set; }
        public int Size { get; set; }
        public QRErrorCorrectionLevel ErrorCorrectionLevel { get; set; }


        public QRCode() : base(ReportItemType.UserDefined)
        {
            Model = QRModel.Model2;
            Size = 4;
            ErrorCorrectionLevel = QRErrorCorrectionLevel.L;
        }

        public override void Render(RenderContext context)
        {
            /*
            if (_data == null) return;

            StringBuilder sb = new StringBuilder();

            if (context.Report.Dialect.PrinterModel == PrinterModel.EpsonPosGeneric) {
                sb.Append(EscCode.CreateCode(29, 40, 107, 4, 0, 49, 65, (int)Model, 0));
                sb.Append(EscCode.CreateCode(29, 40, 107, 3, 0, 49, 67, Size));
                sb.Append(EscCode.CreateCode(29, 40, 107, 3, 0, 49, 69, (int)ErrorCorrectionLevel));

                // prepare data
                int pH = _data.Length / 256, pL = _data.Length % 256;
                sb.Append(EscCode.CreateCode(29, 40, 107, pL, pH, 49, 80, 48));
                sb.Append(_data);
                // and print 
                sb.Append(EscCode.CreateCode(29, 40, 107, 3, 0, 49, 81, 48));
            }
            else if (context.Report.Dialect.PrinterModel == PrinterModel.ZJ5802) {

                int pH = _data.Length / 256, pL = _data.Length % 256;
                sb.Append(EscCode.CreateCode(27, 90, 0, (int)ErrorCorrectionLevel - 48, Size, pL, pH));
                sb.Append(_data);

            }

            var line = context.AddToOutput(this, sb.ToString());
            line.AppendNewLine = false;
            return line;
            */
        }

        string _data = null;
        public override object Data
        {
            get
            {
                return _data;
            }

            set
            {
                _data = (string)value;
            }
        }

    }

}

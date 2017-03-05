using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{

    public enum NVLogoSize : byte
    {
        Normal = 0, DoubleWidth = 1, DoubleHeight = 2, Quadruple = 3
    }

    /// <summary>
    /// Special item to print logo stored in POS printer memory
    /// </summary>
    public class NVLogo : ReportItem
    {
        public Align LogoAlign { get; set; }
        byte _logoIndex = 1;
        NVLogoSize _size = NVLogoSize.Normal;

        public NVLogo() : base(ReportItemType.UserDefined)
        {
            this.LogoAlign = Align.Center;
            this._logoIndex = 1;
        }

        public NVLogo(byte logoIndex, NVLogoSize logoSize) : this()
        {
            this._logoIndex = logoIndex;
            this._size = logoSize;
        }

        public override void Render(RenderContext context)
        {
            context.AddToOutput(this, context.Report.Dialect.Align(this.LogoAlign));

            if (context.Report.Dialect is StarLineDialect) {
                context.AddToOutput(this, new EscCode(27, 28, 112, _logoIndex, (byte)_size));

            }
            else {


                context.AddToOutput(this, new EscCode(28, 112, _logoIndex, (byte)_size));
            }

        }
    }

}

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

        public override OutputLine Render(RenderContext context)
        {
            var data = new List<string>();
            data.Add(context.Report.Dialect.Align(this.LogoAlign).Apply(28, _logoIndex, (byte)_size));

            var line = context.AddToOutput(this, data);
            line.AppendNewLine = false;

            return line;
        }
    }

}

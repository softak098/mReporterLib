using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace mReporterLib
{

    public class Image : ReportItem
    {
        _BitmapData _data;

        public Image(string fileName) : base(ReportItemType.UserDefined)
        {

            _data = GetColumnFormatData(fileName);

        }

        public override void Render(RenderContext context)
        {
            //PrintColumnData(context);
            PrintRasterData(context);
        }

        void PrintRasterData(RenderContext context)
        {
            int yH = _data.Height / 256, yL = _data.Height % 256;
            int xH = _data.Width / 8 / 256, xL = _data.Width / 8 % 256;
            context.AddToOutput(this, new EscCode(29, 118, 48, 0, (byte)xL, (byte)xH, (byte)yL, (byte)yH));

            MemoryStream ms = new MemoryStream(_data.Height * (_data.Width / 8));

            int offset = 0;
            for (var h = 0; h < _data.Height; h++) {

                for (var w = 0; w < _data.Width; w += 8) {
                    byte slice = 0;
                    for (var k = 0; k < 8; k++) {
                        slice |= (byte)((_data.Dots[offset + w + k] ? 1 : 0) << (7 - k));
                    }

                    ms.WriteByte(slice);
                }
                offset += _data.Width;
            }

            context.AddToOutput(this, new BinaryData(ms));
        }

        private void PrintColumnData(RenderContext context)
        {
            int offset = 0;
            while (offset < _data.Height) {

                // The third and fourth parameters to the bit image command are
                // 'nL' and 'nH'. The 'L' and the 'H' refer to 'low' and 'high', respectively.
                // All 'n' really is is the width of the image that we're about to draw.
                // Since the width can be greater than 255 dots, the parameter has to
                // be split across two bytes, which is why the documentation says the
                // width is 'nL' + ('nH' * 256).

                int nH = _data.Width / 256, nL = _data.Width % 256;
                context.AddToOutput(this, new EscCode(27, 42, 33, (byte)nL, (byte)nH));

                MemoryStream ms = new MemoryStream(_data.Width * 3);

                for (int x = 0; x < _data.Width; ++x) {
                    // Remember, 24 dots = 24 bits = 3 bytes. 
                    // The 'k' variable keeps track of which of those
                    // three bytes that we're currently scribbling into.
                    for (int k = 0; k < 3; ++k) {
                        byte slice = 0;
                        // A byte is 8 bits. The 'b' variable keeps track
                        // of which bit in the byte we're recording.                 
                        for (int b = 0; b < 8; ++b) {
                            // Calculate the y position that we're currently
                            // trying to draw. We take our offset, divide it
                            // by 8 so we're talking about the y offset in
                            // terms of bytes, add our current 'k' byte
                            // offset to that, multiple by 8 to get it in terms
                            // of bits again, and add our bit offset to it.
                            int y = (((offset / 8) + k) * 8) + b;
                            // Calculate the location of the pixel we want in the bit array.
                            // It'll be at (y * width) + x.
                            int i = (y * _data.Width) + x;
                            // If the image (or this stripe of the image)
                            // is shorter than 24 dots, pad with zero.
                            bool v = false;
                            if (i < _data.Dots.Length) {
                                v = _data.Dots[i];
                            }
                            // Finally, store our bit in the byte that we're currently
                            // scribbling to. Our current 'b' is actually the exact
                            // opposite of where we want it to be in the byte, so
                            // subtract it from 7, shift our bit into place in a temp
                            // byte, and OR it with the target byte to get it into there.
                            slice |= (byte)((v ? 1 : 0) << (7 - b));
                        }
                        // Phew! Write the damn byte to the buffer

                        ms.WriteByte(slice);
                    }

                }
                // We're done with this 24-dot high pass. Render a newline
                // to bump the print head down to the next line
                // and keep on trucking.
                offset += 24;

                context.AddToOutput(this, new BinaryData(ms));
                //context.AddToOutput(this, context.Report.Dialect.LineFeed);
                context.AddToOutput(this, new EscCode(27, 74, 0));

                //                bw.Write(AsciiControlChars.Newline);

            }
        }

        class _BitmapData
        {
            internal BitArray Dots;
            internal int Width;
            internal int Height;
        }

        private static _BitmapData GetColumnFormatData(string bmpFileName)
        {
            using (var bitmap = (Bitmap)System.Drawing.Image.FromFile(bmpFileName)) {
                var threshold = 127;
                var index = 0;

                var extWidth = bitmap.Width - bitmap.Width % 8 + 8;
                var dots = new BitArray(extWidth * bitmap.Height);

                for (var y = 0; y < bitmap.Height; y++) {
                    for (var x = 0; x < extWidth; x++) {
                        if (x >= bitmap.Width) dots[index++] = false;
                        else {
                            var color = bitmap.GetPixel(x, y);
                            var luminance = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
                            dots[index] = (luminance < threshold);
                            index++;
                        }
                    }
                }

                return new _BitmapData() {
                    Dots = dots,
                    Height = bitmap.Height,
                    Width = extWidth
                };

            }

        }


    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace mReporterLib
{
    /*
	public static void PrintBitmapImage(Context context, String portName, String portSettings, Resources res, int source, int maxWidth, boolean compressionEnable, RasterCommand rasterType) {
		ArrayList<byte[]> commands = new ArrayList<byte[]>();

		RasterDocument rasterDoc = new RasterDocument(RasSpeed.Medium, RasPageEndMode.FeedAndFullCut, RasPageEndMode.FeedAndFullCut, RasTopMargin.Standard, 0, 0, 0);
		Bitmap bm = BitmapFactory.decodeResource(res, source);
		StarBitmap starbitmap = new StarBitmap(bm, false, maxWidth);

		if (rasterType == RasterCommand.Standard) {
			commands.add(rasterDoc.BeginDocumentCommandData());

			commands.add(starbitmap.getImageRasterDataForPrinting_Standard(compressionEnable));

			commands.add(rasterDoc.EndDocumentCommandData());
		} else {
			commands.add(starbitmap.getImageRasterDataForPrinting_graphic(compressionEnable));
			commands.add(new byte[] { 0x1b, 0x64, 0x02 }); // Feed to cutter position
		}

		sendCommand(context, portName, portSettings, commands);
	}
    */




    public class Image : ReportItem
    {
        _BitmapData _data;
        Bitmap _bitmap;

        public Image(string fileName) : base(ReportItemType.UserDefined)
        {

            //_data = GetColumnFormatData(fileName);
            //_bitmap = (Bitmap)System.Drawing.Image.FromFile(fileName);

            _data = GetRasterData(fileName);

        }

        public override void Render(RenderContext context)
        {
            if (context.Report.Dialect is StarLineDialect) {
                // initialize
                context.AddToOutput(this, new EscCode(27, 42, 114, 82));
                // enter raster mode
                context.AddToOutput(this, new EscCode(27, 42, 114, 65));
                // top margin
                context.AddToOutput(this, new EscCode(27, 42, 114, 116, (byte)'0', 0));
                // left margin
                context.AddToOutput(this, new EscCode(27, 42, 114, 109, 108, (byte)'0', 0));
                // right margin
                context.AddToOutput(this, new EscCode(27, 42, 114, 109, 114, (byte)'0', 0));
                // quality - high speed
                context.AddToOutput(this, new EscCode(27, 42, 114, 81, (byte)'0', 0));
                // length - continuous
                context.AddToOutput(this, new EscCode(27, 42, 114, 80, (byte)'0', 0));
                // FF mode
                context.AddToOutput(this, new EscCode(27, 42, 114, 70, (byte)'1', 0));
                // EOT mode
                context.AddToOutput(this, new EscCode(27, 42, 114, 69, (byte)'1', 0));

                OutputRasterData(context);

                // EOT
                context.AddToOutput(this, new EscCode(27, 12, 4));
                // quit raster mode
                context.AddToOutput(this, new EscCode(27, 42, 114, 66));
            }
            else if (context.Report.Dialect is ESCPosDialect) {

                int yH = _data.Height / 256, yL = _data.Height % 256;
                int xH = _data.Width / 8 / 256, xL = _data.Width / 8 % 256;
                context.AddToOutput(this, new EscCode(29, 118, 48, 0, (byte)xL, (byte)xH, (byte)yL, (byte)yH));

                //PrintColumnData(context);
                PrintRasterData(context);
            }
        }

        void PrintRasterData(RenderContext context)
        {
            MemoryStream ms = new MemoryStream(_data.Dots.Length / 8);

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
            internal List<byte[]> ImageLines;
            internal BitArray Dots;
            internal int Width;
            internal int Height;
        }

        private static _BitmapData GetBitmapData(string bmpFileName)
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

        private static _BitmapData GetRasterData(string bmpFileName)
        {
            const int LUMINANCE_TRESHOLD = 127;

            List<byte[]> imgLines = new List<byte[]>();

            using (var bitmap = (Bitmap)System.Drawing.Image.FromFile(bmpFileName)) {

                var extWidth = bitmap.Width - bitmap.Width % 8 + 8;
                //var dots = new BitArray(extWidth * bitmap.Height);


                for (var y = 0; y < bitmap.Height; y++) {

                    var line = new byte[extWidth / 8];

                    for (var x = 0; x < bitmap.Width; x++) {

                        var color = bitmap.GetPixel(x, y);
                        var luminance = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);

                        if (luminance < LUMINANCE_TRESHOLD)
                            line[x / 8] |= (byte)(1 << (7 - x % 8));
                    }

                    imgLines.Add(line);
                }

                return new _BitmapData {
                    ImageLines = imgLines,
                    Height = bitmap.Height,
                    Width = extWidth
                };
            }
        }


        void OutputRasterData(RenderContext context)
        {
            int emptyLines = 0;
            MemoryStream ms = new MemoryStream();

            for (int y = 0; y < _data.Height; y++) {

                var lineData = _data.ImageLines[y];
                int w = lineData.Length;

                // strip trailing zeroes - find last byte with data
                while (w > 0) {
                    if (lineData[--w] != 0) {
                        w++;
                        break;
                    }
                }

                if (w == 0) emptyLines++;
                else {
                    OutputBlankLines(ref emptyLines, ms, context);

                    // add output data of the line
                    int xH = w / 256, xL = w % 256;

                    ms.WriteByte(98);
                    ms.WriteByte((byte)xL);
                    ms.WriteByte((byte)xH);

                    ms.Write(lineData, 0, w);
                }
            }
            OutputBlankLines(ref emptyLines, ms, context);

            context.AddToOutput(this, new BinaryData(ms));
        }

        void OutputBlankLines(ref int emptyLines, Stream ms, RenderContext context)
        {
            if (emptyLines < 1) return;

            if (context.Report.Dialect is StarLineDialect) {
                ms.WriteByte(27);
                ms.WriteByte(42);
                ms.WriteByte(114);
                ms.WriteByte(89);

                var b = Encoding.ASCII.GetBytes(emptyLines.ToString());
                ms.Write(b, 0, b.Length);

                ms.WriteByte(0);
            }

            emptyLines = 0;
        }
    }



    public class StarBitmap
    {

        public static int RASTERCOMMANDHEADER = 9;    // {"ESC", "GS", "S", m, xL, xH, yL, yH, n}
        Color[] pixels;
        int height;
        int width;
        bool dithering;
        byte[] imageData;

        Bitmap _bitmap;

        internal StarBitmap(Bitmap picture, bool supportDithering, int maxWidth)
        {

            _bitmap = picture;



            height = picture.Height;
            width = picture.Width;
            pixels = new Color[height * width];

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    pixels[PixelIndex(x, y)] = picture.GetPixel(x, y);
                }
            }
            // picture.getPixels(pixels, 0, width, 0, 0, width, height);


            dithering = supportDithering;
            imageData = null;


        }



        private int pixelBrightness(int red, int green, int blue)
        {
            int level = (red + green + blue) / 3;
            return level;
        }

        private int PixelIndex(int x, int y)
        {
            return (y * width) + x;
        }



        public byte[] getImageRasterDataForPrinting_Standard(bool compressionEnable)
        {
            if (imageData != null) {
                return imageData;
            }



            int mWidth = width / 8;
            if ((width % 8) != 0) {
                mWidth++;
            }

            List<byte[]> list = new List<byte[]>();
            int dataLength = 0;
            byte[] constructedBytes = new byte[3 + mWidth];

            constructedBytes[0] = (byte)'b';

            int blank = 0;

            for (int y = 0; y < height; y++) {
                int pos = 0;

                for (int x = 0; x < mWidth; x++) {
                    byte constructedByte = 0x00;

                    for (int j = 0; j < 8; j++) {
                        Color pixel;
                        constructedByte <<= 1;

                        if (pos < width) {
                            pixel = pixels[PixelIndex(pos, y)];

                            if (pixelBrightness(pixel.R, pixel.G, pixel.B) < 127) {
                                constructedByte |= 0x01;
                            }
                        }

                        pos++;
                    }

                    constructedBytes[3 + x] = constructedByte;
                }

                int work = mWidth;

                if (compressionEnable) {
                    while (work != 0) {
                        work--;

                        if (constructedBytes[3 + work] != 0x00) {
                            work++;
                            break;
                        }
                    }
                }

                if (work != 0) {
                    while (blank >= 1000) {
                        list.Add(new byte[] { 0x1b, (byte)'*', (byte)'r', (byte)'Y', (byte)'1', (byte)'0', (byte)'0', (byte)'0', 0x00 });
                        dataLength += 9;
                        blank -= 1000;
                    }

                    if (blank != 0) {
                        list.Add(new byte[] { 0x1b, (byte)'*', (byte)'r', (byte)'Y', (byte)('0' + blank / 100), (byte)('0' + (blank % 100) / 10), (byte)('0' + blank % 10), 0x00 });
                        dataLength += 8;
                    }

                    blank = 0;

                    constructedBytes[1] = (byte)(work % 256);
                    constructedBytes[2] = (byte)(work / 256);

                    list.Add(constructedBytes.ToArray());
                    dataLength += 3 + mWidth;
                }
                else {
                    blank++;
                }
            }

            while (blank >= 1000) {
                list.Add(new byte[] { 0x1b, (byte)'*', (byte)'r', (byte)'Y', (byte)'1', (byte)'0', (byte)'0', (byte)'0', 0x00 });
                dataLength += 9;
                blank -= 1000;
            }

            if (blank != 0) {
                list.Add(new byte[] { 0x1b, (byte)'*', (byte)'r', (byte)'Y', (byte)('0' + blank / 100), (byte)('0' + (blank % 100) / 10), (byte)('0' + blank % 10), 0x00 });
                dataLength += 8;
            }

            int distPosition = 0;
            imageData = new byte[dataLength];
            for (int i = 0; i < list.Count; i++) {
                Array.Copy(list[i], 0, imageData, distPosition, list[i].Length);

                //System.arraycopy(list.get(i), 0, imageData, distPosition, list.get(i).length);
                distPosition += list[i].Length;
            }

            return imageData;
        }

   /*

        public byte[] getImageESCPOSRasterDataForPrinting()
        {
            if (imageData != null) {
                return imageData;
            }

            // Converts the image to a Monochrome image using a Steinbert Dithering algorithm. This call can be removed but it that will also remove any dithering.
            if (dithering == true) {
                ConvertToMonochromeSteinbertDithering((float)1.5);
            }

            int mWidth = width / 8;
            if ((width % 8) != 0) {
                mWidth++;
            }

            ArrayList<Byte> data = new ArrayList<Byte>();

            // The real algorithm for converting an image to escpos data is below
            int commandSize = mWidth * height;

            byte p1 = (byte)((commandSize - ((commandSize / 65536) * 65536) - ((commandSize / 16777216) * 16777216)) % 256);
            byte p2 = (byte)((commandSize - ((commandSize / 65536) * 65536) - ((commandSize / 16777216) * 16777216)) / 256);
            byte p3 = (byte)((commandSize - ((commandSize / 16777216) * 16777216)) / 65536);
            byte p4 = (byte)(commandSize / 16777216);
            byte m = 48;
            byte fn = 112;
            byte a = 48;
            byte bx = 1;
            byte by = 1;
            byte c = 49;
            byte xL = (byte)(width % 256);
            byte xH = (byte)(width / 256);
            byte yL = (byte)(height % 256);
            byte yH = (byte)(height / 256);

            byte[] rasterCommand = new byte[] { 0x1d, 0x38, 0x4c, p1, p2, p3, p4, m, fn, a, bx, by, c, xL, xH, yL, yH };

            for (int count = 0; count < rasterCommand.length; count++) {
                data.add(rasterCommand[count]);
            }

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < mWidth; x++) {
                    byte constructedByte = 0;

                    for (int j = 0; j < 8; j++) {
                        constructedByte = (byte)(constructedByte << 1);

                        int pixel;
                        int widthPixel = (x * 8) + j;
                        if (widthPixel < width) {
                            pixel = pixels[PixelIndex(widthPixel, y)];
                        }
                        else {
                            pixel = Color.WHITE;
                        }

                        if (pixelBrightness(Color.red(pixel), Color.green(pixel), Color.blue(pixel)) < 127) {
                            constructedByte = (byte)(constructedByte | 1);
                        }
                    }
                    data.add(constructedByte);
                }
            }

            imageData = new byte[data.size()];
            for (int count = 0; count < data.size(); count++) {
                imageData[count] = data.get(count);
            }

            return imageData;
        }

    */

    }
}


/*
public class RasterDocument {
	// Represaent a star raster mode page

	public enum RasTopMargin {
		Default, Small, Standard
	};

	public enum RasSpeed {
		Full, Medium, Low
	};

	public enum RasPageEndMode {
		Default, None, FeedToCutter, FeedToTearbar, FullCut, FeedAndFullCut, PartialCut, FeedAndPartialCut, Eject, FeedAndEject
	};

	RasTopMargin mTopMargin = RasTopMargin.Default;
	RasSpeed mSpeed = RasSpeed.Full;
	RasPageEndMode mFFMode = RasPageEndMode.Default;
	RasPageEndMode mEOTMode = RasPageEndMode.Default;
	int mPageLength = 0;
	int mLeftMargin = 0;
	int mRightMargin = 0;

	public RasterDocument() {
	}

	public RasterDocument(RasSpeed speed, RasPageEndMode endOfPageBehaviour, RasPageEndMode endOfDocumentBahaviour, RasTopMargin topMargin, int pageLength, int leftMargin, int rightMargin) {
		mSpeed = speed;
		mEOTMode = endOfDocumentBahaviour;
		mFFMode = endOfPageBehaviour;
		mTopMargin = topMargin;
		mPageLength = pageLength;
		mLeftMargin = leftMargin;
		mRightMargin = rightMargin;
	}

	public RasTopMargin getTopMargin() {
		return mTopMargin;
	}

	public void setTopMargin(RasTopMargin value) {
		mTopMargin = value;
	}

	public RasSpeed getPrintSpeed() {
		return mSpeed;
	}

	public void setPrintSpeed(RasSpeed value) {
		mSpeed = value;
	}

	public RasPageEndMode getEndOfPageBehaviour() {
		return mFFMode;
	}

	public void setEndOfPageBehaviour(RasPageEndMode value) {
		mFFMode = value;
	}

	public RasPageEndMode getEndOfDocumentBehaviour() {
		return mEOTMode;
	}

	public void setEndOfDocumentBehaviour(RasPageEndMode value) {
		mEOTMode = value;
	}

	public int getLeftMargin() {
		return mLeftMargin;
	}

	public void setLeftMargin(int value) {
		mLeftMargin = value;
	}

	public int getRightMargin() {
		return mRightMargin;
	}

	public void setRightMargin(int value) {
		mRightMargin = value;
	}

	public int getPageLength() {
		return mPageLength;
	}

	public void setPageLength(int value) {
		mPageLength = value;
	}

	public byte[] BeginDocumentCommandData() {
		String command = "\u001b*rA";
		command += TopMarginCommand(mTopMargin);
		command += SpeedCommand(getPrintSpeed());
		command += PageLengthCommand(mPageLength);
		command += LeftMarginCommand(mLeftMargin);
		command += RightMarginCommand(mRightMargin);
		command += SetEndOfPageModeCommand(getEndOfDocumentBehaviour());
		command += SetEndOfDocModeCommand(getEndOfDocumentBehaviour());
		return command.getBytes();
	}

	public byte[] EndDocumentCommandData() {
		return "\u001b*rB".getBytes();
	}

	public byte[] PageBreakCommandData() {
		return "\u001b\u000c\0".getBytes();
	}

	private String TopMarginCommand(RasTopMargin source) {
		final String cmd = "\u001b*rT";

		switch (source) {
		case Default:
			return cmd + "0\0";
		case Small:
			return cmd + "1\0";
		case Standard:
			return cmd + "2\0";
		}

		return cmd + "0\0";
	}

	private String SpeedCommand(RasSpeed source) {
		final String cmd = "\u001b*rQ";

		switch (source) {
		case Full:
			return cmd + "0\0";
		case Medium:
			return cmd + "1\0";
		case Low:
			return cmd + "2\0";
		}

		return cmd + "0\0";
	}

	private String LeftMarginCommand(int margin) {
		return "\u001b*rml" + Integer.toString(margin).trim() + "\0";
	}

	private String RightMarginCommand(int margin) {
		return "\u001b*rmr" + Integer.toString(margin).trim() + "\0";
	}

	private String PageLengthCommand(int pLength) {
		return "\u001b*rP" + Integer.toString(pLength).trim() + "\0";
	}

	private String EndPageModeCommandValue(RasPageEndMode mode) {
		switch (mode) {
		case Default:
			return "0";
		case None:
			return "1";
		case FeedToCutter:
			return "2";
		case FeedToTearbar:
			return "3";
		case FullCut:
			return "8";
		case FeedAndFullCut:
			return "9";
		case PartialCut:
			return "12";
		case FeedAndPartialCut:
			return "13";
		case Eject:
			return "36";
		case FeedAndEject:
			return "37";
		default:
			return "0";
		}
	}

	private String SetEndOfPageModeCommand(RasPageEndMode mode) {
		return "\u001b*rF" + EndPageModeCommandValue(mode) + "\0";
	}

	private String SetEndOfDocModeCommand(RasPageEndMode mode) {
		return "\u001b*rE" + EndPageModeCommandValue(mode) + "\0";
	}


    */

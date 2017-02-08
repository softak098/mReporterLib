using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib.LabelPrinters
{

    /// <summary>
    /// This class represents one label
    /// </summary>
    public class Label
    {
        private readonly MediaType _mediaType;
        List<Instruction> _instructions;
        LabelPosition _currentPosition = new LabelPosition(0, 0);

        public Label(MediaType mediaType)
        {
            this._mediaType = mediaType;
            _instructions = new List<Instruction>();
        }

        public void LabelSize(int width, int height)
        {

        }

        /// <summary>
        /// Moves print position
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        public void MoveTo(int x, int y)
        {
            _instructions.Add(new PositionInstruction(new LabelPosition(x, y)));
        }

        /// <summary>
        /// Produces text field on label
        /// </summary>
        /// <param name="font">Type of font</param>
        /// <param name="width">Character size, in case of bitmap font, parameter is multiplier of the base size. Maximum value is printer specific</param>
        /// <param name="height">Character size, in case of bitmap font, parameter is multiplier of the base size. Maximum value is printer specific</param>
        /// <param name="rotation">Text rotation</param>
        /// <param name="reverse">Normal or reverse print</param>
        /// <param name="text">Text to print</param>
        public void Text(TextFont font, int width, int height, TextRotation rotation, bool reverse, string text)
        {



        }




        public string Render(PrinterLanguage language)
        {


            return null;
        }



    }

}

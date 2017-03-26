using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace mReporterLib
{

    public class FreeText : ReportItem
    {
        string _text;

        public FreeText(string text) : base(ReportItemType.UserDefined)
        {
            _text = text;
        }

        public override void Render(RenderContext context)
        {
            context.AddToOutput(this, new FreeTextElement(context, _text));
        }
    }

    class FreeTextElement : OutputElement
    {
        RenderContext _context;
        string _text;

        public FreeTextElement(RenderContext context, string text)
        {
            _context = context;
            _text = text;
        }

        public override void WriteTo(Stream stream, Encoding textEncoding)
        {
            List<_Tag> tags = new List<_Tag>();

            ParserState state = ParserState.Text;
            string tag = "";
            char[] buffer = new char[_text.Length];
            int bufferPos = 0;

            for (int i = 0; i < _text.Length; i++) {
                char c = _text[i];


                switch (state) {
                    case ParserState.Text:
                        if (c == TAG_START) {
                            state = ParserState.StartTag;

                            /*
                            tags.Add(new _Tag {
                                SimpleText = new string(buffer, 0, bufferPos - 1)
                            });
                            */

                            bufferPos = 0;

                        }
                        else {
                            Write(c);

                            //buffer[bufferPos++] = c;

                        }


                        break;

                    case ParserState.StartTag:

                        if (c == TAG_END_START) {
                            state = ParserState.EndTag;

                            bufferPos = 0;
                        }
                        else if (c == TAG_PARAMETER_DELIMITER) {

                            tag = new string(buffer, 0, bufferPos - 1);


                            state = ParserState.Parameter;
                            bufferPos = 0;
                        }
                        else if (c == TAG_END) {




                            state = ParserState.Text;

                        }
                        else {
                            buffer[bufferPos++] = c;
                        }





                        break;

                    case ParserState.EndTag:

                        if (c == TAG_END) {



                        }


                        _Tag t = new _Tag { Name = tag };

                        tags.Add(t);


                        buffer[bufferPos++] = c;

                        state = ParserState.Text;
                        bufferPos = 0;

                        break;

                    case ParserState.Parameter:

                        buffer[bufferPos++] = c;



                        break;
                    default:
                        break;
                }






            }




            void Write(char c)
            {
                var b = textEncoding.GetBytes(new char[] { c });
                stream.Write(b, 0, b.Length);
            }


        }

        class _Tag
        {
            public string Name { get; set; }
            public ReportItem Item { get; set; }
            public string SimpleText { get; set; }
        }




        enum ParserState { Text, StartTag, EndTag, Parameter }
        const char TAG_START = '<';
        const char TAG_END = '>';
        const char TAG_END_START = '/';
        const char PARAMETER_DELIMITER = ',';
        const char TAG_PARAMETER_DELIMITER = ':';
    }



}

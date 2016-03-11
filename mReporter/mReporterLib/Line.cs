using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{


    public class GetDataResult
    {
        public string Value;
        public bool WordWrap;
        public Align Alignment;
        public FontStyle Style;

        public GetDataResult()
        {
            WordWrap = false;
            Alignment = Align.Left;
            Style = FontStyle.Normal;
        }

    }

    public class GetDataArgs
    {
        public int Index { get; set; }
        public object Data { get; set; }
        public GetDataResult Result { get; set; }
    }

    public delegate void GetDataHandler(GetDataArgs e);

    public class Line : ReportItem
    {
        private LineTemplate _lineTemplate;

        string _template;
        public string Template
        {
            get
            {
                return _template;
            }
            set
            {
                _template = value;
                _lineTemplate = new LineTemplate(this, value);
            }
        }

        /// <summary>
        /// TRUE to repeat all static text items on line to next lines if some value needs to be splitted to multiple lines
        /// </summary>
        public bool RepeatStaticItems { get; set; }
        /// <summary>
        /// TRUE to generate line on new page (like header for data)
        /// </summary>
        public bool RepeatOnNewPage { get; set; }
        /// <summary>
        /// Sets style for whole line
        /// </summary>
        public FontStyle Style;
        /// <summary>
        /// Sets print style for whole line
        /// </summary>
        public PrintStyle PrintStyle;

        public GetDataHandler GetData { get; set; }

        public Line(ReportItemType type) : base(type)
        {
            RepeatStaticItems = false;
            Style = FontStyle.Normal;
            PrintStyle = PrintStyle.AsBefore;
        }

        protected virtual GetDataResult GetDataResultInternal(int index)
        {
            if (this.GetData != null) {
                GetDataArgs args = new GetDataArgs {
                    Index = index,
                    Data = Data,
                    Result = new GetDataResult()
                };
                this.GetData(args);
                return args.Result;
            }

            return null;
        }

        public override OutputLine Render(RenderContext context)
        {
            if (_lineTemplate == null) return null;

            GetDataResult[] r = new GetDataResult[_lineTemplate.ValueCount];
            for (int i = 0; i < _lineTemplate.ValueCount; i++) {

                r[i] = GetDataResultInternal(i);
            }

            OutputLine oLine = context.AddToOutput(this, _lineTemplate.Format(context,r));

            if (this.Items != null) {
                // process child items
                context.SetOutputParent(oLine);
                foreach (var item in this.Items) {
                    item.Render(context);
                }
                context.SetOutputParent(oLine.Parent);
            }

            return oLine;
        }

        public override string ToString()
        {
            return this.Type.ToString();
        }

    }


}

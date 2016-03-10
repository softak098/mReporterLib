using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{

    enum Align { Left, Right, Center, Justify }

    class GetDataResult
    {
        public string Value { get; set; }
        public Align Alignment { get; set; }
        public bool WordWrap { get; set; }


        public GetDataResult()
        {
            WordWrap = false;
            Alignment = Align.Left;
        }

    }

    class GetDataArgs
    {
        public int Index { get; set; }
        public object Data { get; set; }
    }

    delegate GetDataResult GetDataHandler(GetDataArgs e);

    class Line : ReportItem
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

        public GetDataHandler GetData { get; set; }

        public Line(ReportItemType type) : base(type)
        {
            RepeatStaticItems = false;
        }

        protected virtual GetDataResult GetDataResultInternal(int index)
        {
            GetDataResult result = null;
            if (this.GetData != null) result= this.GetData(new GetDataArgs { Index = index, Data = Data });

            return result;
        }

        public override OutputLine Render(RenderContext context)
        {
            base.Render(context);
            if (_lineTemplate == null) return null;

            GetDataResult[] r = new GetDataResult[_lineTemplate.ValueCount];
            for (int i = 0; i < _lineTemplate.ValueCount; i++) {

                r[i] = GetDataResultInternal(i);
            }

            OutputLine oLine = context.AddToOutput(this, _lineTemplate.Format(r));

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

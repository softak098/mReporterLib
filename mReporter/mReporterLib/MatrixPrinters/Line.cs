﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{


    public class GetDataResult
    {
        /// <summary>
        /// Value to be rendered in place of field
        /// </summary>
        public string Value;
        /// <summary>
        /// Indicates word wrap is enabled for field
        /// </summary>
        public bool WordWrap;
        /// <summary>
        /// Alignment of value in field boundary
        /// </summary>
        public Align Alignment;
        /// <summary>
        /// Font style for field
        /// </summary>
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
        /// <summary>
        /// Index of field in line, first=0
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// Data associated with line, if any
        /// </summary>
        public object Data { get; set; }
        /// <summary>
        /// Data to render in field
        /// </summary>
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
        public bool RepeatStaticItems;
        /// <summary>
        /// TRUE to generate line on new page (like header for data)
        /// </summary>
        public bool RepeatOnNewPage;
        /// <summary>
        /// Sets font style for whole line
        /// </summary>
        public FontStyle Style;
        /// <summary>
        /// Sets print style for whole line
        /// </summary>
        public PrintStyle PrintStyle;
        /// <summary>
        /// Alignment of the line
        /// </summary>
        public Align Alignment;
        /// <summary>
        /// Type of font used
        /// </summary>
        public FontType FontType;
        /// <summary>
        /// Controls inserting page break in block generated by line.
        /// </summary>
        public bool PageBreakInside;
        /// <summary>
        /// Handler to get all fields values before line is rendered
        /// </summary>
        public GetDataHandler GetData { get; set; }

        public Line(ReportItemType type) : base(type)
        {
            RepeatStaticItems = false;
            RepeatOnNewPage = false;
            PageBreakInside = true;
            Style = FontStyle.Normal;
            PrintStyle = PrintStyle.AsBefore;
            Alignment = Align.AsBefore;
            FontType = FontType.A;
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

        public override void Render(RenderContext context)
        {
            if (_lineTemplate == null) return;

            GetDataResult[] r = new GetDataResult[_lineTemplate.ValueCount];
            for (int i = 0; i < _lineTemplate.ValueCount; i++) {

                r[i] = GetDataResultInternal(i);
            }

            LineElement oLine = new LineElement();
            _lineTemplate.Build(context, oLine, r);

            context.AddToOutput(this,oLine);

            if (this.Items != null) {
                // process child items
                context.SetParentElement(oLine);

                Items.ForEach(i => i.Render(context));

                context.SetParentElement(oLine.Parent);
            }
        }

        public override string ToString()
        {
            return this.Type.ToString();
        }

    }


}

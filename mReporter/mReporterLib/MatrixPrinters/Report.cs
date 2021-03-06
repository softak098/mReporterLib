﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{
    public class Report
    {
        List<ReportItem> _items;
        internal List<ReportItem> Items { get { return _items; } }
        /// <summary>
        /// Height of page in lines, set to 0 to disable paging (for endless printers only)
        /// </summary>
        public int PageHeight { get; set; }
        /// <summary>
        /// Defines printer dialect used to generate control sequences for printer
        /// </summary>
        public PrinterDialect Dialect { get; set; }

        public Encoding TextEncoding { get; set; }

        public Report(PrinterDialect dialect)
        {
            this._items = new List<ReportItem>();
            PageHeight = 66;
            Dialect = dialect;
            TextEncoding = Encoding.ASCII;
        }


        public RenderContext Render()
        {
            RenderContext context = new RenderContext(this);

            // pre render all items and create intermediate results
            foreach (var item in this._items) {
                if (item.Type == ReportItemType.PageFooter || item.Type == ReportItemType.PageHeader) continue; // page header&footer is solved when paging is calculated
                item.Render(context);
            }

            return context;
        }

        public PageBuilder BuildPages(RenderContext renderContext)
        {
            PageBuilder builder = new PageBuilder(this, this.TextEncoding, this.PageHeight);

            if (PageHeight > 0) {
                // we are on first page - add page header
                var header = GetPageHeader();
                if (header != null) {
                    RenderContext headerContext = new RenderContext(this);
                    header.Render(headerContext);
                    builder.Build(headerContext);
                }
            }

            builder.Build(renderContext);

            if (PageHeight > 0) {
                // finaly, we should also render page footer on last page
                builder.FinishReport(renderContext);
            }

            // all items are generated, updated placeholders for page counters
            builder.ReplacePageNumber();
            builder.ReplaceTotalPageNumber();

            builder.Reset();

            return builder;
        }


        /// <summary>
        /// Returns page header item
        /// </summary>
        internal ReportItem GetPageHeader()
        {
            return _items.FirstOrDefault(r => r.Type == ReportItemType.PageHeader);
        }

        /// <summary>
        /// Returns page footer
        /// </summary>
        /// <returns></returns>
        internal ReportItem GetPageFooter()
        {
            return _items.FirstOrDefault(r => r.Type == ReportItemType.PageFooter);
        }

        public void AddItem(ReportItem item)
        {
            this._items.Add(item);
        }

    }
}

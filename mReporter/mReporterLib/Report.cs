using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{
    class Report
    {
        List<ReportItem> _items;
        internal List<ReportItem> Items { get { return _items; } }
        /// <summary>
        /// Specifies infinite report without paging, such output to receipt printer
        /// </summary>
        public bool InfiniteReport { get; set; }
        /// <summary>
        /// Height of page in lines
        /// </summary>
        public int PageHeight { get; set; }

        public Report()
        {
            this._items = new List<ReportItem>();
            InfiniteReport = false;
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
            PageBuilder builder = new PageBuilder(this, this.PageHeight);

            // we are on first page - add page header
            var header = GetPageHeader();
            if (header != null) {
                RenderContext headerContext = new RenderContext(this);
                header.Render(headerContext);
                builder.Build(headerContext);
            }

            builder.Build(renderContext);

            /*
            foreach (var item in renderContext) {
                if (item.Type == ReportItemType.PageFooter || item.Type == ReportItemType.PageHeader) continue; // page header&footer is solved when paging is calculated

                item.BuildPages(builder);
            }
            */

            // finaly, we should also render page footer on last page
            builder.FinishReport(renderContext);

            // all items are generated, updated placeholders for page counters
            builder.ReplacePageNumber();
            builder.ReplaceTotalPageNumber();

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


        internal void AddItem(ReportItem item)
        {
            this._items.Add(item);

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{

    public delegate IEnumerable<TDataSource> GetDataSourceHandler<TDataSource>() where TDataSource : class;

    /// <summary>
    /// Rpresents functional group of header, data and footer with associated data source. It can be nested in other group also.
    /// </summary>
    public class Group<TDataSource> : ReportItem
        where TDataSource : class
    {
        public new IEnumerable<TDataSource> DataSource { get; set; }
        public GetDataSourceHandler<TDataSource> GetDataSource = null;

        public Group(ReportItemType type) : base(type)
        {
        }

        public Group() : this(ReportItemType.Group)
        {
        }

        public override OutputLine Render(RenderContext context)
        {
            if (this.GetDataSource != null) {
                this.DataSource = this.GetDataSource();
            }

            ForAllReportItems((item, data) => {

                if (data != null) item.SetData(data);
                item.Render(context);

            });

            return null;
        }

        void ForAllReportItems(Action<ReportItem, TDataSource> action)
        {
            ForReportItem(ReportItemType.Header, action);

            if (this.DataSource != null) {

                foreach (var dataItem in this.DataSource) {

                    ForReportItem(ReportItemType.Detail, action, dataItem);
                }

            }
            else {
                ForReportItem(ReportItemType.Detail, action);
            }

            ForReportItem(ReportItemType.Group, action);
            ForReportItem(ReportItemType.Footer, action);
        }

        void ForReportItem(ReportItemType type, Action<ReportItem, TDataSource> action, TDataSource data = default(TDataSource))
        {
            foreach (ReportItem item in Items.Where(i => i.Type == type)) action(item, data);
        }
    }
}

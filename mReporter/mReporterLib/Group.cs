using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{

    class GetDataSourceArgs { }

    delegate IEnumerable<TDataSource> GetDataSourceHandler<TDataSource>(GetDataSourceArgs e) where TDataSource : class;

    /// <summary>
    /// Rpresents functional group of header, data and footer with associated data source. It can be nested in other group also.
    /// </summary>
    class Group<TDataSource> : ReportItem
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
            base.Render(context);


            if (this.GetDataSource != null) {
                GetDataSourceArgs args = new GetDataSourceArgs();
                this.DataSource = this.GetDataSource(args);

            }

            ForAllReportItems((item, data) => {

                if (data != null) item.SetData(data);
                item.Render(context);

            });

            return null;
        }

        void ForAllReportItems(Action<ReportItem, TDataSource> action)
        {



            foreach (ReportItem item in Items.Where(i => i.Type == ReportItemType.Header)) {

                action(item, null);



            }

            if (this.DataSource != null) {

                foreach (var dataItem in this.DataSource) {

                    foreach (ReportItem item in Items.Where(i => i.Type == ReportItemType.Detail)) {

                        action(item, dataItem);

                    }
                }

            }
            else {
                foreach (ReportItem item in Items.Where(i => i.Type == ReportItemType.Detail)) {
                    action(item, null);
                }
            }

            foreach (ReportItem item in Items.Where(i => i.Type == ReportItemType.Group)) {
                action(item, null);
            }


            foreach (ReportItem item in Items.Where(i => i.Type == ReportItemType.Footer)) {
                action(item, null);

            }

        }

    }
}

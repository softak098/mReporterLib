using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mReporterLib
{
    class ReportItem
    {
        protected ReportItem Parent { get; private set; }

        public ReportItemType Type
        {
            get; private set;
        }

        // internal list of report items
        internal List<ReportItem> Items
        {
            get; private set;
        }

        public IEnumerable<object> DataSource { get; set; }

        public object Data
        {
            get; set;
        }

        /// <summary>
        /// Specifies if item was rendered - at least once
        /// </summary>
        public bool Rendered { get; set; }

        public ReportItem(ReportItemType type)
        {
            this.Type = type;
        }

        public virtual OutputLine Render(RenderContext context) {
            this.Rendered = true;
            return null;
        }

        public void AddItem(ReportItem item)
        {
            if (this.Items == null) this.Items = new List<ReportItem>();
            this.Items.Add(item);
            item.Parent = this;
        }

        /// <summary>
        /// Sets any data for ReportItem object
        /// </summary>
        public virtual void SetData(object data)
        {
            this.Data = data;
        }

        /// <summary>
        /// Returns first parent data found
        /// </summary>
        public object GetParentData()
        {
            ReportItem result = this.Parent;
            while (result != null) {
                if (result.Data != null) return result.Data;
                result = result.Parent;
            }
            return null;
        }
    }

}

using System;
using System.Diagnostics.CodeAnalysis;
using IES.Common;

namespace GenBOE.ActionLogic.ModelView
{
    [ExcludeFromCodeCoverage]
    public abstract class SortablePagedResultsModelView<T> : PagedResultsModelView<T>
    {
        protected SortablePagedResultsModelView()
        {
            this.SortChanged = false;
            this.SortField = "";
            this.OldSortField = "";
            this.Order = SortOrder.Ascending;
        }

        public bool SortChanged { get; set; }
        public string SortField { get; set; }
        public string OldSortField { get; set; }
        public SortOrder Order { get; set; }
        public int OrderInt
        {
            get { return (int) this.Order; }
            set { this.Order = (SortOrder)value; }
        }

        public bool isSortingRequested()
        {
			bool toReturn = this.SortChanged;

            if (toReturn)
            {
                if (this.SortField.Equals(this.OldSortField, StringComparison.CurrentCultureIgnoreCase))
                {
                    this.Order = (this.Order == SortOrder.Ascending) ? SortOrder.Descending : SortOrder.Ascending;
                }
                else
                {
                    this.Order = SortOrder.Ascending;
                }

                this.OldSortField = this.SortField;
                this.SortChanged = false;
            }

            return toReturn;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using IES.Common;

namespace GenBOE.Dtos
{
    /// <summary>
    /// View model for the BOE Summary table
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class BOESummaryGridModelView
    {
        public BOESummaryGridModelView()
            : base()
        {
            LaborType = string.Empty;
            TotalHours = 0;
            TotalCost = 0;
            NumPeople = 0;
            NumDays = 0;
            Category = ElementOfCostType.NotSet;
            BOEID = 0;
            ResourceDecimalPrecision = 0;
        }

        public string LaborType { get; set; }
        public decimal? TotalHours { get; set; }
        public decimal? TotalCost { get; set; }
        public int? NumPeople { get; set; }
        public int? NumDays { get; set; }
        public ElementOfCostType Category { get; set; }
        public int BOEID { get; set; }

        public MSTTravelMode ModeID { get; set; } // todo: may want seperate enumeration for summary grid rms travel mode id

        /// <summary>
        /// The total number of resource entries that are "rolled-up" into this row
        /// </summary>
        public int RollupCount { get; set; }

        /// <summary>
        /// Number of decimal places for resources.
        /// </summary>
        public int ResourceDecimalPrecision { get; set; }

        /// <summary>
        /// Gets Total Hours in the correct format as defined for the workspace.
        /// </summary>
        public string TotalHoursFormatted
        {
            get
            {
                return Utilities.FormatStringWithPrecision(TotalHours.HasValue ? TotalHours.Value : 0, ResourceDecimalPrecision);
            }
        }

        /// <summary>
        /// String format used by the UI for decimal precision.
        /// </summary>
        public string DecimalPrecisionStringFormat
        {
            get
            {
                return Utilities.PrecisionFormattingString(ResourceDecimalPrecision);
            }
        }
    }

    /// <summary>
    /// Extension class for <see cref="BOESummaryGridModelView"/>.
    /// </summary>
    public static class BOESummaryGridModelViewExtensions
    {
        /// <summary>
        /// Merge a set of new BOE Summary table <code>items</code> into the <code>existing</code> ones.  Method implicitly checks to make sure
        /// items are for the same BOE Summary table row before merging.
        /// </summary>
        /// <param name="existing">Set of existing entry items</param>
        /// <param name="item">Set of new entry items</param>
        /// <returns>Set of resulting merged items</returns>
        public static ICollection<BOESummaryGridModelView> Merge(this ICollection<BOESummaryGridModelView> existing, ICollection<BOESummaryGridModelView> items)
        {
            if (existing == null)
            {
                throw new ArgumentNullException(nameof(existing));
            }
            else if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            List<BOESummaryGridModelView> mergedItems = new List<BOESummaryGridModelView>();

            if (existing.Any())
            {
                foreach (BOESummaryGridModelView existingItem in existing)
                {
                    mergedItems.AddRange(existingItem.Merge(items));
                }
            }
            else
            {
                foreach (BOESummaryGridModelView item in items)
                {
                    existing.Add(item);
                }
            }

            return mergedItems;
        }

        /// <summary>
        /// Merge a set of new BOE Summary table <code>items</code> into the <code>existing</code> one.  Method implicitly checks to make sure
        /// items are for the same BOE Summary table row before merging.
        /// </summary>
        /// <param name="existing">Existing entry item</param>
        /// <param name="item">Set of new entry items</param>
        /// <returns>Set of resulting merged items</returns>
        public static ICollection<BOESummaryGridModelView> Merge(this BOESummaryGridModelView existing, ICollection<BOESummaryGridModelView> items)
        {
            if (existing == null)
            {
                throw new ArgumentNullException(nameof(existing));
            }
            else if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            ICollection<BOESummaryGridModelView> mergedItems = new List<BOESummaryGridModelView>();

            foreach (BOESummaryGridModelView item in items)
            {
                BOESummaryGridModelView mergedItem;

                if ((mergedItem = existing.Merge(item)) != null)
                {
                    mergedItems.Add(mergedItem);
                }
            }

            return mergedItems;
        }

        /// <summary>
        /// Merge the new BOE Summary table <code>item</code> into the <code>existing</code> one.  Method implicitly checks to make sure the two
        /// items are for the same BOE Summary table row before merging.
        /// </summary>
        /// <param name="existing">Existing entry item</param>
        /// <param name="item">New entry item</param>
        /// <returns>Resulting merged item</returns>
        public static BOESummaryGridModelView Merge(this BOESummaryGridModelView existing, BOESummaryGridModelView item)
        {
            if (existing == null)
            {
                throw new ArgumentNullException(nameof(existing));
            }
            else if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            BOESummaryGridModelView mergedItem = null;

            if (existing.BOEID == item.BOEID && existing.Category == item.Category && existing.LaborType == item.LaborType)
            {
                if (existing.TotalHours.HasValue && item.TotalHours.HasValue)
                {
                    existing.TotalHours = existing.TotalHours.Value + item.TotalHours.Value;
                }
                else
                {
                    existing.TotalHours = item.TotalHours;
                }

                if (existing.TotalCost.HasValue && item.TotalCost.HasValue)
                {
                    existing.TotalCost = existing.TotalCost.Value + item.TotalCost.Value;
                }
                else
                {
                    existing.TotalCost = item.TotalCost;
                }

                mergedItem = item;
            }

            return mergedItem;
        }
    }
}

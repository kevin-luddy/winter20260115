// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using GenBOE.Dtos;
    using IES.Common;

    /// <summary>
    /// Labor Type Data for new Angular front-end
    /// </summary>
    /// <seealso cref="GenBOE.ActionLogic.ModelView.PersistedDataModelView" />
    public class LaborSpreadDataModelView : PersistedDataModelView
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LaborSpreadDataModelView()
        {
            this.LaborSpreadDate = "";
            this.LaborSpreadValue = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LaborSpreadDataModelView"/> class.
        /// </summary>
        /// <param name="inBoeLaborSpread">The in BOE labor spread.</param>
        public LaborSpreadDataModelView(ResourceSpreadDto inBoeLaborSpread)
            : this()
        {
            if (inBoeLaborSpread != null)
            {
                this.LaborSpreadDate = GenBOEUtilities.AdjustDateTimePrecision(inBoeLaborSpread.LaborSpreadDate, DateTimePrecision.Month).ToMonthString();
                this.LaborSpreadValue = inBoeLaborSpread.LaborSpreadValue;
            }
        }

        private string _LaborSpreadDate;

        /// <summary>
        /// Gets or sets the labor spread date.
        /// </summary>
        [Required(ErrorMessage = "Spread Date is required.")]
        public string LaborSpreadDate
        {
            get
            {
                return this._LaborSpreadDate;
            }
            set
            {
                this._LaborSpreadDate = value;
            }
        }

        /// <summary>
        /// Gets or sets the labor spread value.
        /// </summary>
        public decimal? LaborSpreadValue { get; set; }
    }

    /// <summary>
    /// Extensions for LaborSpreadDataModelView class.
    /// </summary>
    public static class LaborSpreadDataModelViewExtensions
    {
        public static ICollection<DateTime> GetSpreadDatesFull(this ICollection<LaborSpreadDataModelView> laborSpreads)
        {
            if (laborSpreads == null || laborSpreads.All(s => s.LaborSpreadDate == null))
            {
                throw new ArgumentNullException(nameof(laborSpreads), "The labor spreads are null or have no valid dates.");
            }

            DateTime spreadStartDate = laborSpreads.Select(s => s.LaborSpreadDate.ToDateTimeMidMonth()).Min();
            DateTime spreadEndDate = laborSpreads.Select(s => s.LaborSpreadDate.ToDateTimeMidMonth()).Max();

            ICollection<DateTime> spreadDates = new List<DateTime>();

            // display the header (all dates across the spread)
            for (DateTime dt = spreadStartDate.Date; dt.Date <= spreadEndDate; dt = dt.AddMonths(1))
            {
                spreadDates.Add(dt.Normalize());
            }

            return spreadDates;
        }

        /// <summary>
        /// Converts To the boe labor spread.
        /// </summary>
        /// <param name="laborSpread">The labor spread.</param>
        /// <param name="laborTypeId">The labor type identifier.</param>
        /// <returns>A resource spread DTO.</returns>
        /// <exception cref="ArgumentNullException">laborSpread</exception>
        public static ResourceSpreadDto ToBoeLaborSpread(this LaborSpreadDataModelView laborSpread, int? laborTypeId)
        {
            if (laborSpread == null)
            {
                throw new ArgumentNullException(nameof(laborSpread));
            }

            ResourceSpreadDto spread = new ResourceSpreadDto
            {
                BoeID = -1,
                LaborTypeId = laborTypeId.HasValue ? laborTypeId.Value : -1,
                LaborSpreadDate = DateTime.Parse(laborSpread.LaborSpreadDate).Normalize(DateTimePrecision.Month),
                LaborSpreadValue = laborSpread.LaborSpreadValue.GetValueOrDefault(0m)
            };

            return spread;
        }
    }
}

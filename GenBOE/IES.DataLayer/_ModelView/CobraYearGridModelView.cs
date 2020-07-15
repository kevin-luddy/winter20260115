// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using System;
    using IES.Common;

    /// <summary>
    /// The ModelView for the Cobra Year Converter Grid.
    /// </summary>
    public class CobraYearGridModelView : IESUpdateableModelView
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CobraYearGridModelView()
        {
            this.Id = -1;
            this.Year = -1;
        }

        /// <summary>
        /// The Year.
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// The Cobra Date that corresponds to the Year.
        /// </summary>
        public DateTime CobraDate { get; set; }

        /// <summary>
        /// Gets or sets the cobra date string.
        /// </summary>
        public string CobraDateString
        {
            get
            {
                return this.CobraDate.ToString(Constants.DATE_FORMATTING_MONTH_DAY_YEAR);
            }

            set
            {
                DateTime date;

                // Default if Try parse fails is DateTime.MinValue
                DateTime.TryParse(value, out date);

                if (date != DateTime.MinValue)
                {
                    // this should fix any timezone problems
                    date = date.AddHours(12);
                }

                this.CobraDate = date;
            }
        }
    }
}

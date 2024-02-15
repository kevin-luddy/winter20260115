// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
	using IES.Common;

	public class StartEndDateTypeValidator : Validator
    {
        private DateTime? _StartDate;
        private DateTime? _EndDate;
        private string _TypeForErrorMessage;
        private string _ElementName;
        private bool canBeEqual;

        /// <summary>
        /// create validator
        /// </summary>
        /// <param name="inStartDate">the start date to use as the lower bounds allowed</param>
        /// <param name="inEndDate">the end date to use as the upper bounds allowed</param>
        /// <param name="inTypeForErroMessage">The type for the error message "The start date must be before the [inTypeForErrorMessage] start date" (ex. "BOE")</param>
        public StartEndDateTypeValidator(DateTime? inStartDate, DateTime? inEndDate, string inTypeForErroMessage)
            : this(inStartDate, inEndDate, inTypeForErroMessage, "")
        {
        }

        public StartEndDateTypeValidator(DateTime? inStartDate, DateTime? inEndDate, string inTypeForErroMessage, string ElementName, bool canBeEqual = false)
        {
            this._StartDate = inStartDate;
            this._EndDate = inEndDate;
            this._TypeForErrorMessage = inTypeForErroMessage;
            this._ElementName = String.IsNullOrEmpty(ElementName) ? "" : ElementName + " ";
            this.canBeEqual = canBeEqual;
        }


        public override Collection<String> validation(object value, Collection<Dictionary<String, String>> inData)
        {
            Collection<String> response = new Collection<string>();

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            foreach (IStartEndDates element in (IEnumerable<IStartEndDates>)value)
            {
                if (element.StartDate.HasValue && element.EndDate.HasValue)
                {
                    if (element.StartDate.Value.CompareTo(element.EndDate.Value) > 0)
                    {
                        response.Add(this._ElementName + "Start date must be before End date");
                    }
                    else 
                    {
                        // the dates are valid within the row, but need to check against 'master' (ex. BOE) date
                        if (element.StartDate.Value < this._StartDate.Value && !(element.StartDate.Value.Date.Equals(this._StartDate.Value.Date) && this.canBeEqual))
                        {
                            response.Add(this._ElementName + "Start date must be after the " + this._TypeForErrorMessage + " Start date");
                        }

                        if (element.EndDate.Value > this._EndDate.Value && !(element.EndDate.Value.Date.Equals(this._EndDate.Value.Date) && this.canBeEqual))
                        {
                            response.Add(this._ElementName + "End date must be before the " + this._TypeForErrorMessage + " End date");
                        }
                    }
                }
            }

            return response;
        }
    }

    public class verificationDateTime : IStartEndDates
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    };
}

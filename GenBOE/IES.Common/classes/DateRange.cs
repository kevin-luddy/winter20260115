// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.classes
{
    using System;
	using System.Linq;
	using System.Text.RegularExpressions;
	using IES.Common.Exceptions;

    public class DateRange : IEquatable<DateRange> {

        Nullable<DateTime> startDate, endDate;
        public DateRange() : this(new Nullable<DateTime>(), new Nullable<DateTime>()) { }
        public DateRange(Nullable<DateTime> startDate, Nullable<DateTime> endDate) {
            AssertEndDateFollowsStartDate(startDate, endDate);
            this.startDate = startDate;
            this.endDate = endDate;
        }

		/// <summary>
		/// ctor for a string representing a date range
		/// Note: this method assumes a valid format for the string
		/// </summary>
		/// <param name="dateRange">A date range as a string</param>
		public DateRange(string dateRange)
		{
			if (string.IsNullOrEmpty(dateRange))
			{
				throw new ArgumentNullException(nameof(dateRange));
			}

			// split the date range into start and end dates
			string[] dateSeparators = { "through", "thru", " to ", "-" };
			string[] monthYearSeparators = { "/", "-", " " };

			string[] splitDates = dateRange.ToLower().Split(dateSeparators, StringSplitOptions.None);
			string startDateString = splitDates[0].Trim();
			string endDateString = splitDates[1].Trim();

			// Cover cases where "-" is used both as a month/year and start date/end date separator (ex. 01-2023 - 12-2024)
			if (splitDates.Length == 4)
			{
				// this assumes a format of "month-year - month-year"
				startDateString = $"{splitDates[0].Trim()}-{splitDates[1].Trim()}";
				endDateString = $"{splitDates[2].Trim()}-{splitDates[3].Trim()}";
			}
			else if (splitDates.Length == 3)
			{
				// this assumes a format of "month - month-year" and uses the year for both dates
				startDateString = $"{splitDates[0].Trim()}-{splitDates[2].Trim()}";
				endDateString = $"{splitDates[1].Trim()}-{splitDates[2].Trim()}";
			}

			// start date may not contain a year (ex. date range was "Jan - Dec 2024") so add it if missing
			string regexYear = @"\d{2,4}\b$";
			if (!(monthYearSeparators.Any(x => startDateString.Contains(x)) && Regex.IsMatch(startDateString, regexYear)))
			{
				Match yearMatch = Regex.Match(endDateString, regexYear);
				string endDateYear = yearMatch.Value;
				startDateString = $"{startDateString} {endDateYear}";
			}

			if (startDateString.TryParseMonthYear(out DateTime parsedStartDate))
			{
				this.startDate = parsedStartDate;
			}

			if (endDateString.TryParseMonthYear(out DateTime parsedEndDate))
			{
				this.endDate = parsedEndDate;
			}
		}

        public Nullable<TimeSpan> TimeSpan {
            get {  return endDate.Value.Subtract(startDate.Value); }
        }
        public Nullable<DateTime> StartDate {
            get { return startDate; }
            set {
                AssertEndDateFollowsStartDate(value, this.endDate);
                startDate = value; 
            }
        }
        public Nullable<DateTime> EndDate {
            get { return endDate; }
            set {
                AssertEndDateFollowsStartDate(this.startDate, value);
                endDate = value; 
            }
        }
        private void AssertEndDateFollowsStartDate(Nullable<DateTime> inStartDate,
            Nullable<DateTime> inEndDate) {
                if ((inStartDate.HasValue && inEndDate.HasValue) &&
                    (inEndDate.Value < inStartDate.Value))
                {
                    throw new GenValidationException("Start Date must be less than or equal to End Date");
                }
        }
        public DateRange GetIntersection(DateRange other) {
            if(other == null)
            {
                throw new ArgumentException("other can't be null");
            }
            if (!Intersects(other))
            {
                throw new GenValidationException("Date's do not intersect");
            }
            return new DateRange(GetLaterStartDate(other.StartDate), GetEarlierEndDate(other.EndDate));
        }
        private Nullable<DateTime> GetLaterStartDate(Nullable<DateTime> other) {
            return Nullable.Compare<DateTime>(startDate, other) >= 0 ? startDate : other;
        }
        private Nullable<DateTime> GetEarlierEndDate(Nullable<DateTime> other) {
            //!endDate.HasValue == +infinity, not negative infinity 
            //as is the case with !startDate.HasValue
            if (Nullable.Compare<DateTime>(endDate, other) == 0)
            {
                return other;
            }
            if (endDate.HasValue && !other.HasValue)
            {
                return this.endDate;
            }
            if (!endDate.HasValue && other.HasValue)
            {
                return other;
            }
            return (Nullable.Compare<DateTime>(endDate, other) >= 0) ? other : endDate;
        }
        public bool Intersects(DateRange other) {
            if(other == null)
            {
                throw new ArgumentException("other can't be null");
            }
            if((this.startDate.HasValue && other.EndDate.HasValue && 
                other.EndDate.Value < this.startDate.Value) ||
                (this.endDate.HasValue && other.StartDate.HasValue && 
                other.StartDate.Value > this.endDate.Value) ||
                (other.StartDate.HasValue && this.endDate.HasValue && 
                this.endDate.Value < other.StartDate.Value) ||
                (other.EndDate.HasValue && this.startDate.HasValue && 
                this.startDate.Value > other.EndDate.Value)) {
                    return false;
            }
            return true;
        }

		/// <summary>
		/// Check if 2 date ranges have the same start and end dates
		/// </summary>
		/// <param name="other">The DateRange to compare</param>
		/// <returns>True if the 2 start dates are equal and 2 end dates are equal</returns>
        public bool Equals(DateRange other) {
			return !(other is null) && (startDate == other.StartDate) && (endDate == other.EndDate);
		}

		/// <summary>
		/// Check if the given date is the start or end date of the date range
		/// </summary>
		/// <param name="date">date to check</param>
		/// <returns>True if the given date is the start date or end date, otherwise false</returns>
		public bool HasDateAsStartOrEndDate(DateTime date)
		{
			return startDate.Value == date || endDate.Value == date;
		}

		/// <summary>
		/// Does this date range start and end on the same date?
		/// </summary>
		/// <returns>True if month and year are equal for start and end dates</returns>
		public bool StartAndEndSameDate()
		{
			return startDate.Value == endDate.Value;
		}
	}
}

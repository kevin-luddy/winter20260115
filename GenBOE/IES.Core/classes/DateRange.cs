// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Core
{
    using System;
    using IES.Core.Exceptions;

    public class DateRange : IEquatable<DateRange> {

        Nullable<DateTime> startDate, endDate;
        public DateRange() : this(new Nullable<DateTime>(), new Nullable<DateTime>()) { }
        public DateRange(Nullable<DateTime> startDate, Nullable<DateTime> endDate) {
            AssertStartDateFollowsEndDate(startDate, endDate);
            this.startDate = startDate;
            this.endDate = endDate;
        }
        public Nullable<TimeSpan> TimeSpan {
            get {  return endDate.Value.Subtract(startDate.Value); }
        }
        public Nullable<DateTime> StartDate {
            get { return startDate; }
            set {
                AssertStartDateFollowsEndDate(value, this.endDate);
                startDate = value; 
            }
        }
        public Nullable<DateTime> EndDate {
            get { return endDate; }
            set {
                AssertStartDateFollowsEndDate(this.startDate, value);
                endDate = value; 
            }
        }
        private void AssertStartDateFollowsEndDate(Nullable<DateTime> inStartDate,
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
        public bool Equals(DateRange other) {
            if (object.ReferenceEquals(other, null))
            {
                return false;
            }
            return ((startDate == other.StartDate) && (endDate == other.EndDate));
        }
    }
}

// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Models
{
	using System;
	using IES.Common.Core.Exceptions;

	public class DateRange : IEquatable<DateRange>
	{
		private DateTime? startDate, endDate;
		public DateRange() : this(new DateTime?(), new DateTime?()) { }
		public DateRange(DateTime? startDate, DateTime? endDate)
		{
			AssertStartDateFollowsEndDate(startDate, endDate);
			this.startDate = startDate;
			this.endDate = endDate;
		}
		public TimeSpan? TimeSpan
		{
			get { return endDate.Value.Subtract(startDate.Value); }
		}
		public DateTime? StartDate
		{
			get { return startDate; }
			set
			{
				AssertStartDateFollowsEndDate(value, endDate);
				startDate = value;
			}
		}
		public DateTime? EndDate
		{
			get { return endDate; }
			set
			{
				AssertStartDateFollowsEndDate(startDate, value);
				endDate = value;
			}
		}
		private void AssertStartDateFollowsEndDate(DateTime? inStartDate,
			DateTime? inEndDate)
		{
			if (inStartDate.HasValue && inEndDate.HasValue &&
				inEndDate.Value < inStartDate.Value)
			{
				throw new GenValidationException("Start Date must be less than or equal to End Date");
			}
		}
		public DateRange GetIntersection(DateRange other)
		{
			if (other == null)
			{
				throw new ArgumentException("other can't be null");
			}
			if (!Intersects(other))
			{
				throw new GenValidationException("Date's do not intersect");
			}
			return new DateRange(GetLaterStartDate(other.StartDate), GetEarlierEndDate(other.EndDate));
		}
		private DateTime? GetLaterStartDate(DateTime? other)
		{
			return Nullable.Compare(startDate, other) >= 0 ? startDate : other;
		}
		private DateTime? GetEarlierEndDate(DateTime? other)
		{
			//!endDate.HasValue == +infinity, not negative infinity 
			//as is the case with !startDate.HasValue
			if (Nullable.Compare(endDate, other) == 0)
			{
				return other;
			}
			if (endDate.HasValue && !other.HasValue)
			{
				return endDate;
			}
			if (!endDate.HasValue && other.HasValue)
			{
				return other;
			}
			return Nullable.Compare(endDate, other) >= 0 ? other : endDate;
		}
		public bool Intersects(DateRange other)
		{
			if (other == null)
			{
				throw new ArgumentException("other can't be null");
			}
			if (startDate.HasValue && other.EndDate.HasValue &&
				other.EndDate.Value < startDate.Value ||
				endDate.HasValue && other.StartDate.HasValue &&
				other.StartDate.Value > endDate.Value ||
				other.StartDate.HasValue && endDate.HasValue &&
				endDate.Value < other.StartDate.Value ||
				other.EndDate.HasValue && startDate.HasValue &&
				startDate.Value > other.EndDate.Value)
			{
				return false;
			}
			return true;
		}
		public bool Equals(DateRange other)
		{
			if (ReferenceEquals(other, null))
			{
				return false;
			}
			return startDate == other.StartDate && endDate == other.EndDate;
		}
	}
}

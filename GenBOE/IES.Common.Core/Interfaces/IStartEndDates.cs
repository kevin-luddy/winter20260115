using System;

namespace IES.Common.Core.Interfaces
{
	public interface IStartEndDates
	{
		// the start date
		DateTime? StartDate { get; }

		// the end date
		DateTime? EndDate { get; }
	}
}

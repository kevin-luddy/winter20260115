using System;

namespace IES.Core
{
    public interface IStartEndDates
    {
        // the start date
        DateTime? StartDate { get; }

        // the end date
        DateTime? EndDate { get; }
    }
}

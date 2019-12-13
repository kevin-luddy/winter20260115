// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class DateRangeValidator : Validator
    {
        public override Collection<String> validation(object value, Collection<Dictionary<String, String>> inData)
        {
            if (inData == null)
            {
                throw new ArgumentNullException(nameof(inData));
            }

            Collection<String> response = new Collection<string>();

            Dictionary<String, String> data = inData.Any() ? inData.First() : null;

            if (data != null && data.ContainsKey("StartDate") && data.ContainsKey("EndDate"))
            {
                DateTime startDate;
                DateTime endDate;

                if (DateTime.TryParse(data["StartDate"], out startDate) && DateTime.TryParse(data["EndDate"], out endDate))
                {
                    if (startDate > endDate)
                    {
                        response.Add("End Date must be after the Start Date.");
                    }
                }
            }

            return response;
        }
    }

}

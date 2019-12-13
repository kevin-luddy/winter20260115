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
    using GenBOE.Objects;
    using IES.Common;

    public class BOEDateValidator : Validator
    {
        private IFullObjectFactory factory;

        /// <summary>
        /// Constructor
        /// </summary>
        public BOEDateValidator(IFullObjectFactory factory)
        {
            this.factory = factory;
        }

        public override Collection<String> validation(object value, Collection<Dictionary<String, String>> inData)
        {
            Collection<String> response = new Collection<string>();
            if (inData == null)
            {
                throw new ArgumentNullException(nameof(inData));
            }
            Dictionary<String, String> data = new Dictionary<string, string>();

            foreach (Dictionary<string, string> dict in inData)
            {
                if (dict.ContainsKey("context"))
                {
                    if (dict["context"] == "true")
                    {
                        data = dict;
                        break;
                    }
                }
            }

            string contextElementName = string.Empty;

            if (data.ContainsKey("contextElement"))
            {
                contextElementName = data["contextElement"];
            }

            DateTime contextDate;

            if (!DateTime.TryParse(data[contextElementName], out contextDate))
            {
                response.Add("An invalid " + contextElementName + " has been entered.");
                return response;
            }

            int WorkspaceID = Convert.ToInt32(data["WorkspaceID"]);
            FullWorkspace ws = this.factory.CreateFullWorkspace(WorkspaceID);

            // The BOE dates are valid within the BOE row, but need to check against clin date
            if (data.ContainsKey("ClinID") && !string.IsNullOrEmpty(data["ClinID"]))
            {
                int ClinID = Convert.ToInt32(data["ClinID"]);

                FullClin clin = this.factory.CreateFullClin(ClinID);

                // clin is not required to have a date, but if there is a date entered both start/end date are required
                if (ClinID > 0 && clin.StartDate.HasValue)
                {
                    if (contextElementName.Equals("startdate", StringComparison.CurrentCultureIgnoreCase) &&
                         GenBOEUtilities.AdjustDateTimePrecision(contextDate, DateTimePrecision.Month) < GenBOEUtilities.AdjustDateTimePrecision(clin.StartDate.Value, DateTimePrecision.Month))
                    {
                        response.Add("Start date must be after the CLIN start date");
                    }

                    if (contextElementName.Equals("enddate", StringComparison.CurrentCultureIgnoreCase) &&
                        GenBOEUtilities.AdjustDateTimePrecision(contextDate, DateTimePrecision.Month) > GenBOEUtilities.AdjustDateTimePrecision(clin.EndDate.Value, DateTimePrecision.Month))
                    {
                        response.Add("End date must be before the CLIN end date");
                    }
                }
            }
            else // no clin date, need to get workspace date
            {
                if (contextElementName.Equals("startdate", StringComparison.CurrentCultureIgnoreCase) &&
                    GenBOEUtilities.AdjustDateTimePrecision(contextDate, DateTimePrecision.Month) < GenBOEUtilities.AdjustDateTimePrecision(ws.ContractStartDate, DateTimePrecision.Month))
                {
                    response.Add("Start date must be after the Contract start date");
                }

                if (contextElementName.Equals("enddate", StringComparison.CurrentCultureIgnoreCase) &&
                   GenBOEUtilities.AdjustDateTimePrecision(contextDate, DateTimePrecision.Month) > GenBOEUtilities.AdjustDateTimePrecision(ws.ContractEndDate, DateTimePrecision.Month))
                {
                    response.Add("End date must be before the Contract end date");
                }
            }

            return response;
        }
    }
}

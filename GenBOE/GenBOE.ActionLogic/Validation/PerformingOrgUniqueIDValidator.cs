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
    using System.Linq;
    using GenBOE.DataBridge.DTO;

    public class PerformingOrgUniqueIDValidator : Validator
    {
        private IPerformingOrgDTODataLoader perfOrgLoader;

        /// <summary>
        /// Constructor
        /// </summary>
        public PerformingOrgUniqueIDValidator(IPerformingOrgDTODataLoader perfOrgLoader)
        {
            this.perfOrgLoader = perfOrgLoader;
        }

        public override Collection<string> validation(object value, Collection<Dictionary<string, string>> inData)
        {
            Collection<string> response = new Collection<string>();
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (value.GetType() != typeof(string))
            {
                throw new InvalidCastException("value");
            }
            string valueToValidate = (string)value;
            Dictionary<String, String> data = inData != null && inData.Count > 0 ? inData.First<Dictionary<String, String>>() : null;

            if (data != null && data.Keys.Contains("PerformingOrgListID") && !data.Keys.Contains("PerformingOrgID"))
            {
                string performingOrgListID;
                data.TryGetValue("PerformingOrgListID", out performingOrgListID);

                bool isUnique = this.perfOrgLoader.GetByListIdAndName(Convert.ToInt32(performingOrgListID), valueToValidate) == null;

                if (!isUnique)
                {
                    response.Add("The Performing Org ID must be unique");
                }
            }
            else if (data != null && data.Keys.Contains("PerformingOrgListID") && data.Keys.Contains("PerformingOrgID"))
            {
                string performingOrgListID;
                data.TryGetValue("PerformingOrgListID", out performingOrgListID);

                string performingOrgID;
                data.TryGetValue("PerformingOrgID", out performingOrgID);

                bool isUnique = !this.perfOrgLoader.GetByListId(Convert.ToInt32(performingOrgListID)).Any(x => x.PerformingOrgName.ToUpper() == valueToValidate.ToUpper() && x.Id != Convert.ToInt32(performingOrgID));

                if (!isUnique)
                {
                    response.Add("The Performing Org ID must be unique");
                }
            }
            else
            {
                response.Add("Invalid data");
            }

            return response;
        }
    }
}

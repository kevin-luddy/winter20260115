// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.GeneralHelper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;

    /// <summary>
    /// Class to help manage the delegation dates and their dependencies. 
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:Require suffix for certain derived/extended classes.")]
    public class EppDelegationDatesHelper : List<KeyValuePair<EppDelegationAuthority, string>>
    {
        /// <summary>
        /// Constructor that adds the dates in reverse dependency order.
        /// Meaning that at each level, the dates for that level and all dates BELOW it are required.
        /// </summary>
        public EppDelegationDatesHelper()
        {
            Add(EppDelegationAuthority.Corporate, "CorporateEppDate");
            Add(EppDelegationAuthority.Corporate, "PreCorporateEppDate");
            Add(EppDelegationAuthority.Space, "SpaceEppDate");
            Add(EppDelegationAuthority.Space, "PreSpaceEppDate");
            Add(EppDelegationAuthority.LoB, "LobEppDate");
            Add(EppDelegationAuthority.Program, "ProgramEppDate");
        }

        /// <summary>
        /// Adds a new date with EPP delegation and associated property name to the ordered dictionary.
        /// </summary>
        /// <param name="key">Delegation authority level</param>
        /// <param name="value">Associated property name</param>
        public void Add(EppDelegationAuthority key, string value)
        {
            this.Add(new KeyValuePair<EppDelegationAuthority, string>(key, value));
        }

        /// <summary>
        /// Gets a list of Property names that are required (in sequential order) for the given EPP Delegation level.
        /// </summary>
        /// <param name="delegationAuthority">EPP Delegation assigned to the Contract</param>
        /// <returns>Ordered list of required dates</returns>
        public List<string> GetRequiredEppDatesForDelegation(EppDelegationAuthority? delegationAuthority)
        {
            List<string> result = new List<string>();
            bool matched = false;

            foreach (KeyValuePair<EppDelegationAuthority, string> item in this)
            {
                if (!matched && item.Key == delegationAuthority)
                {
                    matched = true;
                }

                if (!matched)
                {
                    continue;
                }

                result.Add(item.Value);
            }

            return result;
        }

        /// <summary>
        /// Tests the required dates for the assigned EPP Delegation level to ensure that subsequent steps are equal or greater in value.
        /// </summary>
        /// <param name="dto">Contracts DTO</param>
        /// <param name="errMessages">List to which encountered errors will be added.</param>
        /// <returns>True if all the required data are available and they are in sequential order (or equal to each other)</returns>
        /// <exception cref="ArgumentNullException">If DTO is null</exception>
        public bool AreRequiredDatesSequential(ContractsDto dto, List<string> errMessages)
        {
            _ = dto ?? throw new ArgumentNullException(nameof(dto));
            errMessages = errMessages ?? new List<string>();

            bool isValid = true;
            
            List<string> requiredDates = this.GetRequiredEppDatesForDelegation((EppDelegationAuthority?)dto?.EppDelegationAuthority);
            for (int i = 1; i < requiredDates.Count; i++)
            {
                DateTime? thisDate = (DateTime?)dto.GetType().GetProperty(requiredDates[i]).GetValue(dto, null);
                DateTime? lastDate = (DateTime?)dto.GetType().GetProperty(requiredDates[i - 1]).GetValue(dto, null);

                if ((thisDate == null || lastDate == null) || lastDate > thisDate)
                {
                    isValid = false;
                    errMessages.Add($"{requiredDates[i - 1]} is after {requiredDates[i]}, OR {requiredDates[i - 1]} or {requiredDates[i]} was null.");
                }
            }

            return isValid;
        }

        /// <summary>
        /// Tests the list of dates required for the supplied contract to see if any are missing.
        /// </summary>
        /// <param name="dto">Contracts DTO</param>
        /// <param name="errMessages">List to which encountered errors will be added.</param>
        /// <returns>True if all the required data are available and they are in sequential order (or equal to each other)</returns>
        /// <exception cref="ArgumentNullException">If DTO is null</exception>
        public bool AreRequiredDatesPopulated(ContractsDto dto, List<string> errMessages)
        {
            _ = dto ?? throw new ArgumentNullException(nameof(dto));
            errMessages = errMessages ?? new List<string>();

            bool isValid = true;
            
            List<string> requiredDates = this.GetRequiredEppDatesForDelegation((EppDelegationAuthority?)dto?.EppDelegationAuthority);

            if (requiredDates.Where(x => dto.GetType().GetProperty(x).GetValue(dto, null) == null).Any())
            {
                errMessages.Add(string.Join(", ", requiredDates.Where(x => dto.GetType().GetProperty(x).GetValue(dto, null) == null).ToArray()) + " date(s) required.");
                isValid = false;
            }

            return isValid;
        }
    }
}

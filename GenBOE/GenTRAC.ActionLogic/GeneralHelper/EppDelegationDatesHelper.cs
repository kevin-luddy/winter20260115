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
        /// Dictionary that holds frieldly names for EPP Dates
        /// </summary>
        private readonly Dictionary<string, string> EppDateNames;

        /// <summary>
        /// Constructor that adds the dates in reverse dependency order.
        /// Meaning that at each level, the dates for that level and all dates BELOW it are required.
        /// </summary>
        public EppDelegationDatesHelper()
        {
            // Property-Enum mapping
            Add(EppDelegationAuthority.Corporate, nameof(ContractsDto.ScheduledActualCorporateEppDate));
            Add(EppDelegationAuthority.Corporate, nameof(ContractsDto.ScheduledActualPreCorporateEppDate));
            Add(EppDelegationAuthority.Space, nameof(ContractsDto.ScheduledActualSpaceEppDate));
            Add(EppDelegationAuthority.Space, nameof(ContractsDto.ScheduledActualPreSpaceEppDate));
            Add(EppDelegationAuthority.LoB, nameof(ContractsDto.ScheduledActualLobEppDate));
			Add(EppDelegationAuthority.MissionSegment, nameof(ContractsDto.ScheduledActualMissionSegmentEppDate));
            Add(EppDelegationAuthority.Program, nameof(ContractsDto.ScheduledActualProgramEppDate));

			// Property-Friendly name mapping
			EppDateNames = new Dictionary<string, string>
            {
                { nameof(ContractsDto.ScheduledActualCorporateEppDate), "Scheduled/Actual Corporate EPP Date" },
                { nameof(ContractsDto.ScheduledActualPreCorporateEppDate), "Scheduled/Actual Pre-Corporate EPP Date" },
                { nameof(ContractsDto.ScheduledActualSpaceEppDate), "Scheduled/Actual Space EPP Date" },
                { nameof(ContractsDto.ScheduledActualPreSpaceEppDate), "Scheduled/Actual Pre-Space EPP Date" },
                { nameof(ContractsDto.ScheduledActualLobEppDate), "Scheduled/Actual Line of Business EPP Date" },
				{ nameof(ContractsDto.ScheduledActualMissionSegmentEppDate), "Scheduled/Actual Mission Segment EPP Date" },
                { nameof(ContractsDto.ScheduledActualProgramEppDate), "Scheduled/Actual Program EPP Date" },
				{ nameof(ContractsDto.ScheduledActualBidEppDate), "Scheduled/Actual Bid EPP Date" }
			};
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
        /// Tests the dates that have been provided to ensure that subsequent steps are equal or greater in value.
        /// </summary>
        /// <param name="dto">Contracts DTO</param>
        /// <param name="errMessages">List to which encountered errors will be added.</param>
        /// <returns>True if all the required data are available and they are in sequential order (or equal to each other)</returns>
        /// <exception cref="ArgumentNullException">If DTO is null</exception>
        public bool AreEnteredDatesSequential(ContractsDto dto, List<string> errMessages)
        {
            _ = dto ?? throw new ArgumentNullException(nameof(dto));
            errMessages = errMessages ?? new List<string>();

            bool isValid = true;

            // get a list of all possible date names
            List<string> requiredDates = this.GetRequiredEppDatesForDelegation(EppDelegationAuthority.Corporate);

			// Bid EPP date is never required, but if it is entered then it must be sequential so it's added here
			requiredDates.Add(nameof(ContractsDto.ScheduledActualBidEppDate));

			// now get a list of the date names that have actually been provided
			List<string> givenDates = requiredDates.Where(x => dto.GetType().GetProperty(x).GetValue(dto, null) != null).ToList();
            givenDates.Reverse();

            // make sure the provided dates are sequential
            for (int i = 1; i < givenDates.Count; i++)
            {
                DateTime? thisDate = (DateTime?)dto.GetType().GetProperty(givenDates[i]).GetValue(dto, null);
                DateTime? lastDate = (DateTime?)dto.GetType().GetProperty(givenDates[i - 1]).GetValue(dto, null);

                if (thisDate != null && lastDate != null && lastDate > thisDate)
                {
                    isValid = false;
                    errMessages.Add($"{EppDateNames[givenDates[i - 1]]} must be before {EppDateNames[givenDates[i]]}");
                }
            }

            return isValid;
        }

		/// <summary>
		/// Tests the list of dates required for the supplied contract to see if any are missing.
		/// </summary>
		/// <param name="dto">Contracts DTO</param>
		/// <param name="errMessages">List to which encountered errors will be added.</param>
		/// <param name="isNss">Is the LOB set to National Security Space</param>
		/// <returns>True if all the required data are available and they are in sequential order (or equal to each other)</returns>
		/// <exception cref="ArgumentNullException">If DTO is null</exception>
		public bool AreRequiredDatesPopulated(ContractsDto dto, List<string> errMessages, bool isNss = false)
        {
            _ = dto ?? throw new ArgumentNullException(nameof(dto));
            errMessages = errMessages ?? new List<string>();

            bool isValid = true;
            
            List<string> requiredDates = this.GetRequiredEppDatesForDelegation((EppDelegationAuthority?)dto?.EppDelegationAuthority);

			if (!isNss)
			{
				// Mission Segment EPP Date is only required if the Proposal's LOB is National Security Space
				if (requiredDates.Contains(nameof(ContractsDto.ScheduledActualMissionSegmentEppDate)))
				{
					requiredDates.Remove(nameof(ContractsDto.ScheduledActualMissionSegmentEppDate));
				}
			}

            if (requiredDates.Any(x => dto.GetType().GetProperty(x).GetValue(dto, null) == null))
            {
                // convert the property names to friendly names
                string[] missingDates = requiredDates.Where(x => dto.GetType().GetProperty(x).GetValue(dto, null) == null).ToArray();
                for(int i = 0; i < missingDates.Count(); i++)
                {
                    missingDates[i] = EppDateNames[missingDates[i]];
                }

                errMessages.Add(string.Join(", ", missingDates) + " required.");
                isValid = false;
            }

            return isValid;
        }
    }
}

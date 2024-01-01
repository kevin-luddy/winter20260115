// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System.Collections.Generic;
    using System.Web.Mvc;
    using IES.Standard;

    /// <summary>
    /// Contracts Loader interface
    /// </summary>
    public interface IContractsLoader : IDataLoader<ContractsDto>
    {
        /// <summary>
        /// Gets all Contracts for the proposal.
        /// </summary>
        /// <param name="proposalId">The proposal ID.</param>
        /// <returns>
        /// A Contract for the given proposal.
        /// </returns>
        ContractsDto GetContractForProposal(int proposalId);

        /// <summary>
        /// Gets a list of select items representing the EppDelegationAuthority enumeration values
        /// </summary>
        /// <param name="selectedValue">Enum option that should be selected by default</param>
        /// <returns>List of SelectListItems</returns>
        ICollection<SelectListItem> GetEppSelectValues(EppDelegationAuthority? selectedValue);
    }
}
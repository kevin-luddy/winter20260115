// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    /// <summary>
    /// Interface for Manage Proposal Info Loader
    /// </summary>
    public interface IManageProposalInfoLoader
    {
        /// <summary>
        /// Saves proposal info from the Admin Management page
        /// </summary>
        /// <param name="dtoToSave">Dto to save</param>
        /// <returns>Id of the updated Proposal</returns>
        int? SaveProposalInfo(ManageProposalInfoDto dtoToSave);
    }
}

// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.ObjectModel;
    using GenBOE.Dtos;
    using IES.Common;

    public interface IClinDTODataLoader : IDataLoader<ClinDTO>
    {
        /// <summary>
        /// Get the number of BOEs associated with a particular CLIN ID
        /// </summary>
        /// <param name="clinID">CLIN ID</param>
        /// <returns>the number of BOEs associated with the CLIN </returns>
        int GetBoeCountByClinID(int clinID);

        /// <summary>
        /// Gets a single collection of task variable IDs for a clin ID
        /// </summary>
        /// <param name="clinID"></param>
        /// <returns></returns>
        Collection<int> GetTaskVariableIDsByClinID(int clinID);

        /// <summary>
        /// Gets a single collection of workspace variable IDs for a clin ID
        /// </summary>
        /// <param name="clindId"></param>
        /// <returns></returns>
        Collection<int> GetWorkspaceVariableIDsByClinID(int clinId);

        /// <summary>
        /// Gets the Clin DTOs
        /// </summary>
        /// <param name="wsId">the WorkSpace to get ClinS</param>
        /// <returns>list of Clin DTOs</returns>
        Collection<ClinDTO> GetByWorkspaceId(int wsId);

        /// <summary>
        /// Take in a CLIN unPadded number and return a padded CLIN number
        /// </summary>
        /// <returns></returns>
        string PadClinNumber(string unPaddedCLINNum);
    }
}

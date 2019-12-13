// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using GenBOE.Dtos;
    using IES.Common;

    public interface IWbsDTODataLoader : IDataLoader<WbsDTO>
    {
        /// <summary>
        /// Gets a collection of WbsDTOs by Workspace Id.
        /// </summary>
        /// <param name="workspaceId">Id of the workspace</param>
        /// <returns>Collection of WbsDTOs</returns>
        ICollection<WbsDTO> GetByWorkspaceId(int workspaceId);

        /// <summary>
        /// Get unique WBS
        /// </summary>
        /// <param name="inWbsNumber">wbs number</param>
        /// <param name="inWorkspaceID">workspace iD</param>
        /// <returns>true if unique, false if not</returns>
        bool IsWbsNumberUnique(string inWbsNumber, int inWorkspaceID, int? inWbsID);

        /// <summary>
        /// Get the task variable IDS associated with the WBS ID
        /// </summary>
        /// <param name="inWbsID">WBS ID</param>
        /// <returns>task variable IDs</returns>
        ICollection<int> GetTaskVariableIdsById(int inWbsID);

        /// <summary>
        /// Get the workspace variable IDs associated with the WBS ID
        /// </summary>
        /// <param name="inWbsID">WBS ID</param>
        /// <returns>workspace variable IDs</returns>
        ICollection<int> GetWorkspaceVariableIdsById(int inWbsID);

        /// <summary>
        /// Check if the WBS is already tied to a Material BOE
        /// </summary>
        /// <param name="inWbsID">WBS id</param>
        /// <returns>bool</returns>
        bool IsWbsTiedToMaterialBoe(int inBoeID, int inWbsID);

        /// <summary>
        /// Check if the WBS and CLIN is already tied to a Material BOE
        /// </summary>
        /// <param name="inWbsID">WBS id</param>
        /// <param name="inClinID">CLIN id</param>
        /// <returns>bool</returns>
        bool IsWbsTiedToMaterialBoeAndClin(int inBoeID, int inWbsID, int inClinID);

        /// <summary>
        /// Gets BOE IDs for BOEs that belong to the Wbs and all of it's descendants
        /// </summary>
        /// <param name="wbsId">Wbs Id</param>
        /// <returns>A collection of Boe Ids</returns>
        ICollection<int> GetBoeIdsForWbsIdWithNesting(int wbsId);

        /// <summary>
        /// Remaps the Task and Workspace variables from a WBS to a BOE
        /// </summary>
        /// <param name="wbsId">Wbs Id</param>
        /// <param name="boeId">Boe Id</param>
        void RemapTaskAndWorkspaceVariablesFromWbsToBoe(int wbsId, int boeId);

        /// <summary>
        /// Gets a collection of all WBS that are parents of the specified WBS in the workspace
        /// </summary>
        /// <param name="workspaceId">Workspace Id</param>
        /// <param name="wbsNumber">The WBS number to find parents of</param>
        /// <returns>A collection of WBS elements, ordered by Level</returns>
        ICollection<WbsDTO> GetAllParentWbs(int workspaceId, string wbsNumber);

        /// <summary>
        /// Gets a collection of all WBS that are children of the specified WBS in the workspace
        /// </summary>
        /// <param name="workspaceId">Workspace Id</param>
        /// <param name="wbsNumber">The WBS number to find children of</param>
        /// <returns>A collection of WBS elements, ordered by Level</returns>
        ICollection<WbsDTO> GetAllChildWbs(int workspaceId, string wbsNumber);

        /// <summary>
        /// Takes in a WBS unPadded number and return an padded WBS number
        /// </summary>
        /// <param name="unPaddedWBSNum">Unpadded WBS number (i.e. the display number)</param>
        /// <returns>padded WBS number</returns>
        string PadWBSNumber(string unPaddedWBSNum);
    }
}

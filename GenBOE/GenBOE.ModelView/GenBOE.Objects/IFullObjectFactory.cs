// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Objects
{
    using System.Collections.Generic;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;

    /// <summary>
    /// By going through this factory, properties on the full objects can be mocked out in tests.
    /// </summary>
    public interface IFullObjectFactory
    {
        IBoeDTODataLoader BoeLoader { get; set; }

        /// <summary>
        /// Creates a full workspace.
        /// </summary>
        /// <param name="shortname">shortname</param>
        /// <param name="ignoreAndClearCache">Should the cache be cleared out?</param>
        /// <returns>full workspace</returns>
        FullWorkspace CreateFullWorkspace(string shortname, bool ignoreAndClearCache = false);

        /// <summary>
        /// Creates the full project map workspace.
        /// </summary>
        /// <param name="shortname">The shortname.</param>
        /// <param name="ignoreAndClearCache">if set to <c>true</c> [ignore and clear cache].</param>
        /// <returns>A full project map workspace.</returns>
        FullProjectMapWorkspace CreateFullProjectMapWorkspace(string shortname, bool ignoreAndClearCache = false);

        /// <summary>
        /// Creates a full workspace.
        /// </summary>
        /// <param name="workspaceId">workspace id</param>
        /// <returns>full workspace</returns>
        FullWorkspace CreateFullWorkspace(int workspaceId);

        /// <summary>
        /// Creates a full workspace.
        /// </summary>
        /// <param name="workspace">workspace dto</param>
        /// <returns>full workspace</returns>
        FullWorkspace CreateFullWorkspace(WorkspaceDTO workspace);

        /// <summary>
        /// Creates the cloned project map workspace.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <returns></returns>
        FullProjectMapWorkspace CreateClonedProjectMapWorkspace(FullWorkspace workspace);

        /// <summary>
        /// Gets permissions for the specified user
        /// </summary>
        /// <param name="ntid">ntid</param>
        /// <returns>Permissions for the user</returns>
        IReadOnlyCollection<SecurityPermissionsResponse> GetPermissionsForUser(string ntid);

        /// <summary>
        /// Creates a full boe.
        /// </summary>
        /// <param name="boeId">boe id</param>
        /// <returns>full boe</returns>
        FullBoe CreateFullBoe(int boeId);
        
        /// <summary>
        /// Creates a full boe.
        /// </summary>
        /// <param name="boe">boe dto</param>
        /// <returns>full boe</returns>
        FullBoe CreateFullBoe(BoeDTO boe);

        /// <summary>
        /// Creates an empty full boe.
        /// </summary>
        /// <param name="boe">boe dto</param>
        /// <returns>full boe</returns>
        FullBoe CreateFullBoe();

        /// <summary>
        /// Creates a collection of Full Boes
        /// </summary>
        /// <param name="boeIds">Boe Ids</param>
        /// <returns>Collection of full boes</returns>
        ICollection<FullBoe> CreateFullBoes(ICollection<int> boeIds);

        /// <summary>
        /// Creates a collection of Full Boes, and preloads RTE data into them
        /// </summary>
        /// <param name="boeIds">Boe Ids</param>
        /// <returns>Collection of full boes</returns>
        ICollection<FullBoe> CreateFullBoesWithRteData(ICollection<int> boeIds);

        /// <summary>
        /// Creates Full Clin based on Id
        /// </summary>
        /// <param name="clinId">Clin Id</param>
        /// <returns>Full Clin</returns>
        FullClin CreateFullClin(int clinId);

        /// <summary>
        /// Creates a collection of Full Clins based on Ids
        /// </summary>
        /// <param name="clinIds">Clin Ids</param>
        /// <returns>an iCollection of Full Clins</returns>
        ICollection<FullClin> CreateFullClins(ICollection<int> clinIds);

        /// <summary>
        /// Creates Empty Full Clin
        /// </summary>
        /// <returns>Full Clin</returns>
        FullClin CreateFullClin();

        /// <summary>
        /// Creates Full Clin based on Clin Dto
        /// </summary>
        /// <param name="clin">Clin Dto</param>
        /// <returns>Full Clin</returns>
        FullClin CreateFullClin(ClinDTO clin);

        /// <summary>
        /// Creates Full Wbs
        /// </summary>
        /// <param name="wbsId">Wbs Id</param>
        /// <returns>Full Wbs</returns>
        FullWbs CreateFullWbs(int wbsId);

        /// <summary>
        /// Creates Full Wbs
        /// </summary>
        /// <param name="wbs">Wbs</param>
        /// <returns>Full Wbs</returns>
        FullWbs CreateFullWbs(WbsDTO wbs);

        /// <summary>
        /// Creates an empty Full Wbs
        /// </summary>
        /// <returns>Full Wbs</returns>
        FullWbs CreateFullWbs();

        /// <summary>
        /// Creates Full Wbs objects based on Ids
        /// </summary>
        /// <param name="wbsIds">Wbs Ids</param>
        /// <returns>Corresponding Data</returns>
        ICollection<FullWbs> CreateFullWbses(ICollection<int> wbsIds);

        /// <summary>
        /// Gets Material element from an Id.
        /// </summary>
        /// <param name="materialId">Material Id</param>
        /// <returns>Full material element.</returns>
        MaterialDTO GetMaterialById(int materialId);

        /// <summary>
        /// Creates Task element from an Id.
        /// </summary>
        /// <param name="taskElementId">Task Element Id</param>
        /// <param name="costPrecision">The Cost precision for the workspace.</param>
        /// <param name="hoursPrecision">The Hours precision for the workspace.</param>
        /// <returns>Full task element.</returns>
        BoeTaskElementDTO CreateTaskElement(int taskElementId, int hoursPrecision, int costPrecision);

        /// <summary>
        /// Creates Travel dto from an Id.
        /// </summary>
        /// <param name="travelId">Travel Id</param>
        /// <returns>Travel Dto.</returns>
        TravelDTO CreateTravel(int travelId);

        /// <summary>
        /// Creates an Other Direct Cost from an Id.
        /// </summary>
        /// <param name="otherDirectCostId">Other Direct Cost Id</param>
        /// <returns>Other Direct Cost Dto.</returns>
        OtherDirectCostDTO CreateOtherDirectCost(int otherDirectCostId);

        /// <summary>
        /// Creates an Approver Response from an Id.
        /// </summary>
        /// <param name="approverResponseId">Approver response Id.</param>
        /// <returns>Approver Response DTO.</returns>
        BoeApproverResponseDTO CreateApproverResponse(int approverResponseId);

        /// <summary>
        /// Creates a Custom Field from an Id.
        /// </summary>
        /// <param name="customFieldId">Custom Field Id.</param>
        /// <returns>Custom Field DTO.</returns>
        CustomFieldDTO CreateCustomField(int customFieldId);

        /// <summary>
        /// Create a collect of Custom Field by Ids.
        /// </summary>
        /// <param name="customFieldIds">Custom Field Ids.</param>
        /// <returns>Custom Field DTO collection.</returns>
        ICollection<CustomFieldDTO> CreateCustomFields(ICollection<int> customFieldIds);

        /// <summary>
        /// Clears the permissions cache.
        /// </summary>
        /// <param name="ntId">The nt identifier.</param>
        void ClearPermissionsCache(string ntId);

        /// <summary>
        /// Clears the workspace cache.
        /// </summary>
        /// <param name="shortname">Short name of the workspace.</param>
        void ClearWorkspaceCache(string shortname);
    }
}

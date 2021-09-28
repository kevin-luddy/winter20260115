// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using GenBOE.Dtos;
    using IES.Common;

    public interface IBoeTaskElementDTODataLoader : IBulkDataLoader<BoeTaskElementDTO>
    {
        int CreateOrSaveLMLaborSpread(int LaborTypeID, ResourceSpreadDto inSaveLaborSpread);
        int CreateorSaveLMLaborType(int TaskElementID, ResourceTypeDto inSaveLMLaborType);
        int CreateOrSaveTaskElementDetail(int BoeID, BoeTaskElementDTO inTaskDetail);
        int DeleteLMLaborSpreadsByLaborTypeID(int LaborTypeID);
        void DeleteLMLaborType(ResourceTypeDto inDeleteLaborType);
        void DeleteTaskElementDetail(BoeTaskElementDTO inTaskElementDetail);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flag")]
        void SaveBOETaskElementLaborTypeWarning(int inBOETaskElementID, bool inLaborTypeWarningFlag);
        Dictionary<int, int> SaveBoeTaskElements(Collection<BoeTaskElementDTO> inBoeTaskElementDTOs);
        Dictionary<int, int> SaveOrdinaryVariables(Collection<OrdinaryVariableDto> inOrdinaryVars);

        OrdinaryVariableDto GetTaskVariableByTaskVariableID(int inTaskVariableID);

        /// <summary>
        /// Gets the by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="hoursPrecision">The hours precision.</param>
        /// <param name="costPrecision">The cost precision.</param>
        /// <returns></returns>
        BoeTaskElementDTO GetById(int id, int hoursPrecision, int costPrecision);

        /// <summary>
        /// Gets Task Elements by Boe Id
        /// </summary>
        /// <param name="boeId">Boe Id</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <param name="costPrecision">The Cost precision for the workspace.</param>
        /// <param name="hoursPrecision">The Hours precision for the workspace.</param>
        /// <returns>Fully Loaded Boe Task Element DTOs</returns>
        ICollection<BoeTaskElementDTO> GetByBoeId(int boeId, int hoursPrecision, int costPrecision, bool includeRTEFields = false);

        /// <summary>
        /// (BULK LOAD) Get Boe Task Elements by Ids
        /// </summary>
        /// <param name="ids">Boe Task Element Ids</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <param name="costPrecision">The Cost precision for the workspace.</param>
        /// <param name="hoursPrecision">The Hours precision for the workspace.</param>
        /// <returns>Boe Task Elements</returns>
        ICollection<BoeTaskElementDTO> GetByIds(ICollection<int> ids, bool includeRTEFields, int hoursPrecision, int costPrecision);

        /// <summary>
        /// Gets Task Elements by Boe Id
        /// </summary>
        /// <param name="boeIds">A collection of Boe Ids</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <param name="costPrecision">The Cost precision for the workspace.</param>
        /// <param name="hoursPrecision">The Hours precision for the workspace.</param>
        /// <returns>Fully Loaded Boe Task Element DTOs</returns>
        ICollection<BoeTaskElementDTO> GetByBoeIds(ICollection<int> boeIds, bool includeRTEFields, int hoursPrecision, int costPrecision);

        /// <summary>
        /// Gets Task Elements by Workspace Id
        /// </summary>
        /// <param name="wsId">Workspace Id</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <param name="costPrecision">The Cost precision for the workspace.</param>
        /// <param name="hoursPrecision">The Hours precision for the workspace.</param>
        /// <returns>Fully Loaded Boe Task Element DTOs</returns>
        ICollection<BoeTaskElementDTO> GetByWorkspaceId(int wsId, bool includeRTEFields, int hoursPrecision, int costPrecision);

        /// <summary>
        /// Pulls RTE fields for the DTOs, and updates them as needed
        /// </summary>
        /// <param name="dtos">DTOs that whose RTE fields will be loaded, if they are null</param>
        void LoadRTEFields(ICollection<BoeTaskElementDTO> dtos);
    }
}

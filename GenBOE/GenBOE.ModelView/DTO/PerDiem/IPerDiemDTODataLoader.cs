// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using GenBOE.Dtos;

    public interface IPerDiemDTODataLoader
    {
        /// <summary>
        /// Gets All Per diems from the DB.
        /// </summary>
        /// <returns></returns>
        ICollection<PerDiemDTO> GetAllPerDiem();

        /// <summary>
        /// Gets a collection of perdiems for the Given Ids.
        /// </summary>
        /// <param name="perdiemIds">Perdiem Ids.</param>
        /// <returns>Perdiems.</returns>
        ICollection<PerDiemDTO> GetByIds(ICollection<int> perdiemIds);

        /// <summary>
        /// Gets per diems that are locked by the workspace.  If the workspace is not in the locked, closed or completed state, returns
        /// the system level per diems.
        /// </summary>
        /// <param name="perDiemIds">Per diem ids.</param>
        /// <param name="inWorkspace">Workspace.</param>
        /// <returns>Locked per diem or system level perdiem.</returns>
        ICollection<PerDiemDTO> GetByIds(ICollection<int> perDiemIds, WorkspaceDTO inWorkspace);

        /// <summary>
        /// Save a per diem.  Called for insert or update.
        /// </summary>
        /// <param name="inPerDiem">Per diem to save.</param>
        /// <returns>Id of the saved per diem.</returns>
        int SavePerDiem(PerDiemDTO inPerDiem);

        /// <summary>
        /// Gets a per diem that is locked by the workspace.  If the workspace is not in the locked, closed or completed state, returns
        /// the system level per diem.
        /// </summary>
        /// <param name="perDiemID">Per diem id.</param>
        /// <param name="inWorkspace">Workspace.</param>
        /// <returns>Locked per diem or system level perdiem.</returns>
        PerDiemDTO GetPerDiemDTOByPerDiemID(int perDiemID, WorkspaceDTO inWorkspace);
    }
}

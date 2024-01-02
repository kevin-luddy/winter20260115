// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.Dtos;
    using GenBOE.Models;
    using IES.Standard;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// Performs all the CRUD for Per diems.
	/// </summary>
	public class PerDiemDTODataLoader : IPerDiemDTODataLoader
    {
        private readonly ILogger _log;

        /// <summary>
        /// default ctor
        /// </summary>
        /// <param name="logger">logger</param>
        public PerDiemDTODataLoader(ILogger<PerDiemDTODataLoader> logger)
		{
			this._log = logger;
		}

        /// <summary>
        /// Gets a collection of per diems for the Given Ids.
        /// </summary>
        /// <param name="perdiemIds">Per diem Ids.</param>
        /// <returns>Perdiems.</returns>
        
        public ICollection<PerDiemDTO> GetByIds(ICollection<int> perdiemIds)
        {
            ICollection<PerDiemDTO> perdiems = new Collection<PerDiemDTO>();

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                perdiems = ConvertToDto((from p in gbe.PerDiems
                                         where perdiemIds.Contains(p.PerDiemID)
                                         select p).ToCollection<PerDiem>());
            }
            return perdiems;
        }

        /// <summary>
        /// Gets all Per diems from the DB. 
        /// </summary>
        /// <returns>All per diems.</returns>
        
        virtual public ICollection<PerDiemDTO> GetAllPerDiem()
        {
            ICollection<PerDiemDTO> diems = new Collection<PerDiemDTO>();
            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    diems = ConvertToDto((from p in gbe.PerDiems
                                          select p).ToCollection<PerDiem>());
                }
            }

            return diems;
        }

        /// <summary>
        /// Save a per diem.  Called for insert or update.
        /// </summary>
        /// <param name="inPerDiem">Per diem to save.</param>
        /// <returns>Id of the saved per diem.</returns>
        virtual public int SavePerDiem(PerDiemDTO inPerDiem)
        {
            if (inPerDiem == null)
            {
                throw new ArgumentNullException(nameof(inPerDiem));
            }
            if (inPerDiem.Updateable == UpdateType.None)
            {
                throw new ArgumentException("please supply the Updateable argument");
            }

            int PerDiemID = 0;
            if (inPerDiem.Updateable == UpdateType.Upsert)
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    PerDiemID = Convert.ToInt32(gbe.upsertPerDiem(inPerDiem.Id, inPerDiem.PerDiemDestination, inPerDiem.Qualification, inPerDiem.HotelRate, inPerDiem.MIERate, inPerDiem.PerDiemNotes, inPerDiem.LastUpdatedBy, inPerDiem.UpdateDate).SingleOrDefault());
                } // end using gbe
            }
            else if (inPerDiem.Updateable == UpdateType.Deleted)
            {
                throw new NotSupportedException("Per Diem cannot be deleted");
            }

            return PerDiemID;
        }

        /// <summary>
        /// Gets a per diem that is locked by the workspace.  If the workspace is not in the locked, closed or completed state, returns
        /// the system level per diem.
        /// </summary>
        /// <param name="perDiemID">Per diem id.</param>
        /// <param name="inWorkspace">Workspace.</param>
        /// <returns>Locked per diem or system level perdiem.</returns>
        public virtual PerDiemDTO GetPerDiemDTOByPerDiemID(int perDiemID, WorkspaceDTO inWorkspace)
        {
            if (inWorkspace == null)
            {
                throw new ArgumentNullException(nameof(inWorkspace));
            }

            PerDiemDTO toReturn;
            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                if (inWorkspace.WorkspaceState == WorkspaceState.Locked || inWorkspace.WorkspaceState == WorkspaceState.Closed || inWorkspace.WorkspaceState == WorkspaceState.Complete)
                {
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        toReturn = ConvertToDto((from p in gbe.WorkspaceLockedPerDiems
                                                 where p.PerDiemID == perDiemID && p.WorkspaceID == inWorkspace.Id
                                                 select p).ToCollection<WorkspaceLockedPerDiem>()).FirstOrDefault();
                    }
                }
                else
                {
                    //get the system level rates
                    toReturn = this.GetByIds(new Collection<int> { perDiemID }).FirstOrDefault();
                }

                // With enhancements 35365/35366, BOEs in a Locked workspace can now be editable (and new/unsaved trips created)
                if (toReturn == null && inWorkspace.WorkspaceState == WorkspaceState.Locked)
                {
                    toReturn = this.GetByIds(new Collection<int> { perDiemID }).FirstOrDefault();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Gets per diems that are locked by the workspace.  If the workspace is not in the locked, closed or completed state, returns
        /// the system level per diems.
        /// </summary>
        /// <param name="perDiemIds">Per diem ids.</param>
        /// <param name="inWorkspace">Workspace.</param>
        /// <returns>Locked per diems or system level perdiems.</returns>
        public virtual ICollection<PerDiemDTO> GetByIds(ICollection<int> perDiemIds, WorkspaceDTO inWorkspace)
        {
            if (inWorkspace == null)
            {
                throw new ArgumentNullException(nameof(inWorkspace));
            }

            ICollection<PerDiemDTO> locked = new Collection<PerDiemDTO>();
            ICollection<PerDiemDTO> toReturn = new Collection<PerDiemDTO>();

            if (inWorkspace.WorkspaceState == WorkspaceState.Locked || inWorkspace.WorkspaceState == WorkspaceState.Closed || inWorkspace.WorkspaceState == WorkspaceState.Complete)
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    locked = ConvertToDto((from p in gbe.WorkspaceLockedPerDiems
                                             where perDiemIds.Contains(p.PerDiemID) && p.WorkspaceID == inWorkspace.Id
                                             select p).ToCollection<WorkspaceLockedPerDiem>());
                }
            }

            if (locked.Any() || inWorkspace.WorkspaceState == WorkspaceState.Closed || inWorkspace.WorkspaceState == WorkspaceState.Complete)
            {
                //Always use locked WS rates in Complete/Closed workspaces
                //When a WS is locked, there will not be WS rates if it contains an unlocked BOE, so system rates should be used
                toReturn = locked;
            }
            else
            {
                //Workspace rates are not locked
                toReturn = this.GetByIds(perDiemIds);
            }

            return toReturn;
        }

        #region privates

        /// <summary>
        /// Converts all PerDiem entities to PerDiemDTOs.
        /// </summary>
        /// <param name="entities">PerDiem entities.</param>
        /// <returns>Converted PerDiem Dtos.</returns>
        private ICollection<PerDiemDTO> ConvertToDto(ICollection<PerDiem> entities)
        {
            ICollection<PerDiemDTO> perdiems = new Collection<PerDiemDTO>();

            if (entities.Any())
            {
                foreach (PerDiem entity in entities)
                {
                    perdiems.Add(new PerDiemDTO
                    {
                        Id = entity.PerDiemID,
                        PerDiemDestination = entity.PerDiemDestination,
                        Qualification = entity.Qualification,
                        HotelRate = entity.HotelRate,
                        MIERate = entity.MIERate,
                        PerDiemNotes = entity.PerDiemNotes,
                        LastUpdatedBy = entity.PerDiemLastUpdateETIUserID,
                        PerDiemLastUpdatedDate = GenBOEUtilities.AdjustDateTimePrecision(entity.PerDiemLastUpdateDT, DateTimePrecision.Day),
                        UpdateDate = entity.UpdateDT,
                        // Records from the system level per diems are not considered locked.
                        LockedRate = false
                    });
                }
            }
            return perdiems;
        }

        /// <summary>
        /// Converts all WorkspaceLockedPerDiem entities to PerDiemDTOs.
        /// </summary>
        /// <param name="entities">PerDiem entities.</param>
        /// <returns>Converted PerDiem Dtos.</returns>
        private ICollection<PerDiemDTO> ConvertToDto(ICollection<WorkspaceLockedPerDiem> entities)
        {
            ICollection<PerDiemDTO> perdiems = new Collection<PerDiemDTO>();

            if (entities.Any())
            {
                foreach (WorkspaceLockedPerDiem entity in entities)
                {
                    perdiems.Add(new PerDiemDTO
                    {
                        Id = entity.PerDiemID,
                        PerDiemDestination = entity.PerDiemDestination,
                        Qualification = entity.Qualification,
                        HotelRate = entity.HotelRate,
                        MIERate = entity.MIERate,
                        PerDiemNotes = entity.PerDiemNotes,
                        LastUpdatedBy = entity.PerDiemLastUpdateETIUserID,
                        PerDiemLastUpdatedDate = GenBOEUtilities.AdjustDateTimePrecision(entity.PerDiemLastUpdateDT, DateTimePrecision.Day),
                        UpdateDate = entity.UpdateDT,
                        // Records from the workspace level per diems are considered locked.
                        LockedRate = true
                    });
                }
            }
            return perdiems;
        }

        #endregion
    }
}

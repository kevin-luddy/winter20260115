// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using IES.Standard;
    using System.Collections.ObjectModel;
    using GenBOE.Models;
    using GenBOE.Dtos;

    public class MiscTravelRateDTOLoader : DataLoader<MiscTravelRateDTO>, IMiscTravelRateDTOLoader
    {
        /// <summary>
        /// default ctor
        /// </summary>
        /// <param name="logger">logger</param>
        public MiscTravelRateDTOLoader(ILogger logger)
		{
			this.Log = logger;
		}
        /// <summary>
        /// Gets all miscellaneous travel rates.
        /// </summary>
        /// <returns>Collection of MiscTravelRateDTO</returns>
        
        virtual public Collection<MiscTravelRateDTO> GetAll()
        {
            Collection<MiscTravelRateDTO> toReturn = null;
            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {

                //Get All Misc Travel Rates
                using (GenBoeEntities gbe = new GenBoeEntities())
                {

                    toReturn = (from m in gbe.TravelMiscRates
                                select new MiscTravelRateDTO
                                {
                                    Id = m.TravelMiscRateID,
                                    MiscTravelRateMode = m.TransportationMode,
                                    MiscTravelRate = m.MiscellaneousRate,
                                    SortCode = m.SortCode,
                                    inUse = m.MiscRateInUse,
                                    // System rates are not considered locked.
                                    lockedRate = false,
                                    UpdateDate = m.UpdateDT
                                }).ToCollection<MiscTravelRateDTO>();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Gets a miscellaneous travel rate by its travel rate Id.
        /// </summary>
        /// <param name="inTravelRateID">Travel Rate Id.</param>
        /// <returns>Travel rate for given Id.</returns>
        
        virtual public new MiscTravelRateDTO GetById(int inTravelRateID)
        {
            MiscTravelRateDTO toReturn = new MiscTravelRateDTO();

            // Get the misctravelrate Data
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                toReturn = this.GetByIds(new Collection<int> { inTravelRateID }).FirstOrDefault();
            }

            return toReturn;
        }

        /// <summary>
        /// Gets a collection of MiscTraveRateDTOs for a given list of Ids.
        /// </summary>
        /// <param name="travelRateIds">Travel Rate Ids.</param>
        /// <returns>MiscTraveRateDTOs</returns>
        public override ICollection<MiscTravelRateDTO> GetByIds(ICollection<int> ids)
        {
            ICollection<MiscTravelRateDTO> miscTravelRates = new Collection<MiscTravelRateDTO>();

            // Get the misctravelrate Data
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                miscTravelRates = (from m in gbe.TravelMiscRates
                                   where ids.Contains(m.TravelMiscRateID)
                                   select new MiscTravelRateDTO
                                   {
                                       Id = m.TravelMiscRateID,
                                       MiscTravelRateMode = m.TransportationMode,
                                       MiscTravelRate = m.MiscellaneousRate,
                                       SortCode = m.SortCode,
                                       inUse = m.MiscRateInUse,
                                       // System rates are not considered locked.
                                       lockedRate = false,
                                       UpdateDate = m.UpdateDT
                                   }).ToCollection<MiscTravelRateDTO>();
            }
            return miscTravelRates;
        }

        
        virtual public MiscTravelRateDTO GetById(int inTravelRateID, WorkspaceDTO inWorkspace)
        {
            if (inWorkspace == null)
            {
                throw new ArgumentNullException(nameof(inWorkspace));
            }
            MiscTravelRateDTO toReturn = new MiscTravelRateDTO();

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {             
                if (inWorkspace.WorkspaceState == WorkspaceState.Locked || inWorkspace.WorkspaceState == WorkspaceState.Closed || inWorkspace.WorkspaceState == WorkspaceState.Complete)
                {
                    //get the WS locked rate
                    toReturn = this.GetWorkspaceLockedMiscTravelRateDTOByID(inTravelRateID, inWorkspace.Id);
                }
                else
                {
                    toReturn = this.GetById(inTravelRateID);
                }

                // With enhancements 35365/35366, BOEs in a Locked workspace can now be editable (and new/unsaved trips created)
                if (toReturn == null && inWorkspace.WorkspaceState == WorkspaceState.Locked)
                {
                    toReturn = this.GetById(inTravelRateID);
                }
            }

            return toReturn;

        }

        /// <summary>
        /// Retrieves a locked <see cref="MiscTravelRateDTO"/>s from the database if the workspace is locked, otherwise retrieves
        /// system misc travel rates.
        /// </summary>
        /// <param name="inMiscTravelRateIds">The IDs of the <see cref="MiscTravelRateDTO"/>s being retrieved</param>
        /// <param name="inWorkspace">The Workspace dto.</param>
        /// <returns>The <see cref="MiscTravelRateDTO"/>s requested</returns>
        
        public ICollection<MiscTravelRateDTO> GetByIds(ICollection<int> inMiscTravelRateIds, WorkspaceDTO inWorkspace)
        {
            ICollection<MiscTravelRateDTO> locked = new Collection<MiscTravelRateDTO>();
            ICollection<MiscTravelRateDTO> toReturn = new Collection<MiscTravelRateDTO>();

            if (inWorkspace == null)
            {
                throw new ArgumentNullException(nameof(inWorkspace));
            }

            if (inWorkspace.WorkspaceState == WorkspaceState.Locked || inWorkspace.WorkspaceState == WorkspaceState.Closed || inWorkspace.WorkspaceState == WorkspaceState.Complete)
            {
                // Get the misctravelrate Data
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    locked = (from m in gbe.WorkspaceLockedTravelMiscRates
                                where inMiscTravelRateIds.Contains(m.TravelMiscRateID) && m.WorkspaceID == inWorkspace.Id
                                select new MiscTravelRateDTO
                                {
                                    Id = m.TravelMiscRateID,
                                    MiscTravelRateMode = m.TransportationMode,
                                    MiscTravelRate = m.MiscellaneousRate,
                                    SortCode = m.SortCode,
                                    inUse = m.MiscRateInUse,
                                    lockedRate = true,
                                    UpdateDate = m.UpdateDT
                                }).ToCollection<MiscTravelRateDTO>();
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
                toReturn = this.GetByIds(inMiscTravelRateIds);
            }

            return toReturn;
        }

        /// <summary>
        /// Delete the Misc travel rate
        /// </summary>
        /// <param name="inTravelrate"></param>        
        protected override int? Delete(MiscTravelRateDTO dtoToDelete)
        {
            if (dtoToDelete == null)
            {
                throw new ArgumentNullException(nameof(dtoToDelete));
            }

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    //Change to delete travel rate when SP is in.
                    gbe.deleteTravelMiscRate(dtoToDelete.Id, dtoToDelete.UpdateDate);
                }
            }

            return dtoToDelete.Id;
        }
        
        virtual public Dictionary<int, int> SaveMiscTravelRates(Collection<MiscTravelRateDTO> inMiscTravelRateDTOs)
        {
            if (inMiscTravelRateDTOs == null)
            {
                throw new ArgumentNullException(nameof(inMiscTravelRateDTOs));
            }
            Dictionary<int, int> toReturn = new Dictionary<int, int>();

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                foreach (MiscTravelRateDTO mtr in inMiscTravelRateDTOs)
                {
                    if (mtr.Updateable == UpdateType.Deleted)
                    {
                        this.Delete(mtr);
                        toReturn.Add(mtr.Id, mtr.Id);
                    }
                    else if (mtr.Updateable == UpdateType.Upsert)
                    {
                        toReturn.Add(mtr.Id, (int)this.Save(mtr));
                    }

                } // end foreach
            }

            return toReturn;
        }

        /// <summary>
        /// Upser the Misc Travel Rate Element
        /// </summary>
        /// <param name="inMiscTravelRate"></param>
        /// <returns></returns>
        protected override int? Upsert(MiscTravelRateDTO dtoToUpsert)
        {
            if (dtoToUpsert == null)
            {
                throw new ArgumentNullException(nameof(dtoToUpsert));
            }

            int MISC_TRAVEL_RATE_ID = 0;

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                //Upsert SP
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    MISC_TRAVEL_RATE_ID = Convert.ToInt32(gbe.upsertTravelMiscellaneousRate(dtoToUpsert.Id, dtoToUpsert.MiscTravelRateMode, dtoToUpsert.MiscTravelRate, (byte)dtoToUpsert.SortCode, dtoToUpsert.UpdateDate).SingleOrDefault());
                } // end using gbe
            }

            return MISC_TRAVEL_RATE_ID;
        }

        /// <summary>
        /// Retrieves a locked <see cref="MiscTravelRateDTO"/> from the database
        /// </summary>
        /// <param name="inMiscTravelRateID">The ID of the <see cref="MiscTravelRateDTO"/> being retrieved</param>
        /// <param name="inWorkspaceID">The ID of the <see cref="WorkspaceDTO"/> that is locking the PerDiem rate</param>
        /// <returns>The <see cref="MiscTravelRateDTO"/> requested</returns>
        
        private MiscTravelRateDTO GetWorkspaceLockedMiscTravelRateDTOByID(int inMiscTravelRateID, int inWorkspaceID)
        {
            MiscTravelRateDTO toReturn = new MiscTravelRateDTO();

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                // Get the misctravelrate Data
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    var MiscRate = (from m in gbe.WorkspaceLockedTravelMiscRates
                                    where m.TravelMiscRateID == inMiscTravelRateID && m.WorkspaceID == inWorkspaceID
                                    select new MiscTravelRateDTO
                                    {
                                        Id = m.TravelMiscRateID,
                                        MiscTravelRateMode = m.TransportationMode,
                                        MiscTravelRate = m.MiscellaneousRate,
                                        SortCode = m.SortCode,
                                        inUse = m.MiscRateInUse,
                                        lockedRate = true,
                                        UpdateDate = m.UpdateDT

                                    }).FirstOrDefault();

                    toReturn = MiscRate;
                }
            }

            return toReturn;

        }
    }
}

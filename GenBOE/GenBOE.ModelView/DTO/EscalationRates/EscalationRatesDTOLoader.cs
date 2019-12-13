// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using IES.Common;
    using GenBOE.Dtos;
    using GenBOE.Models;

    public class EscalationRatesDTOLoader : DataLoader<EscalationRatesDTO>, IEscalationRatesDTOLoader
    {
        private Logger log = new Logger(typeof(EscalationRatesDTOLoader));

        /// <summary>
        /// Returns a collection of Escalation Rates DTOs based on the Collection of Ids
        /// </summary>
        /// <param name="ids">Escalation Rate Ids</param>
        /// <returns>The matching DTOs</returns>
        [DbQuery]
        public override ICollection<EscalationRatesDTO> GetByIds(ICollection<int> ids)
        {
            ICollection<EscalationRatesDTO> toReturn = null;

            if (ids != null)
            {
                using (StopwatchTimer sw = new StopwatchTimer(this.log))
                {
                    
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        toReturn = (from x in gbe.TravelEscalationRates
                                    where ids.Contains(x.TravelEscalationRateID)
                                    select new EscalationRatesDTO
                                    {
                                        Year = x.Year,
                                        DevEscalation = x.DevEscalation,
                                        LMSIEscalation = x.LMSIEscalation,
                                        MiscRate = x.MiscRate ?? 0,
                                        UpdateDate = x.UpdateDT,
                                        EscalationRateID = x.TravelEscalationRateID,
                                        LockedRate = false
                                    }).ToCollection();
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// GetAll Escalation Rates
        /// </summary>
        /// <returns></returns>
        [DbQuery]
        public virtual ICollection<EscalationRatesDTO> GetAll()
        {
            Collection<EscalationRatesDTO> toReturn;
            using (StopwatchTimer sw = new StopwatchTimer(this.log))
            {

                // Get All Misc Travel Rates
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from x in gbe.TravelEscalationRates
                                select new EscalationRatesDTO
                                {
                                    Year = x.Year,
                                    DevEscalation = x.DevEscalation,
                                    LMSIEscalation = x.LMSIEscalation,
                                    MiscRate = x.MiscRate ?? 0,
                                    UpdateDate = x.UpdateDT,
                                    EscalationRateID = x.TravelEscalationRateID,
                                    LockedRate = false,
                                    Id = x.TravelEscalationRateID
                                }).ToCollection();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Returns a collection of Escalation Rates for the workspace. Will return locked 
        /// rates depending on workspace state, otherwise the system rates will be returned.
        /// </summary>
        /// <param name="workspace">WorkspaceDTO</param>
        /// <returns>Collection of EscalationRatesDTO</returns>
        public virtual ICollection<EscalationRatesDTO> GetByWorkspace(WorkspaceDTO workspace)
        {
            ICollection<EscalationRatesDTO> toReturn;
            ICollection<EscalationRatesDTO> lockedWsRates = new Collection<EscalationRatesDTO>();
            using (StopwatchTimer sw = new StopwatchTimer(this.log))
            {
                if (workspace == null)
                {
                    throw new ArgumentNullException(nameof(workspace));
                }

                //Determine if System or Workspace rates should be used
                if (workspace.WorkspaceState == WorkspaceState.Locked || workspace.WorkspaceState == WorkspaceState.Closed || workspace.WorkspaceState == WorkspaceState.Complete)
                {
                    //get the WS locked rates
                    lockedWsRates = this.GetByWorkspaceId(workspace.Id);
                }

                if (lockedWsRates.Any() || workspace.WorkspaceState == WorkspaceState.Closed || workspace.WorkspaceState == WorkspaceState.Complete)
                {
                    //Always use locked WS rates in Complete/Closed workspaces
                    //When a WS is locked, there will not be WS rates if it contains an unlocked BOE, so system rates should be used
                    toReturn = lockedWsRates;
                }
                else
                {
                    //Workspace rates are not locked
                    toReturn = this.GetAll();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// This function retrieves the escalation rates associated with a workspace with locked rates.
        /// </summary>
        /// <param name="inWorkspaceID">The workspace Id</param>
        /// <returns>A collection of locked escalation rates</returns>
        private ICollection<EscalationRatesDTO> GetByWorkspaceId(int inWorkspaceID)
        {
            ICollection<EscalationRatesDTO> toReturn;

            using (StopwatchTimer sw = new StopwatchTimer(this.log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {

                    toReturn = (from x in gbe.WorkspaceLockedTravelEscalationRates
                                where x.WorkspaceID == inWorkspaceID
                                select new EscalationRatesDTO
                                {
                                    Year = x.Year,
                                    DevEscalation = x.DevEscalation ?? 0,
                                    LMSIEscalation = x.LMSIEscalation ?? 0,
                                    MiscRate = x.MiscRate ?? 0,
                                    UpdateDate = x.UpdateDT,
                                    EscalationRateID = x.TravelEscalationRateID,
                                    LockedRate = true,
                                    Id = x.TravelEscalationRateID
                                }).ToList().ToCollection();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Delete the Escalation rate
        /// </summary>
        /// <param name="dtoToDelete">Dto that will be deleted.</param>
        /// <returns>
        /// Id of the deleted object
        /// </returns>
        /// <exception cref="System.ArgumentNullException">dtoToDelete</exception>
        protected override int? Delete(EscalationRatesDTO dtoToDelete)
        {
            if (dtoToDelete == null)
            {
                throw new ArgumentNullException(nameof(dtoToDelete));
            }

            int? toReturn;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                // Change to delete travel rate when SP is in.
                toReturn = gbe.deleteTravelEscalationRate(dtoToDelete.EscalationRateID, dtoToDelete.UpdateDate);
            }

            return toReturn;
        }

        /// <summary>
        /// Upser the Escalation Rates
        /// </summary>
        /// <param name="dtoToUpsert">Dto to upsert.</param>
        /// <returns>
        /// Int representing the id of the upserted item.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">dtoToUpsert</exception>
        protected override int? Upsert(EscalationRatesDTO dtoToUpsert)
        {
            if (dtoToUpsert == null)
            {
                throw new ArgumentNullException(nameof(dtoToUpsert));
            }

            int escalationRateID;

            // Upsert SP
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                var resultsLinq = gbe.upsertTravelEscalationRate(dtoToUpsert.EscalationRateID, dtoToUpsert.Year, dtoToUpsert.DevEscalation, dtoToUpsert.LMSIEscalation, 
                    dtoToUpsert.MiscRate, dtoToUpsert.UpdateDate);

                escalationRateID = Convert.ToInt32(resultsLinq.SingleOrDefault());
            }

            return escalationRateID;
        }
    }
}
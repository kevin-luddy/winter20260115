// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using System.Linq;
    using IES.Common;
    using GenBOE.Dtos;
    using GenBOE.Models;
    using System.Collections.ObjectModel;
    using System;

    /// <summary>
    /// The T&M Resource Rate Data Loader Class.
    /// </summary>
    public class TMResourceRateDTODataLoader : DataLoader<TMResourceRateDTO>, ITMResourceRateDTODataLoader
    {
        private Logger _log = new Logger(typeof(TMResourceRateDTODataLoader));

        /// <summary>
        /// Returns a collection of T&M Resource Rates based on the Collection of Ids
        /// </summary>
        /// <param name="ids">T&M Resource Rate Ids</param>
        /// <returns>The matching DTOs</returns>
        [DbQuery]
        public override ICollection<TMResourceRateDTO> GetByIds(ICollection<int> ids)
        {
            ICollection<TMResourceRateDTO> toReturn = null;

            if (ids != null)
            {
                using (StopwatchTimer sw = new StopwatchTimer(this._log))
                {
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        toReturn = ConvertToDto((from x in gbe.TMResourceRates
                                                 where ids.Contains(x.TMResourceRateID)
                                                 select x).ToCollection<TMResourceRate>());
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Gets the T&M resource rates for a workspace.
        /// </summary>
        /// <param name="workspaceId">The workspace identifier</param>
        /// <returns>>A collection of T&M resource rates for a given workspace.</returns>
        [DbQuery]
        public ICollection<TMResourceRateDTO> GetByWorkspaceId(int workspaceId)
        {
            ICollection<TMResourceRateDTO> toReturn = new Collection<TMResourceRateDTO>();
            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    // get the T&M Resource Rate data
                    toReturn = ConvertToDto((from x in gbe.TMResourceRates
                                where x.WorkspaceID == workspaceId
                                select x).ToCollection<TMResourceRate>());
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Converts TMResourceRate entities to TMResourceRateDTOs.
        /// </summary>
        /// <param name="tmResourceRates">TMResourceRate entities.</param>
        /// <returns>Converted TMResourceRateDTOs.</returns>
        private ICollection<TMResourceRateDTO> ConvertToDto(ICollection<TMResourceRate> tmResourceRates)
        {
            ICollection<TMResourceRateDTO> tmResoureRateDtos = new Collection<TMResourceRateDTO>();
            if (tmResourceRates.Any())
            {
                foreach (TMResourceRate entity in tmResourceRates)
                {
                    tmResoureRateDtos.Add(new TMResourceRateDTO
                    {
                        ResourceRateID = entity.TMResourceRateID,
                        WorkspaceID = entity.WorkspaceID,
                        ResourceRate = entity.TMResourceRate1,
                        ResourceID = entity.TMResourceID,
                        StartDate = entity.TMResourceRateStartDate.HasValue ? GenBOEUtilities.AdjustDateTimePrecision(entity.TMResourceRateStartDate.Value, DateTimePrecision.Month) : (DateTime?)null,
                        EndDate = entity.TMResourceRateEndDate.HasValue ? GenBOEUtilities.AdjustDateTimePrecision(entity.TMResourceRateEndDate.Value, DateTimePrecision.Month) : (DateTime?)null,
                        UpdateDate = entity.UpdateDT,
                        LockedRate = false
                    });
                }
            }
            return tmResoureRateDtos;
        }

        /// <summary>
        /// Saves a collection of T&M resource rate DTOs.
        /// </summary>
        /// <param name="inResourceRates">T&M Resource rate DTOs to save.</param>
        /// <returns>Mapping of old Id to new Id.</returns>
        public Dictionary<int, int> SaveTMResourceRates(ICollection<TMResourceRateDTO> inResourceRates)
        {
            if (inResourceRates == null)
            {
                throw new ArgumentNullException(nameof(inResourceRates));
            }

            Dictionary<int, int> toReturn = new Dictionary<int, int>();

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                foreach (TMResourceRateDTO rate in inResourceRates)
                {
                    if (rate.Updateable == UpdateType.None)
                    {
                        throw new ArgumentException("Please supply the Updateable argument");
                    }
                }

                foreach (TMResourceRateDTO tmResourceRate in inResourceRates)
                {
                    if (tmResourceRate.Updateable == UpdateType.Deleted)
                    {
                        Delete(tmResourceRate);
                    }
                    else if (tmResourceRate.Updateable == UpdateType.Upsert)
                    {
                        int originalID = tmResourceRate.ResourceRateID;
                        Upsert(tmResourceRate);
                        toReturn.Add(originalID, tmResourceRate.ResourceRateID);
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Upserts a T&M Resource Rate.
        /// </summary>
        /// <param name="dtoToUpsert">Dto to upsert.</param>
        /// <returns>Id of the upserted item.</returns>
        /// <exception cref="System.ArgumentNullException">dtoToUpsert</exception>
        protected override int? Upsert(TMResourceRateDTO dtoToUpsert)
        {
            if (dtoToUpsert == null)
            {
                throw new ArgumentNullException(nameof(dtoToUpsert));
            }

            int? tmResourceRateId = null;

            // Upsert SP
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                var resultsLinq = gbe.upsertTMResourceRate(
                        dtoToUpsert.ResourceRateID,
                        dtoToUpsert.WorkspaceID,
                        dtoToUpsert.ResourceID,
                        dtoToUpsert.StartDate.HasValue ? GenBOEUtilities.AdjustDateTimePrecision(dtoToUpsert.StartDate.Value, DateTimePrecision.Month) : dtoToUpsert.StartDate,
                        dtoToUpsert.EndDate.HasValue ? GenBOEUtilities.AdjustDateTimePrecision(dtoToUpsert.EndDate.Value, DateTimePrecision.Month) : dtoToUpsert.EndDate,
                        dtoToUpsert.ResourceRate,
                        dtoToUpsert.UpdateDate
                        );

                tmResourceRateId = Convert.ToInt32(resultsLinq.SingleOrDefault());
                dtoToUpsert.ResourceRateID = tmResourceRateId.HasValue ? tmResourceRateId.Value : 0;
            }

            return tmResourceRateId;
        }

        /// <summary>
        /// Deletes a T&M Resource Rate.
        /// </summary>
        /// <param name="dtoToDelete">T&M Resource Rate to delete.</param>
        /// <returns>Id of the deleted item.</returns>
        /// <exception cref="System.ArgumentNullException">dtoToDelete</exception>
        protected override int? Delete(TMResourceRateDTO dtoToDelete)
        {
            if (dtoToDelete == null)
            {
                throw new ArgumentNullException(nameof(dtoToDelete));
            }

            int? toReturn = null;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                toReturn = gbe.deleteTMResourceRate(dtoToDelete.ResourceRateID, dtoToDelete.UpdateDate);
            }

            return toReturn;
        }
    }
}

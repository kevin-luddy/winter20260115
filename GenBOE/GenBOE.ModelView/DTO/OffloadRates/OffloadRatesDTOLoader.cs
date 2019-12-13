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
    using GenBOE.Dtos;
    using GenBOE.Models;
    using IES.Common;

    /// <summary>
    /// Offload Rates DTO Loader.
    /// </summary>
    /// <seealso cref="GenBOE.DataBridge.DTO.IOffloadRatesDTOLoader" />
    public class OffloadRatesDTOLoader : DataLoader<OffloadRatesDTO>, IOffloadRatesDTOLoader
    {
        /// <summary>
        /// The logger
        /// </summary>
        private Logger logger = new Logger(typeof(OffloadRatesDTOLoader));

        /// <summary>
        /// Returns a collection of Offload Rates DTOs based on the Collection of Ids
        /// </summary>
        /// <param name="ids">Offload Rate Ids</param>
        /// <returns>The matching DTOs</returns>
        [DbQuery]
        public override ICollection<OffloadRatesDTO> GetByIds(ICollection<int> ids)
        {
            ICollection<OffloadRatesDTO> toReturn = null;

            if (ids != null)
            {
                using (StopwatchTimer sw = new StopwatchTimer(this.logger))
                {
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        toReturn = (from x in gbe.SystemOffloadRates
                                    where ids.Contains(x.OffloadRateID)
                                    select new OffloadRatesDTO
                                    {
                                        Year = x.Year,
                                        HourlyRate = x.HourlyRate,
                                        Id = x.OffloadRateID,
                                        Percent = x.PercentToOffload,
                                        PerformingOrg = x.PerfOrg,
                                        Resource = x.Resource,
                                        SubResource = x.SubcontractorResource,
                                        UpdateDate = x.UpdateDT
                                    }).ToCollection<OffloadRatesDTO>();
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get All System Offload Rates.
        /// </summary>
        /// <returns>All of the System offload rates</returns>
        [DbQuery]
        virtual public ICollection<OffloadRatesDTO> GetAllSystemRates()
        {
            Collection<OffloadRatesDTO> toReturn = new Collection<OffloadRatesDTO>();
            using (StopwatchTimer sw = new StopwatchTimer(this.logger))
            {
                // Get All Offload Rates
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from x in gbe.SystemOffloadRates
                                select new OffloadRatesDTO
                                {
                                    Year = x.Year,
                                    HourlyRate = x.HourlyRate,
                                    Id = x.OffloadRateID,
                                    Percent = x.PercentToOffload,
                                    PerformingOrg = x.PerfOrg,
                                    Resource = x.Resource,
                                    SubResource = x.SubcontractorResource,
                                    UpdateDate = x.UpdateDT
                                }).ToCollection();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// This function retrieves the offload rates associated with a workspace.
        /// </summary>
        /// <param name="workspaceId">The workspace identifier</param>
        /// <returns>A collection of offload rates</returns>
        public ICollection<OffloadRatesDTO> GetByWorkspaceId(int workspaceId)
        {
            ICollection<OffloadRatesDTO> toReturn = new Collection<OffloadRatesDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this.logger))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from x in gbe.WorkspaceOffloadRates
                                where x.WorkspaceID == workspaceId
                                select new OffloadRatesDTO
                                {
                                    Year = x.Year,
                                    HourlyRate = x.HourlyRate,
                                    Id = x.OffloadRateID,
                                    Percent = x.PercentToOffload,
                                    PerformingOrg = x.PerfOrg,
                                    Resource = x.Resource,
                                    SubResource = x.SubcontractorResource,
                                    UpdateDate = x.UpdateDT
                                }).ToList().ToCollection<OffloadRatesDTO>();
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Copies the system default offload rates.
        /// </summary>
        /// <param name="workspaceId">The workspace identifier.</param>
        public void CopySystemDefaultOffloadRates(int workspaceId)
        {
            if (workspaceId < 1)
            {
                throw new ArgumentException("workspaceId must be a positive integer.");
            }

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                gbe.resetWorkspaceOffloadRates(workspaceId);
            }
        }

        /// <summary>
        /// Determines if the workspace rates are out of date compared to system rates.
        /// </summary>
        /// <param name="workspaceId">The workspace Id to check.</param>
        /// <returns>True if rates are out of date, false if they are up to date</returns>
        public bool AreCurrentOffloadRatesOutOfDate(int workspaceId)
        {
            ICollection<OffloadRatesDTO> workspaceRates = this.GetByWorkspaceId(workspaceId);
            ICollection<OffloadRatesDTO> systemRates = this.GetAllSystemRates();

            if (workspaceRates.Count != systemRates.Count)
            {
                // If there is a difference in rate count, rates are out of date, return true
                return true;
            }

            foreach (OffloadRatesDTO workspaceRate in workspaceRates)
            {
                OffloadRatesDTO systemRate = systemRates.FirstOrDefault(s => s.Year == workspaceRate.Year && s.PerformingOrg == workspaceRate.PerformingOrg && s.Resource == workspaceRate.Resource);
                if (systemRate == null || systemRate.UpdateDate != workspaceRate.UpdateDate || systemRate.HourlyRate != workspaceRate.HourlyRate || systemRate.Percent != workspaceRate.Percent
                    || systemRate.SubResource != workspaceRate.SubResource)
                {
                    // If there is a difference in any field in at least one rate, rates are out of date, return true
                    return true;
                }
            }

            // No differences, so rates are current, return false
            return false;
        }

        /// <summary>
        /// Delete the System Offload rate.
        /// </summary>
        /// <param name="dtoToDelete">Dto that will be deleted.</param>
        /// <returns>
        /// Id of the deleted object
        /// </returns>
        /// <exception cref="System.ArgumentNullException">dtoToDelete</exception>
        protected override int? Delete(OffloadRatesDTO dtoToDelete)
        {
            if (dtoToDelete == null)
            {
                throw new ArgumentNullException(nameof(dtoToDelete));
            }

            int? toReturn = null;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                toReturn = gbe.deleteSystemOffloadRate(dtoToDelete.Id, dtoToDelete.UpdateDate);
            }

            return toReturn;
        }

        /// <summary>
        /// Upsert the System Offload Rate.
        /// </summary>
        /// <param name="dtoToUpsert">Dto to upsert.</param>
        /// <returns>
        /// Int representing the id of the upserted item.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">dtoToUpsert</exception>
        protected override int? Upsert(OffloadRatesDTO dtoToUpsert)
        {
            if (dtoToUpsert == null)
            {
                throw new ArgumentNullException(nameof(dtoToUpsert));
            }

            int offloadRateId = 0;

            // Upsert SP
            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                var resultsLinq = gbe.upsertSystemOffloadRate(dtoToUpsert.Id, dtoToUpsert.Resource, dtoToUpsert.PerformingOrg, dtoToUpsert.Percent, dtoToUpsert.Year,
                    dtoToUpsert.SubResource, dtoToUpsert.HourlyRate, dtoToUpsert.UpdateDate);

                offloadRateId = Convert.ToInt32(resultsLinq.SingleOrDefault());
            }

            return offloadRateId;
        }
    }
}
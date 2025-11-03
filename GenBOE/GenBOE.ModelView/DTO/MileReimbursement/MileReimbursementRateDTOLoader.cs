// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System;
    using System.Linq;
    using IES.Common;
    using GenBOE.Dtos;
    using GenBOE.Models;

    public class MileReimbursementRateDTOLoader : IMileReimbursementRateDTOLoader
    {
        private Logger _log = new Logger(typeof(MileReimbursementRateDTOLoader));

        /// <summary>
        /// Saves an updated Mile Reimbursement Rate
        /// </summary>
        /// <param name="inMileReimbursementRateDTO">The Rate DTO</param>
        public virtual void SaveMileReimbursementRateDTO(MileReimbursementRateDTO inMileReimbursementRateDTO)
        {
            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                if (inMileReimbursementRateDTO == null)
                {
                    throw new ArgumentNullException(nameof(inMileReimbursementRateDTO));
                }

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    gbe.updateMileageReimbursementRate(inMileReimbursementRateDTO.Id, inMileReimbursementRateDTO.MileReimbursementRate, inMileReimbursementRateDTO.UpdateDate);
                }
            }
        }

        /// <summary>
        /// Gets the rate for the system
        /// </summary>
        /// <returns>The rate for the system</returns>
        [DbQuery]
        public virtual MileReimbursementRateDTO GetMileReimbursementRateDTO()
        {
            MileReimbursementRateDTO toReturn = null;
            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {

					MileReimbursementRateDTO mileRate = (from o in gbe.MileageReimbursementRates
                                    select new MileReimbursementRateDTO
                                    {
                                        MileReimbursementRate = o.RatePerMile,
                                        Id = o.MileageReimbursementRateID,
                                        UpdateDate = o.UpdateDT
                                    }).FirstOrDefault();

                    toReturn = mileRate;
                }

                if (toReturn == null)
                {
                    toReturn = new MileReimbursementRateDTO()
                    {
                        MileReimbursementRate = 0,
                        Id = -1
                    };
                }
            }

            return toReturn;
        }       
    }
}
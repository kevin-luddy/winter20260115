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
    using GenBOE.Models;
    using GenBOE.Dtos;

    public class OtherDirectCostDTODataLoader : IOtherDirectCostDTODataLoader
    {
        private readonly ILogger _log;

        /// <summary>
        /// Constructor
        /// </summary>
        public OtherDirectCostDTODataLoader(ILogger logger)
		{
			this._log = logger;
		}

        #region Retrieves

        /// <summary>
        /// get ODC by ODC ID
        /// </summary>
        /// <param name="inODCID">ODC Id</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <returns>Corresponding data</returns>
        virtual public OtherDirectCostDTO GetById(int inODCID, bool includeRTEFields = false)
        {
            return this.GetByIds(new List<int>() { inODCID }).FirstOrDefault();
        }

        
        virtual public ICollection<OtherDirectCostDTO> GetByIds(ICollection<int> odcIds, bool includeRTEFields = false)
        {
            List<OtherDirectCostDTO> result = new List<OtherDirectCostDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                // Get the ODC Data
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    result = (from o in gbe.ODCTaskElements
                              where odcIds.Contains(o.ODCTaskElementID)
                              select new OtherDirectCostDTO
                              {
                                  // 2 RTE fields:
                                  TaskDescription = includeRTEFields ? o.ODCTaskDescription : null,
                                  MoqText = includeRTEFields ? o.ODCMOQText : null,
                                  WasMoqTextSet = includeRTEFields,
                                  WasDescriptionSet = includeRTEFields,

                                  Id = o.ODCTaskElementID,
                                  TaskID = o.ODCTaskID,
                                  TaskTitle = o.ODCTaskTitle,
                                  UpdateDate = o.UpdateDT,
                                  BoeID = o.BOEID,
                                  StartDate = o.TaskStartDate,
                                  EndDate = o.TaskEndDate,
                                  BOETaskElementOrder = o.SortOrderID
                              }).ToList();

                    var ODCTypes = gbe.ODCTypes.Where(oT => odcIds.Contains(oT.ODCTaskElementID)).OrderBy(oT => oT.ODCTypeID).Select(oT =>
                        new { Key = oT.ODCTaskElementID,
                              Value = new OtherDirectCostType
                                            {
                                                ODCTypeID = oT.ODCTypeID,
                                                ResourceID = oT.ResourceID,
                                                PerformingOrgID = oT.PerformingOrganizationID,
                                                SpreadCurve = (SpreadCurves)oT.SpreadCurveID,
                                                Cost = oT.ODCTypeCost,
                                                UpdateDate = oT.UpdateDT,
                                                StartDate = oT.ODCTypeStartDate,
                                                EndDate = oT.ODCTypeEndDate,

                                                ODCSpreadsIEnum = gbe.ODCSpreads.Where(oS => oS.ODCTypeID == oT.ODCTypeID).OrderBy(oS => oS.ODCSpreadDate).Select(oS =>
                                                                        new OtherDirectCostSpread
                                                                        {
                                                                            ODCSpreadID = oS.ODCSpreadID,
                                                                            ODCSpreadDate = oS.ODCSpreadDate,
                                                                            CostSpreadValue = oS.ODCSpreadValue
                                                                        })
                                            }}
                                  ).ToList();

                    result.ForEach(
                            x =>
                            {
                                x.StartDate = x.StartDate.HasValue ? GenBOEUtilities.AdjustDateTimePrecision((DateTime)x.StartDate, DateTimePrecision.Month) : x.ODCTypes.Min(z => z.StartDate);
                                x.EndDate = x.EndDate.HasValue ? GenBOEUtilities.AdjustDateTimePrecision((DateTime)x.EndDate, DateTimePrecision.Month) : x.ODCTypes.Max(z => z.EndDate);

                                x.ODCTypes = ODCTypes.Where(z => z.Key == x.Id).Select(z => z.Value).ToCollection();
                                x.ODCTypes.ToList().ForEach(oT =>
                                {
                                    oT.ODCSpreads = oT.ODCSpreadsIEnum.ToCollection();
                                    oT.ODCSpreadsIEnum = null;
                                    oT.ODCSpreads.ToList().ForEach(oS => { oS.BoeID = x.BoeID; });
                                });
                            }
                        );
                }
            }

            return result;
        }

        /// <summary>
        /// Gets data by Boe Ids
        /// </summary>
        /// <param name="odcIds">Odc Ids</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <returns>Corresponding data</returns>
        
        virtual public ICollection<OtherDirectCostDTO> GetByBoeIds(ICollection<int> boeIds, bool includeRTEFields = false)
        {
            List<OtherDirectCostDTO> result = new List<OtherDirectCostDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                // Get the ODC Data
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    result = (from o in gbe.ODCTaskElements
                              where boeIds.Contains(o.BOEID)
                              select new OtherDirectCostDTO
                              {
                                  // 2 RTE fields:
                                  TaskDescription = includeRTEFields ? o.ODCTaskDescription : null,
                                  MoqText = includeRTEFields ? o.ODCMOQText : null,
                                  WasMoqTextSet = includeRTEFields,
                                  WasDescriptionSet = includeRTEFields,

                                  Id = o.ODCTaskElementID,
                                  TaskID = o.ODCTaskID,
                                  TaskTitle = o.ODCTaskTitle,
                                  UpdateDate = o.UpdateDT,
                                  BoeID = o.BOEID,
                                  StartDate = o.TaskStartDate,
                                  EndDate = o.TaskEndDate,
                                  BOETaskElementOrder = o.SortOrderID
                              }).ToList();

                    List<int> odcIds = result.Select(o => o.Id).Distinct().ToList();

                    var ODCTypes = gbe.ODCTypes.Where(oT => odcIds.Contains(oT.ODCTaskElementID)).OrderBy(oT => oT.ODCTypeID).Select(oT =>
                        new
                        {
                            Key = oT.ODCTaskElementID,
                            Value = new OtherDirectCostType
                            {
                                ODCTypeID = oT.ODCTypeID,
                                ResourceID = oT.ResourceID,
                                PerformingOrgID = oT.PerformingOrganizationID,
                                SpreadCurve = (SpreadCurves)oT.SpreadCurveID,
                                Cost = oT.ODCTypeCost,
                                UpdateDate = oT.UpdateDT,
                                StartDate = oT.ODCTypeStartDate,
                                EndDate = oT.ODCTypeEndDate,

                                ODCSpreadsIEnum = gbe.ODCSpreads.Where(oS => oS.ODCTypeID == oT.ODCTypeID).OrderBy(oS => oS.ODCSpreadDate).Select(oS =>
                                                        new OtherDirectCostSpread
                                                        {
                                                            ODCSpreadID = oS.ODCSpreadID,
                                                            ODCSpreadDate = oS.ODCSpreadDate,
                                                            CostSpreadValue = oS.ODCSpreadValue
                                                        })
                            }
                        }).ToList();

                    result.ForEach(
                            x =>
                            {
                                x.StartDate = x.StartDate.HasValue ? GenBOEUtilities.AdjustDateTimePrecision((DateTime)x.StartDate, DateTimePrecision.Month) : x.ODCTypes.Min(z => z.StartDate);
                                x.EndDate = x.EndDate.HasValue ? GenBOEUtilities.AdjustDateTimePrecision((DateTime)x.EndDate, DateTimePrecision.Month) : x.ODCTypes.Max(z => z.EndDate);

                                x.ODCTypes = ODCTypes.Where(z => z.Key == x.Id).Select(z => z.Value).ToCollection();
                                x.ODCTypes.ToList().ForEach(oT =>
                                {
                                    oT.ODCSpreads = oT.ODCSpreadsIEnum.ToCollection();
                                    oT.ODCSpreadsIEnum = null;
                                    oT.ODCSpreads.ToList().ForEach(oS => { oS.BoeID = x.BoeID; });
                                });
                            }
                        );
                }
            }

            return result;
        }

        #region RTE Load Methods

        /// <summary>
        /// Pulls RTE fields for the DTOs, and updates them as needed
        /// </summary>
        /// <param name="dtos">DTOs that whose RTE fields will be loaded, if they are null</param>
        
        public void LoadRTEFields(ICollection<OtherDirectCostDTO> dtos)
        {
            if (dtos == null || !dtos.Any()) { return; }

            List<RteFieldsHelper> dataFromDb = null;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                // Only want to pull RTE for an object when:
                //   - the object exists (Id > 0)
                //   - at least one of the RTE fields has not been inserted into, or retrieved already
                List<int> dtoIds = dtos.Where(x => x.Id > 0 && (!x.WasMoqTextSet || !x.WasDescriptionSet) && x.Updateable != UpdateType.Deleted).Select(x => x.Id).ToList();

                if (dtoIds.Any())
                {
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        gbe.Database.CommandTimeout = 360;  // give queries enough time to execute

                        // get the basic BOE data from the sprocResults
                        dataFromDb = (from o in gbe.ODCTaskElements
                                      where dtoIds.Contains(o.ODCTaskElementID)
                                      select new RteFieldsHelper
                                      {
                                          Id = o.ODCTaskElementID,
                                          Description = o.ODCTaskDescription,
                                          MoqText = o.ODCMOQText
                                      }).ToList();
                    }

                    dataFromDb.AsParallel().ForAll(dbData =>
                    {
                        OtherDirectCostDTO dto = dtos.First(b => b.Id == dbData.Id);

                        // We do not want to overwrite existing data during, so we only fill missing data, if available
                        dto.TaskDescription = dto.WasDescriptionSet ? dto.TaskDescription : dbData.Description;
                        dto.MoqText = dto.WasMoqTextSet ? dto.MoqText : dbData.MoqText;
                    });
                }
            }
        }

        #endregion

        #endregion
    }
}
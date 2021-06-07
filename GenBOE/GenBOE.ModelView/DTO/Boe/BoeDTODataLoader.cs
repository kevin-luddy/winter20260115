// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
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
    using static IES.Common.Constants;

    public class BoeDTODataLoader : DataLoader<BoeDTO>, IBoeDTODataLoader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BoeDTODataLoader()
        {
            this.Log = new Logger(typeof(BoeDTODataLoader));
        }

        #region Retrieves

        #region Multi DbQuery Methods

        /// <summary>
        /// Get BOE ids by CLIN ids
        /// </summary>
        /// <param name="clinIds">CLIN ids</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <returns>Collection of BOE Ids</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        [DbQuery(2)]
        virtual public ICollection<BoeDTO> GetByClinIds(ICollection<int> clinIds, bool includeRTEFields = false)
        {
            // The BOEDTO to return
            List<BoeDTO> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    gbe.Database.CommandTimeout = 360;  // give queries enough time to execute

                    // get the basic BOE data from the sprocResults
                    toReturn = (from b in gbe.BOEs
                                join xRef in gbe.WBS_CLIN_BOE_XREF.Where(x => clinIds.Contains(x.CLINID.Value)) on b.BOEID equals xRef.BOEID
                                orderby b.BOEID
                                select new BoeDTO
                                {
                                    // 2 RTE fields:
                                    Description = includeRTEFields ? b.BOEDescription : null,
                                    DataSource = includeRTEFields ? b.DataSource : null,
                                    WasDataSourceSet = includeRTEFields,
                                    WasDescriptionSet = includeRTEFields,

                                    Id = b.BOEID,
                                    StartDate = b.BOEStartDate,
                                    EndDate = b.BOEEndDate,
                                    Title = b.BOETitle,
                                    State = (BOEState)b.BOEStateID,
                                    UpdateDate = b.UpdateDT,
                                    WorkspaceID = b.WorkspaceID,
                                    NumAuthorReassigned = b.NumAuthorReassigned,
                                    HistoricMetricDisclosureChecked = b.MetricDisclosureAcknowledge,
                                    isMaterial = b.IsMaterial,
                                    IsMultiClinWbs = b.IsMultiClinWbs,
                                    // Custom Fields
                                    CustomFieldValueContainers = gbe.BOECustomFieldValueXREFs.Where(cf => cf.BOEID == b.BOEID)
                                                .Select(cf => new CustomFieldValueContainer
                                                {
                                                    ContainerID = cf.BCFVID,
                                                    CustomFieldValueID = cf.CustomFieldValueID,
                                                    UpdateDate = cf.UpdateDT,
                                                    IsOpenEnded = cf.CustomFieldValue.CustomField.IsOpenEnded,
                                                    OpenEndedValue = cf.CustomFieldValue.CustomFieldValueDescription,
                                                    CustomFieldID = cf.CustomFieldValue.CustomFieldID
                                                }).ToList(),
                                    CLINID = xRef == null ? null : xRef.CLINID,
                                    WBSID = xRef == null ? null : xRef.WBSID,
                                    WCBID = xRef == null ? null : (int?)xRef.WCBID,

                                    // Approval date based on the State History..
                                    // If it's awaiting approval, we use updated boe state id column, otherwise, if it's approved, we use CurrentBoeStateId
                                    SubmitForApprovalDate = b.BOEStateID == (int)BOEState.AwaitingApproval
                                        ? gbe.BOEStateHistories.Where(sH => sH.UpdatedBOEStateID == (int)BOEState.AwaitingApproval && sH.BOEID == b.BOEID).Select(sH => sH.UpdateDT).Max()
                                        : (b.BOEStateID == (int)BOEState.Approved
                                            ? gbe.BOEStateHistories.Where(sH => sH.CurrentBOEStateID == (int)BOEState.AwaitingApproval && sH.BOEID == b.BOEID).Select(sH => sH.UpdateDT).Max()
                                            : DateTime.MinValue)
                                }).ToList();


                    // pulling these inside of the query above complicated the generated SQL greatly - quadrupled the length of it
                    var users = gbe.BOEUserRoles
                        .Where(uR =>
                            (uR.RoleID == (int)Role.Author || uR.RoleID == (int)Role.SubcontractorAuthor)
                            && gbe.WBS_CLIN_BOE_XREF.Where(xRef => xRef.BOEID == uR.BOEID && clinIds.Contains(xRef.CLINID.Value)).Any())
                        .Select(uR => new { uR.BOEID, uR.RoleID, uR.ETIUserID }).ToList();

                    toReturn.ForEach(
                            x =>
                            {
                                x.StartDate = x.StartDate.Normalize();
                                x.EndDate = x.EndDate.Normalize();
                                x.SubmitForApprovalDate = x.SubmitForApprovalDate.Normalize(DateTimePrecision.Day);

                                x.AuthorIDs = users.Where(z => z.RoleID == (int)Role.Author && z.BOEID == x.Id).Select(z => z.ETIUserID).ToCollection();
                                x.SubcontractorAuthorIDs = users.Where(z => z.RoleID == (int)Role.SubcontractorAuthor && z.BOEID == x.Id).Select(z => z.ETIUserID).ToCollection();
                            }
                        );

                    this.LoadSikorskyFields(gbe, toReturn);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Gets BOEs by Wbs Id
        /// </summary>
        /// <param name="wbsId">Wbs Id</param>
        /// <returns>Corresponding Boe Elements</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        virtual public ICollection<BoeDTO> GetByWbsId(int wbsId)
        {
            // The BOEDTO to return
            List<BoeDTO> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    gbe.Database.CommandTimeout = 360;  // give queries enough time to execute

                    // get the basic BOE data from the sprocResults
                    toReturn = (from b in gbe.BOEs
                                join xRef in gbe.WBS_CLIN_BOE_XREF.Where(x => x.WBSID == wbsId) on b.BOEID equals xRef.BOEID
                                orderby b.BOEID
                                select new BoeDTO
                                {
                                    // 2 RTE fields:
                                    Description = b.BOEDescription,
                                    DataSource = b.DataSource,
                                    WasDataSourceSet = true,
                                    WasDescriptionSet = true,

                                    Id = b.BOEID,
                                    StartDate = b.BOEStartDate,
                                    EndDate = b.BOEEndDate,
                                    Title = b.BOETitle,
                                    State = (BOEState)b.BOEStateID,
                                    UpdateDate = b.UpdateDT,
                                    WorkspaceID = b.WorkspaceID,
                                    NumAuthorReassigned = b.NumAuthorReassigned,
                                    HistoricMetricDisclosureChecked = b.MetricDisclosureAcknowledge,
                                    isMaterial = b.IsMaterial,
                                    IsMultiClinWbs = b.IsMultiClinWbs,
                                    // Custom Fields
                                    CustomFieldValueContainers = gbe.BOECustomFieldValueXREFs.Where(cf => cf.BOEID == b.BOEID)
                                                .Select(cf => new CustomFieldValueContainer
                                                {
                                                    ContainerID = cf.BCFVID,
                                                    CustomFieldValueID = cf.CustomFieldValueID,
                                                    UpdateDate = cf.UpdateDT,
                                                    IsOpenEnded = cf.CustomFieldValue.CustomField.IsOpenEnded,
                                                    OpenEndedValue = cf.CustomFieldValue.CustomFieldValueDescription,
                                                    CustomFieldID = cf.CustomFieldValue.CustomFieldID
                                                }).ToList(),
                                    CLINID = xRef == null ? null : xRef.CLINID,
                                    WBSID = xRef == null ? null : xRef.WBSID,
                                    WCBID = xRef == null ? null : (int?)xRef.WCBID,

                                    // Approval date based on the State History..
                                    // If it's awaiting approval, we use updated boe state id column, otherwise, if it's approved, we use CurrentBoeStateId
                                    SubmitForApprovalDate = b.BOEStateID == (int)BOEState.AwaitingApproval
                                        ? gbe.BOEStateHistories.Where(sH => sH.UpdatedBOEStateID == (int)BOEState.AwaitingApproval && sH.BOEID == b.BOEID).Select(sH => sH.UpdateDT).Max()
                                        : (b.BOEStateID == (int)BOEState.Approved
                                            ? gbe.BOEStateHistories.Where(sH => sH.CurrentBOEStateID == (int)BOEState.AwaitingApproval && sH.BOEID == b.BOEID).Select(sH => sH.UpdateDT).Max()
                                            : DateTime.MinValue)
                                }).ToList();


                    // pulling these inside of the query above complicated the generated SQL greatly - quadrupled the length of it
                    var users = gbe.BOEUserRoles
                        .Where(uR =>
                            (uR.RoleID == (int)Role.Author || uR.RoleID == (int)Role.SubcontractorAuthor)
                            && gbe.WBS_CLIN_BOE_XREF.Where(xRef => xRef.BOEID == uR.BOEID && xRef.WBSID == wbsId).Any())
                        .Select(uR => new { uR.BOEID, uR.RoleID, uR.ETIUserID }).ToList();

                    toReturn.ForEach(
                            x =>
                            {
                                x.StartDate = x.StartDate.Normalize();
                                x.EndDate = x.EndDate.Normalize();
                                x.SubmitForApprovalDate = x.SubmitForApprovalDate.Normalize(DateTimePrecision.Day);

                                x.AuthorIDs = users.Where(z => z.RoleID == (int)Role.Author && z.BOEID == x.Id).Select(z => z.ETIUserID).ToCollection();
                                x.SubcontractorAuthorIDs = users.Where(z => z.RoleID == (int)Role.SubcontractorAuthor && z.BOEID == x.Id).Select(z => z.ETIUserID).ToCollection();
                            }
                        );

                    this.LoadSikorskyFields(gbe, toReturn);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get Boes by their Ids
        /// </summary>
        /// <param name="ids">Ids of the BOEs to retrieve</param>
        /// <returns>Collection of BOE DTOs</returns>
        public override ICollection<BoeDTO> GetByIds(ICollection<int> ids)
        {
            return this.GetByIds(ids, false);
        }

        /// <summary>
        /// Get Boes by their Ids
        /// </summary>
        /// <param name="ids">Ids of the BOEs to retrieve</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <returns>Collection of BOE DTOs</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        [DbQuery(2)]
        public ICollection<BoeDTO> GetByIds(ICollection<int> ids, bool includeRTEFields = false)
        {
            List<BoeDTO> toReturn = null;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                gbe.Database.CommandTimeout = 360;  // give queries enough time to execute

                // get the basic BOE data from the sprocResults
                toReturn = (from b in gbe.BOEs
                            where ids.Contains(b.BOEID)
                            join xRef in gbe.WBS_CLIN_BOE_XREF.Where(x => x.BOEID.HasValue) on b.BOEID equals xRef.BOEID into temp
                            from xRef in temp.DefaultIfEmpty() // left outer joins for above..
                            orderby b.BOEID
                            select new BoeDTO
                            {
                                // 2 RTE fields:
                                Description = includeRTEFields ? b.BOEDescription : null,
                                DataSource = includeRTEFields ? b.DataSource : null,
                                WasDataSourceSet = includeRTEFields,
                                WasDescriptionSet = includeRTEFields,

                                Id = b.BOEID,
                                StartDate = b.BOEStartDate,
                                EndDate = b.BOEEndDate,
                                Title = b.BOETitle,
                                State = (BOEState)b.BOEStateID,
                                UpdateDate = b.UpdateDT,
                                WorkspaceID = b.WorkspaceID,
                                NumAuthorReassigned = b.NumAuthorReassigned,
                                HistoricMetricDisclosureChecked = b.MetricDisclosureAcknowledge,
                                isMaterial = b.IsMaterial,
                                IsMultiClinWbs = b.IsMultiClinWbs,
                                // Custom Fields
                                CustomFieldValueContainers = gbe.BOECustomFieldValueXREFs.Where(cf => cf.BOEID == b.BOEID)
                                            .Select(cf => new CustomFieldValueContainer
                                            {
                                                ContainerID = cf.BCFVID,
                                                CustomFieldValueID = cf.CustomFieldValueID,
                                                UpdateDate = cf.UpdateDT,
                                                IsOpenEnded = cf.CustomFieldValue.CustomField.IsOpenEnded,
                                                OpenEndedValue = cf.CustomFieldValue.CustomFieldValueDescription,
                                                CustomFieldID = cf.CustomFieldValue.CustomFieldID
                                            }).ToList(),
                                CLINID = xRef == null ? null : xRef.CLINID,
                                WBSID = xRef == null ? null : xRef.WBSID,
                                WCBID = xRef == null ? null : (int?)xRef.WCBID,

                                // Approval date based on the State History..
                                // If it's awaiting approval, we use updated boe state id column, otherwise, if it's approved, we use CurrentBoeStateId
                                SubmitForApprovalDate = b.BOEStateID == (int)BOEState.AwaitingApproval
                                    ? gbe.BOEStateHistories.Where(sH => sH.UpdatedBOEStateID == (int)BOEState.AwaitingApproval && sH.BOEID == b.BOEID).Select(sH => sH.UpdateDT).Max()
                                    : (b.BOEStateID == (int)BOEState.Approved
                                        ? gbe.BOEStateHistories.Where(sH => sH.CurrentBOEStateID == (int)BOEState.AwaitingApproval && sH.BOEID == b.BOEID).Select(sH => sH.UpdateDT).Max()
                                        : DateTime.MinValue)
                            }).ToList();

                // pulling these inside of the query above complicated the generated SQL greatly - quadrupled the length of it
                var users = gbe.BOEUserRoles
                    .Where(uR => (uR.RoleID == (int)Role.Author || uR.RoleID == (int)Role.SubcontractorAuthor) && ids.Contains(uR.BOEID))
                    .Select(uR => new { uR.BOEID, uR.RoleID, uR.ETIUserID }).ToList();

                toReturn.ForEach(
                        x =>
                        {
                            x.StartDate = x.StartDate.Normalize();
                            x.EndDate = x.EndDate.Normalize();
                            x.SubmitForApprovalDate = x.SubmitForApprovalDate.Normalize(DateTimePrecision.Day);

                            x.AuthorIDs = users.Where(z => z.RoleID == (int)Role.Author && z.BOEID == x.Id).Select(z => z.ETIUserID).ToCollection();
                            x.SubcontractorAuthorIDs = users.Where(z => z.RoleID == (int)Role.SubcontractorAuthor && z.BOEID == x.Id).Select(z => z.ETIUserID).ToCollection();
                        }
                    );

                this.LoadSikorskyFields(gbe, toReturn);
            }

            return toReturn;
        }

        /// <summary>
        /// Gets all BOEs for the workspace
        /// </summary>
        /// <param name="workspaceId">Workspace Id</param>
        /// <param name="includeRTEFields">Indicates whether RTE fields should be retrieved as a part of the data pull</param>
        /// <returns>BOEs for the specified WS</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        [DbQuery(2)]
        public ICollection<BoeDTO> GetByWorkspaceId(int workspaceId, bool includeRTEFields = false)
        {
            // The BOEDTO to return
            List<BoeDTO> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    gbe.Database.CommandTimeout = 360;  // give queries enough time to execute

                    // get the basic BOE data from the sprocResults
                    toReturn = (from b in gbe.BOEs
                                where b.WorkspaceID == workspaceId
                                join xRef in gbe.WBS_CLIN_BOE_XREF.Where(x => x.BOEID.HasValue) on b.BOEID equals xRef.BOEID into temp
                                from xRef in temp.DefaultIfEmpty() // left outer joins for above..
                                orderby b.BOEID
                                select new BoeDTO
                                {
                                    // 2 RTE fields:
                                    Description = includeRTEFields ? b.BOEDescription : null,
                                    DataSource = includeRTEFields ? b.DataSource : null,
                                    WasDataSourceSet = includeRTEFields,
                                    WasDescriptionSet = includeRTEFields,

                                    Id = b.BOEID,
                                    StartDate = b.BOEStartDate,
                                    EndDate = b.BOEEndDate,
                                    Title = b.BOETitle,
                                    State = (BOEState)b.BOEStateID,
                                    UpdateDate = b.UpdateDT,
                                    WorkspaceID = b.WorkspaceID,
                                    NumAuthorReassigned = b.NumAuthorReassigned,
                                    HistoricMetricDisclosureChecked = b.MetricDisclosureAcknowledge,
                                    isMaterial = b.IsMaterial,
                                    IsMultiClinWbs = b.IsMultiClinWbs,
                                    // Custom Fields
                                    CustomFieldValueContainers = gbe.BOECustomFieldValueXREFs.Where(cf => cf.BOEID == b.BOEID)
                                                .Select(cf => new CustomFieldValueContainer
                                                {
                                                    ContainerID = cf.BCFVID,
                                                    CustomFieldValueID = cf.CustomFieldValueID,
                                                    UpdateDate = cf.UpdateDT,
                                                    IsOpenEnded = cf.CustomFieldValue.CustomField.IsOpenEnded,
                                                    OpenEndedValue = cf.CustomFieldValue.CustomFieldValueDescription,
                                                    CustomFieldID = cf.CustomFieldValue.CustomFieldID
                                                }).ToList(),
                                    CLINID = xRef == null ? null : xRef.CLINID,
                                    WBSID = xRef == null ? null : xRef.WBSID,
                                    WCBID = xRef == null ? null : (int?)xRef.WCBID,

                                    // Approval date based on the State History..
                                    // If it's awaiting approval, we use updated boe state id column, otherwise, if it's approved, we use CurrentBoeStateId
                                    SubmitForApprovalDate = b.BOEStateID == (int)BOEState.AwaitingApproval
                                        ? gbe.BOEStateHistories.Where(sH => sH.UpdatedBOEStateID == (int)BOEState.AwaitingApproval && sH.BOEID == b.BOEID).Select(sH => sH.UpdateDT).Max()
                                        : (b.BOEStateID == (int)BOEState.Approved
                                            ? gbe.BOEStateHistories.Where(sH => sH.CurrentBOEStateID == (int)BOEState.AwaitingApproval && sH.BOEID == b.BOEID).Select(sH => sH.UpdateDT).Max()
                                            : DateTime.MinValue)
                                }).ToList();

                    // pulling these inside of the query above complicated the generated SQL greatly - quadrupled the length of it
                    var users = gbe.BOEUserRoles
                        .Where(uR =>
                            (uR.RoleID == (int)Role.Author || uR.RoleID == (int)Role.SubcontractorAuthor)
                            && gbe.BOEs.Where(boe => boe.WorkspaceID == workspaceId && boe.BOEID == uR.BOEID).Any())
                        .Select(uR => new { uR.BOEID, uR.RoleID, uR.ETIUserID }).ToList();

                    toReturn.ForEach(
                            x =>
                            {
                                x.StartDate = x.StartDate.Normalize();
                                x.EndDate = x.EndDate.Normalize();
                                x.SubmitForApprovalDate = x.SubmitForApprovalDate.Normalize(DateTimePrecision.Day);

                                x.AuthorIDs = users.Where(z => z.RoleID == (int)Role.Author && z.BOEID == x.Id).Select(z => z.ETIUserID).ToCollection();
                                x.SubcontractorAuthorIDs = users.Where(z => z.RoleID == (int)Role.SubcontractorAuthor && z.BOEID == x.Id).Select(z => z.ETIUserID).ToCollection();
                            }
                        );

                    this.LoadSikorskyFields(gbe, toReturn);
                }
            }

            return toReturn;
        }

        #endregion

        #region Sikorsky / Project Map Custom Fields

        /// <summary>
        /// Loads Sikorsky Custom Fields into BOE properties
        /// </summary>
        /// <param name="gbe">GenBOE Entities connected to the DB</param>
        /// <param name="boesToLoad">Boes to load</param>
        [DbQuery]
        private void LoadSikorskyFields(GenBoeEntities gbe, List<BoeDTO> boesToLoad)
        {
            List<int> boeIds = boesToLoad.Select(z => z.Id).ToList();

            var sikorskyCfData = gbe.BOECustomFieldValueXREFs.Where(x => boeIds.Contains(x.BOEID)).Select(x => new {
                CfName = x.CustomFieldValue.CustomFieldValueName,
                CfValue = x.CustomFieldValue.CustomFieldValueDescription,
                FieldName = x.CustomFieldValue.CustomField.CustomFieldName.ToUpper(),
                BoeId = x.BOEID
            }).ToList();

            boesToLoad.ForEach(boe => {
                boe.SOW = sikorskyCfData.FirstOrDefault(x => x.BoeId == boe.Id && x.FieldName == SikorskyConstants.SIKORSKY_CF_SOW.ToUpper())?.CfName;
                boe.SOWTitle = sikorskyCfData.FirstOrDefault(x => x.BoeId == boe.Id && x.FieldName == SikorskyConstants.SIKORSKY_CF_SOW.ToUpper())?.CfValue;
                boe.Category = sikorskyCfData.FirstOrDefault(x => x.BoeId == boe.Id && x.FieldName == SikorskyConstants.SIKORSKY_CF_CATEGORY.ToUpper())?.CfValue;
                boe.CamName = sikorskyCfData.FirstOrDefault(x => x.BoeId == boe.Id && x.FieldName == SikorskyConstants.SIKORSKY_CF_CAMNAME.ToUpper())?.CfValue;
                boe.ClassOfCost = sikorskyCfData.FirstOrDefault(x => x.BoeId == boe.Id && x.FieldName == SikorskyConstants.SIKORSKY_CF_CLASSOFCOST.ToUpper())?.CfValue?.Replace(" ", string.Empty).GetEnumeratedValueNullable<ClassOfCost>() ?? ClassOfCost.None;
            });
        }

        #endregion

        #region RTE Load Methods

        /// <summary>
        /// Pulls RTE fields for the DTOs, and updates them as needed
        /// </summary>
        /// <param name="dtos">DTOs that whose RTE fields will be loaded, if they are null</param>
        [DbQuery]
        public void LoadRTEFields(ICollection<BoeDTO> dtos)
        {
            if (dtos == null || !dtos.Any()) { return; }

            List<RteFieldsHelper> dataFromDb = null;

            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                // Only want to pull RTE for an object when:
                //   - the object exists (Id > 0)
                //   - at least one of the RTE fields has not been inserted into, or retrieved already
                List<int> dtoIds = dtos.Where(x => x.Id > 0 && (!x.WasDataSourceSet || !x.WasDescriptionSet) && x.Updateable != UpdateType.Deleted).Select(x => x.Id).ToList();

                if (dtoIds.Any())
                {
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        gbe.Database.CommandTimeout = 360;  // give queries enough time to execute

                        // get the basic BOE data from the sprocResults
                        dataFromDb = (from b in gbe.BOEs
                                      where dtoIds.Contains(b.BOEID)
                                      select new RteFieldsHelper
                                      {
                                          Id = b.BOEID,
                                          Description = b.BOEDescription,
                                          DataSource = b.DataSource
                                      }).ToList();
                    }

                    dataFromDb.AsParallel().ForAll(dbData =>
                        {
                            BoeDTO dto = dtos.First(b => b.Id == dbData.Id);

                            // We do not want to overwrite existing data during, so we only fill missing data, if available
                            dto.Description = dto.WasDescriptionSet ? dto.Description : dbData.Description;
                            dto.DataSource = dto.WasDataSourceSet ? dto.DataSource : dbData.DataSource;
                        });
                }
            }
        }

        #endregion

        /// <summary>
        /// Get all BOE Data
        /// </summary>
        /// <returns>Boe DTO</returns>
        [DbQuery]
        virtual public Collection<BoeDTO> GetAllBoesForMetrics()
        {
            Collection<BoeDTO> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    // select the Boes from the database
                    toReturn = new Collection<BoeDTO>((from b in gbe.BOEs
                                                       select new BoeDTO
                                                       {
                                                           State = (BOEState)b.BOEStateID,
                                                           WorkspaceID = b.WorkspaceID
                                                       }).ToArray());
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get task variable IDs that contain the inputted BOE ID
        /// </summary>
        /// <param name="inBoeID">BOE ID</param>
        /// <returns>task variable IDs</returns>
        [DbQuery]
        virtual public ICollection<int> GetTaskVariableIdsByBoeId(int inBoeID)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                ICollection<int> toReturn = new Collection<int>();

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    // since variables can be based on BOEs, Clins or WBS, that's how we need to look for them..
                    toReturn = (from s in gbe.SumOfBOE_OrdinaryVariableXREF
                                where s.BOEID == inBoeID
                                    || (s.CLINID.HasValue && gbe.WBS_CLIN_BOE_XREF.Where(x => x.BOEID == inBoeID && x.CLINID == s.CLINID).Any())
                                    || (s.WBSID.HasValue && gbe.WBS_CLIN_BOE_XREF.Where(x => x.BOEID == inBoeID && x.WBSID == s.WBSID).Any())
                                select s.OrdinaryVariableID).Distinct().ToCollection();
                }

                return toReturn;
            }
        }

        /// <summary>
        /// Get workspace variable IDs that are affect by the BOE for Sum Of Boe variables.
        /// </summary>
        /// <param name="inBoeID">BOE ID</param>
        /// <returns>workspace variable IDs</returns>
        [DbQuery]
        virtual public ICollection<int> GetWorkspaceVariableIdsByBoeId(int inBoeID)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                ICollection<int> toReturn = new Collection<int>();
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    // since variables can be based on BOEs, Clins or WBS, that's how we need to look for them..
                    toReturn = (from s in gbe.SumOfBOE_WorkspaceVariableXREF
                                where s.BOEID == inBoeID
                                    || (s.CLINID.HasValue && gbe.WBS_CLIN_BOE_XREF.Where(x => x.BOEID == inBoeID && x.CLINID == s.CLINID).Any())
                                    || (s.WBSID.HasValue && gbe.WBS_CLIN_BOE_XREF.Where(x => x.BOEID == inBoeID && x.WBSID == s.WBSID).Any())
                                select s.WorkspaceVariableID).Distinct().ToCollection();
                }

                return toReturn;
            }
        }

        /// <summary>
        /// Returns the xref id for the given combination
        /// </summary>
        /// <param name="wbsID">WBS id</param>
        /// <param name="clinID">CLIN id</param>
        /// <param name="boeID">BOE id</param>
        /// <returns>XREF id</returns>
        [DbQuery]
        virtual public int GetWbsClinBoeXrefId(int? wbsID, int? clinID, int? boeID)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    int toReturn = -1;

                    var resultsLinq = (from b in gbe.WBS_CLIN_BOE_XREF
                                       where
                                       ((wbsID.HasValue && b.WBSID == wbsID) ||
                                        ((wbsID == null || !wbsID.HasValue) && !b.WBSID.HasValue)) &&
                                       ((clinID.HasValue && b.CLINID == clinID) ||
                                        ((clinID == null || !clinID.HasValue) && !b.CLINID.HasValue)) &&
                                       ((boeID.HasValue && b.BOEID == boeID) ||
                                        ((boeID == null || !boeID.HasValue) && !b.BOEID.HasValue))
                                       select b).FirstOrDefault();

                    if (resultsLinq != null)
                    {
                        toReturn = resultsLinq.WCBID;
                    }

                    return toReturn;
                }
            }
        }

        /// <summary>
        /// Checks if the BOE exists in the Xref table given the Custom Field ID and boe ID
        /// </summary>
        /// <param name="inBOEID">BOE ID</param>
        /// <param name="inCustomFieldID">custom field ID</param>
        /// <returns>true if it exists, false if not</returns>
        [DbQuery]
        virtual public bool CheckIfBoeExistsByCustomFieldId(int inBOEID, int inCustomFieldID)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                bool toReturn = false;

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    toReturn = (from b in gbe.BOECustomFieldValueXREFs
                                from c in gbe.CustomFieldValues
                                where b.BOEID == inBOEID && c.CustomFieldID == inCustomFieldID && b.CustomFieldValueID == c.CustomFieldValueID
                                select b.BCFVID).Any();
                }

                return toReturn;
            }
        }

        /// <summary>
        /// Get a ICollection of BOE IDs that are using the resource ID
        /// </summary>
        /// <param name="inResourceID"></param>
        /// <returns>BOE IDs whose children elements use a given resource ID</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        [DbQuery]
        virtual public ICollection<int> GetIdsByResourceId(int inResourceID)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                List<int> boeIds = new List<int>();

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    boeIds.AddRange((from lt1 in gbe.BOELaborTypes
                                     join be1 in gbe.BOETaskElements on lt1.BOETaskElementID equals be1.BOETaskElementID
                                     join b1 in gbe.BOEs on be1.BOEID equals b1.BOEID
                                     where lt1.ResourceID == inResourceID
                                     select b1.BOEID)
                                     .Union(
                                         from o3 in gbe.ODCTypes
                                         join oe3 in gbe.ODCTaskElements on o3.ODCTaskElementID equals oe3.ODCTaskElementID
                                         join b3 in gbe.BOEs on oe3.BOEID equals b3.BOEID
                                         where o3.ResourceID == inResourceID
                                         select b3.BOEID)
                                     .Union(
                                         from tt4 in gbe.TravelTrips
                                         join te4 in gbe.TravelTripTaskElements on tt4.TravelTripTaskElementID equals te4.TravelTripTaskElementID
                                         join b4 in gbe.BOEs on te4.BOEID equals b4.BOEID
                                         join w4 in gbe.Workspaces on b4.WorkspaceID equals w4.WorkspaceID
                                         join r4 in gbe.Resources on w4.ResourceListID equals r4.ResourceListID
                                         where tt4.SegmentID == r4.SegmentID && r4.ResourceID == inResourceID
                                         select b4.BOEID)
                                     .Distinct().ToList());
                }

                return boeIds;
            }
        }

        /// <summary>
        /// Determines if the BOE Contains a Material Task Element
        /// Returns true if the BOE is marked as Material, exists in the Material Task Element table and not in the ODC or Labor Task Element table
        /// </summary>
        /// <param name="inBoeID"></param>
        /// <returns></returns>
        [DbQuery]
        virtual public bool BoeContainsMaterialElement(int inBoeID)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                bool containsMaterials = false;

                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    containsMaterials = (from b in gbe.BOEs
                                         where b.BOEID == inBoeID && b.IsMaterial == true && (from t in gbe.MaterialTaskElements select t.BOEID).Contains(b.BOEID)
                                         select b.BOEID).Any();
                }

                return containsMaterials;
            }
        }

        /// <summary>
        /// Determines if the BOE contains a labor/cost element
        /// Returns true if the BOE is not marked as material, exists in either the Task Element or ODC Elemment, and not in Material Task Element
        /// </summary>
        /// <param name="inBoeID"></param>
        /// <returns></returns>
        [DbQuery]
        virtual public bool BoeContainsLaborCostElement(int inBoeID)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                bool containsLabor = false;
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    containsLabor = (from b in gbe.BOEs
                                     where b.BOEID == inBoeID && b.IsMaterial == false && (((from t in gbe.BOETaskElements
                                                                                             select t.BOEID).Contains(b.BOEID)
                                                                                   || (from t in gbe.ODCTaskElements
                                                                                       select t.BOEID).Contains(b.BOEID) ||
                                                                                       (from t in gbe.TravelTripTaskElements
                                                                                        select t.BOEID).Contains(b.BOEID))
                                                                                        &&
                                                                                       !(from m in gbe.MaterialTaskElements
                                                                                         select m.BOEID).Contains(b.BOEID))
                                     select b.BOEID).Any();
                }

                return containsLabor;
            }
        }

        /// <summary>
        /// Returns true if the Boe belongs to the given workspace.
        /// </summary>
        /// <param name="workspaceId">Workspace Id</param>
        /// <param name="boeId">Boe Id</param>
        /// <returns>True/False</returns>
        [DbQuery]
        virtual public bool DoesWorkspaceContainBoe(int workspaceId, int boeId)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                bool result = false;
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    result = gbe.BOEs.Any(x => x.WorkspaceID == workspaceId && x.BOEID == boeId);
                }

                return result;
            }
        }

        /// <summary>
        /// Gets Boe's state
        /// </summary>
        /// <param name="boeId">Boe Id</param>
        /// <returns>Boe's State</returns>
        [DbQuery]
        virtual public BOEState GetBoeState(int boeId)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                BOEState result;
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    // default value for int is a 0, which is also the state of "none"
                    result = (BOEState)gbe.BOEs.Where(x => x.BOEID == boeId).Select(x => x.BOEStateID).FirstOrDefault();
                }

                return result;
            }
        }

        #endregion

        #region Commits

        /// <summary>
        /// Upsert the BOE
        /// </summary>
        /// <param name="dtoToUpsert">BOE to upsert</param>
        /// <returns>ID of the upserted BOE</returns>
        protected override int? Upsert(BoeDTO dtoToUpsert)
        {
            // Make sure that RTE data is loaded, that way we do not wipe it out..
            this.LoadRTEFields(new List<BoeDTO>() { dtoToUpsert });

            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                int? toReturn = null;

                if (dtoToUpsert != null)
                {

                    // If this is a new BOE and the WCBID is undefined look for a matching WBS/CLIN combination with
                    // no BOE currently assigned, and use it if found
                    if (dtoToUpsert.Id < 1 && dtoToUpsert.WCBID < 1)
                    {
                        dtoToUpsert.WCBID = GetWbsClinBoeXrefId(dtoToUpsert.WBSID, dtoToUpsert.CLINID, null);
                    }

                    // assemble CSV lists of ids for LMC and subcontractor authors
                    string lmcAuthorsList = null;
                    if (dtoToUpsert.AuthorIDs != null && dtoToUpsert.AuthorIDs.Any())
                    {
                        lmcAuthorsList = string.Join(",", dtoToUpsert.AuthorIDs.Select(i => i.ToString()));
                    }

                    string subcontractorAuthorsList = null;
                    if (dtoToUpsert.SubcontractorAuthorIDs != null && dtoToUpsert.SubcontractorAuthorIDs.Any())
                    {
                        subcontractorAuthorsList = string.Join(",", dtoToUpsert.SubcontractorAuthorIDs.Select(i => i.ToString()));
                    }

                   using (GenBoeEntities gbm = new GenBoeEntities())
                    {
                        toReturn = gbm.upsertBOE(
                                    dtoToUpsert.WCBID,
                                    dtoToUpsert.Id,
                                    dtoToUpsert.WBSID,
                                    dtoToUpsert.CLINID,
                                    dtoToUpsert.State == BOEState.None ? (int?)BOEState.Unassigned : (int?)dtoToUpsert.State,
                                    dtoToUpsert.StartDate,
                                    dtoToUpsert.EndDate,
                                    lmcAuthorsList,
                                    dtoToUpsert.WorkspaceID,
                                    dtoToUpsert.UpdateDate,
                                    dtoToUpsert.Description,
                                    dtoToUpsert.DataSource,
                                    dtoToUpsert.UpdatedByUserId,
                                    dtoToUpsert.HistoricMetricDisclosureChecked,
                                    dtoToUpsert.NumAuthorReassigned,
                                    dtoToUpsert.isMaterial,
                                    dtoToUpsert.Title,
                                    subcontractorAuthorsList,
                                    dtoToUpsert.CopySourceBoeId,
                                    dtoToUpsert.IsMultiClinWbs).FirstOrDefault().Value;

                        // refresh the update-date to match the DB value, only needed if we need to update custom field value containers
                        if (dtoToUpsert.CustomFieldValueContainers != null && dtoToUpsert.CustomFieldValueContainers.Any())
                        {
                            BOE boeEntity;
                            if ((boeEntity = gbm.BOEs.FirstOrDefault(b => b.BOEID == dtoToUpsert.Id)) != null)
                            {
                                dtoToUpsert.UpdateDate = boeEntity.UpdateDT;
                            }
                        }
                    }

                    // save BOE Custom Field Value Containers
                    if (dtoToUpsert.CustomFieldValueContainers != null && dtoToUpsert.CustomFieldValueContainers.Any())
                    {
                        this.SaveBOECustomFieldValueContainers(dtoToUpsert.CustomFieldValueContainers, toReturn.Value);
                    }
                }

                return toReturn;
            }
        }

        /// <summary>
        /// Delete the BOE
        /// </summary>
        /// <param name="dtoToDelete">BOE to delete</param>
        /// <returns>Id of the deleted BOE</returns>
        protected override int? Delete(BoeDTO dtoToDelete)
        {
            int? toReturn = null;

            if (dtoToDelete != null)
            {
                using (StopwatchTimer sw = new StopwatchTimer(this.Log))
                {
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        gbe.deleteBOE(dtoToDelete.Id, dtoToDelete.UpdateDate);
                    }
                }

                toReturn = dtoToDelete.Id;
            }

            return toReturn;
        }

        /// <summary>
        /// Save BOE Custom Field Value Containers
        /// </summary>
        /// <param name="inCustomFieldValueContainers">CustomFieldValueContainers</param>
        /// <param name="inBOEID">BOE ID</param>
        private void SaveBOECustomFieldValueContainers(ICollection<CustomFieldValueContainer> inCustomFieldValueContainers, int inBOEID)
        {
            if (inCustomFieldValueContainers == null)
            {
                throw new ArgumentNullException(nameof(inCustomFieldValueContainers));
            }

            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                foreach (CustomFieldValueContainer BOECustomFieldValueContainer in inCustomFieldValueContainers)
                {
                    this.SaveBOECustomFieldValueContainer(BOECustomFieldValueContainer, inBOEID);
                }
            }
        }

        /// <summary>
        /// Save a single BOE Custom Field Value Container
        /// </summary>
        /// <param name="inCustomFieldValueContainer">CustomFieldValueContainer to be saved or deleted</param>
        private void SaveBOECustomFieldValueContainer(CustomFieldValueContainer inCustomFieldValueContainer, int inBOEID)
        {
            if (inCustomFieldValueContainer == null)
            {
                throw new ArgumentNullException(nameof(inCustomFieldValueContainer));
            }

            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                if (inCustomFieldValueContainer.Updateable == UpdateType.Deleted)
                {
                    this.DeleteBoeCustomFieldValueContainer(inCustomFieldValueContainer, inBOEID);
                }
                else if (inCustomFieldValueContainer.Updateable == UpdateType.Upsert)
                {
                    this.UpdateBoeCustomFieldValueContainer(inCustomFieldValueContainer, inBOEID);
                }
            }
        }

        /// <summary>
        /// Update custom field value container
        /// </summary>
        /// <param name="inCustomFieldValueContainer">CustomFieldValueContainer data</param>
        virtual public void UpdateBoeCustomFieldValueContainer(CustomFieldValueContainer inCustomFieldValueContainer, int inBOEID)
        {
            if (inCustomFieldValueContainer == null)
            {
                throw new ArgumentNullException(nameof(inCustomFieldValueContainer));
            }

            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    gbe.upsertBOECustomFieldValue(inCustomFieldValueContainer.ContainerID,
                            inBOEID,
                            inCustomFieldValueContainer.CustomFieldID,
                            inCustomFieldValueContainer.CustomFieldValueID,
                            inCustomFieldValueContainer.OpenEndedValue,
                            inCustomFieldValueContainer.UpdateDate,
                            inCustomFieldValueContainer.IsOpenEnded);
                }
            }
        }

        /// <summary>
        /// Delete custom field value container
        /// </summary>
        /// <param name="inCustomFieldValueContainer">CustomFieldValueContainer data</param>
        virtual public void DeleteBoeCustomFieldValueContainer(CustomFieldValueContainer inCustomFieldValueContainer, int inBOEID)
        {
            if (inCustomFieldValueContainer == null)
            {
                throw new ArgumentNullException(nameof(inCustomFieldValueContainer));
            }

            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    gbe.deleteBOECustomFieldValue(
                            inCustomFieldValueContainer.ContainerID,
                            inBOEID,
                            inCustomFieldValueContainer.CustomFieldValueID,
                            inCustomFieldValueContainer.UpdateDate,
                            inCustomFieldValueContainer.IsOpenEnded);
                }
            }
        }

        #endregion
    }

    internal class RteFieldsHelper
    {
        public int Id { get; set; }
        public string DataSource { get; set; }
        public string Description { get; set; }
        public string MoqText { get; set; }
    }
}
 
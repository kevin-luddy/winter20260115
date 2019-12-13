// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.IO.Import;
    using GenBOE.ActionLogic.ModelView.Workspace;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.DataBridge.Reference;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.PickList;

    /// <summary>
    /// Workspace Controller logic class for MST behavior 
    /// </summary>
    public class WorkspaceControllerLogicMST : WorkspaceControllerLogic
    {

        private RMSZoneTravelRatesFeesDataLoader zoneTravelRatesFeesLoader;
        private IEscalationRatesDTOLoader systemEscalationRatesLoader;
        private IMSTTravelNonzoneFeesAndCostsDTODataLoader systemFeesLoader;
        private IOffloadRatesDTOLoader offloadRatesLoader;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="workspaceLoader">The workspace loader.</param>
        /// <param name="userLoader">The user loader.</param>
        /// <param name="resourceLoader">The resource loader.</param>
        /// <param name="tmResourceRateDTODataLoader">The T&amp;M resource rate dto data loader.</param>
        /// <param name="boeTaskElementRecalc">The boe task element recalc.</param>
        /// <param name="inUseDataLoader">The In-Use data loader.</param>
        /// <param name="retriever">The retriever.</param>
        /// <param name="factory">The factory.</param>
        /// <param name="commonDataMapper">The common data mapper.</param>
        /// <param name="permissionLoader">The permission loader.</param>
        /// <param name="boeImporter">The boe importer.</param>
        /// <param name="boeLaborControllerLogic">The boe labor controller logic.</param>
        /// <param name="fullWsRecalc">The full ws recalc.</param>
        /// <param name="zoneTravelRatesFeesLoader">The zone travel rates fees loader.</param>
        /// <param name="systemEscalationRatesLoader">The system escalation rates loader.</param>
        /// <param name="systemFeesLoader">The system fees loader.</param>
        /// <param name="workspaceVariableLoader">The workspace variable loader.</param>
        /// <param name="customFieldValueLoader">The custom field value loader.</param>
        /// <param name="offloadRatesDTOLoader">The offload rates dto loader.</param>
        /// <param name="projectMapDataLoader">The project map data loader.</param>
        /// <param name="boePickListMapper">The boe pick list mapper.</param>
        /// <param name="ptmPickListMapper">The ptm pick list mapper.</param>
        public WorkspaceControllerLogicMST(
            IWorkspaceDTODataLoader workspaceLoader,
            IUserDTODataLoader userLoader,
            IResourceDTODataLoader resourceLoader,
            ITMResourceRateDTODataLoader tmResourceRateDTODataLoader,
            BoeTaskElementRecalculation boeTaskElementRecalc,
            IInUseDataLoader inUseDataLoader,
            IFullObjectFactory factory,
            ICommonDataMapper commonDataMapper,
            IPermissionsDTODataLoader permissionLoader,
            FullBoeDataImporter boeImporter,
            IBOELaborControllerLogic boeLaborControllerLogic,
            IFullWorkspaceRecalculation fullWsRecalc,
            RMSZoneTravelRatesFeesDataLoader zoneTravelRatesFeesLoader,
            IEscalationRatesDTOLoader systemEscalationRatesLoader,
            IMSTTravelNonzoneFeesAndCostsDTODataLoader systemFeesLoader,
            IWorkspaceVariableDTODataLoader workspaceVariableLoader,
            ICustomFieldValueDTODataLoader customFieldValueLoader,
            ICustomFieldDTODataLoader customFieldLoader,
            IOffloadRatesDTOLoader offloadRatesDTOLoader,
            IProjectMapDataLoader projectMapDataLoader,
            IPickListMapper boePickListMapper,
            IPickListMapper ptmPickListMapper)
            : base(
                workspaceLoader,
                userLoader,
                resourceLoader,
                tmResourceRateDTODataLoader,
                boeTaskElementRecalc,
                inUseDataLoader,
                factory,
                commonDataMapper,
                permissionLoader,
                boeImporter,
                boeLaborControllerLogic,
                fullWsRecalc,
                workspaceVariableLoader,
                customFieldValueLoader,
                customFieldLoader,
                projectMapDataLoader,
                boePickListMapper,
                ptmPickListMapper
        )
        {
            this.zoneTravelRatesFeesLoader = zoneTravelRatesFeesLoader;
            this.systemEscalationRatesLoader = systemEscalationRatesLoader;
            this.systemFeesLoader = systemFeesLoader;
            this.offloadRatesLoader = offloadRatesDTOLoader;
        }

        /// <summary>
        /// Creates the <see cref="IWorkspaceIdentificationModelView"/> for the MST mode
        /// </summary>
        /// <param name="workspace">The <see cref="FullWorkspace"/> used to populate the model view</param>
        /// <returns>A populated <see cref="IWorkspaceIdentificationModelView"/> for the IS&amp;GS mode</returns>
        public override IWorkspaceIdentificationModelView GetWorkspaceIdentificationModelView(FullWorkspace workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            UserDTO costVolumeLeadDTO = this.UserLoader.GetUserByID(workspace.CostVolumeLeadPricerUserID);

            return new WorkspaceIdentificationMSTModelView(workspace, costVolumeLeadDTO);
        }

        /// <summary>
        /// Creates the <see cref="CreateWorkspaceMSTModelView"/> for the MST mode
        /// </summary>
        /// <returns>A new <see cref="CreateWorkspaceMSTModelView"/></returns>
        public override ICreateWorkspaceModelView GetCreateWorkspaceModelView()
        {
            return new CreateWorkspaceMSTModelView();
        }

        /// <summary>
        /// Returns a dictionary of resource primary keys and ids for the workspace.  This is specific subset for the workspace resource rate page.
        /// </summary>
        /// <param name="workspace">The full workspace</param>
        /// <returns>A dictionary of resource primary keys and ids for the workspace.</returns>
        public override IDictionary<string, Tuple<bool, int>> GetWorkspaceResourcesForWorkspaceResourceRateTM(FullWorkspace workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            SortedDictionary<string, Tuple<bool, int>> resources = new SortedDictionary<string, Tuple<bool, int>>();

            // get resources that allow mapping
            var nonMappableResources = (from x in workspace.ResourcesForWsResourceListId
                                        where (x.ElementOfCost == ElementOfCostType.Sub ||
                                        x.ElementOfCost == ElementOfCostType.IWTA)
                                        select x).ToArray();

            foreach (ResourceDTO resource in nonMappableResources)
            {
                resources.Add(resource.ResourceName, new Tuple<bool, int>(false, resource.Id));
            }

            return resources;
        }

        /// <summary>
        /// Gets a filtered list of Resources
        /// </summary>
        /// <param name="inWorkspaceResources">The list of resources to filter</param>
        /// <returns></returns>
        public override ICollection<ResourceDTO> GetFilteredWorkspaceResources(IReadOnlyCollection<ResourceDTO> inWorkspaceResources)
        {
            if (inWorkspaceResources == null)
            {
                throw new ArgumentNullException(nameof(inWorkspaceResources), "inWorkspaceResources cannot be null");
            }

            ICollection<ResourceDTO> workspaceResources = (from x in inWorkspaceResources
                                                           where
                                                               (x.ElementOfCost == ElementOfCostType.Sub ||
                                                               x.ElementOfCost == ElementOfCostType.IWTA ||
                                                               (x.ElementOfCost == ElementOfCostType.LMLabor)
                                                           )
                                                           select x).ToArray();

            return workspaceResources;
        }

        /// <summary>
        /// Gets a filtered list of Resources
        /// </summary>
        /// <param name="inWorkspaceResources">The list of resources to filter</param>
        /// <returns></returns>
        public override ICollection<ResourceDTO> GetFilteredOtherWorkspaceResources(IReadOnlyCollection<ResourceDTO> inWorkspaceResources)
        {
            if (inWorkspaceResources == null)
            {
                throw new ArgumentNullException(nameof(inWorkspaceResources), "inWorkspaceResources cannot be null");
            }

            return this.GetFilteredWorkspaceResources(inWorkspaceResources);
        }

        /// <summary>
        /// Return true, if user is BOE Author or Workspace Administrator.
        /// </summary>
        /// <param name="isAuthor">true, if user is BOE Author</param>
        /// <param name="isWorkspaceAdmin">true, if user is Workspace Administrator</param>
        /// <returns></returns>
        public override bool CanExportBoeForWorkoffline(bool isAuthor, bool isWorkspaceAdmin)
        {
            // For MST, a BOE can only be exported for Workoffline if they are the author or workspace administrator.
            return isAuthor || isWorkspaceAdmin;
        }

        /// <summary>
        /// Creates a new model view for Space specific T&amp;M WorkspaceResourceRateModelView 
        /// </summary>
        /// <returns></returns>
        public override WorkspaceResourceRateTMModelView CreateWorkspaceResourceRateTMModelView()
        {
            WorkspaceResourceRateTMModelView model =
                new WorkspaceResourceRateTMModelView
                {
                    WorkspaceResourceRateTMHeadingText = WebConstants.WORKSPACE_RESOURCE_RATE_TM_SPACE_HEADING_TEXT,
                    WorkspaceResourceRateTMJumpDescription = WebConstants.WORKSPACE_RESOURCE_RATE_TM_SPACE_JUMP_DESCRIPTION_TEXT,
                    WorkspaceResourceRateTMControlDescription = WebConstants.WORKSPACE_RESOURCE_RATE_TM_SPACE_CONTROL_DESCRIPTION_TEXT
                };
            return model;
        }

        /// <summary>
        /// Creates a new model view for Space specific WorkspaceResourceRateTMModelView 
        /// </summary>
        /// <returns></returns>
        public override WorkspaceResourceRateTMModelView CreateWorkspaceResourceRateTMModelView(TMResourceRateDTO resourceRateDTO, ResourceDTO workspaceResourceDTO, bool inUse)
        {
            WorkspaceResourceRateTMModelView model =
                new WorkspaceResourceRateTMModelView(resourceRateDTO, workspaceResourceDTO, inUse)
                {
                    WorkspaceResourceRateTMHeadingText = WebConstants.WORKSPACE_RESOURCE_RATE_TM_SPACE_HEADING_TEXT,
                    WorkspaceResourceRateTMJumpDescription = WebConstants.WORKSPACE_RESOURCE_RATE_TM_SPACE_JUMP_DESCRIPTION_TEXT,
                    WorkspaceResourceRateTMControlDescription = WebConstants.WORKSPACE_RESOURCE_RATE_TM_SPACE_CONTROL_DESCRIPTION_TEXT
                };
            return model;
        }

        /// <summary>
        /// Gets the default ExcelReportTemplateType for a new workspace
        /// </summary>
        /// <returns>Default ExcelReportTemplateType</returns>
        public override ExcelReportTemplateType GetDefaultReportTemplateType()
        {
            return ExcelReportTemplateType.MASTER;
        }

        /// <summary>
        /// Gets the default picklist values for template types
        /// </summary>
        /// <param name="ws">Workspace</param>
        /// <returns>Collection of template types</returns>
        public override ICollection<ExcelReportTemplateType> GetPicklistReportTemplateTypes(FullWorkspace ws)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }
            Collection<ExcelReportTemplateType> toReturn = new Collection<ExcelReportTemplateType>();

            // Project Map Workspaces do no get these templates by default
            if (!ws.IsProjectMapWorkspace)
            {
                toReturn.Add(ExcelReportTemplateType.MASTER);
                toReturn.Add(ExcelReportTemplateType.MST_DEEPWATER_PORTRAIT);
                toReturn.Add(ExcelReportTemplateType.MST_PORTRAIT_12_POINT_1_INCH_MARGINS);
                toReturn.Add(ExcelReportTemplateType.MST_SPACE_FENCE_PORTRAIT);
                toReturn.Add(ExcelReportTemplateType.MST_STANDARD_PORTRAIT);
                toReturn.Add(ExcelReportTemplateType.MST_PORTRAIT_12_POINT_1_INCH_MARGINS_GENBOE_TASK_IDS);
                toReturn.Add(ExcelReportTemplateType.MST_PORTRAIT_12_POINT_1_INCH_MARGINS_GENBOE_BOE_TASK_RESOURCE_IDS);
                toReturn.Add(ExcelReportTemplateType.MST_PORTRAIT_12_POINT_1_INCH_MARGINS_CUSTOM_FIELDS);
                toReturn.Add(ExcelReportTemplateType.RMS_PORTRAIT_WBS_CLIN_SOW);
            }

            // RMS - Sikorsky Project Map added to all Workspace types, default template for Project Map Workspaces
            toReturn.Add(ExcelReportTemplateType.RMS_SIKORSKY_PROJECT_MAP);
            toReturn.Add(ExcelReportTemplateType.RMS_RMS_PROJECT_MAP);

            return toReturn;
        }

        /// <summary>
        /// Populates properties with company specific data
        /// </summary>
        /// <param name="theModel">The <see cref="ICreateWorkspaceModelView" /> to populate</param>
        /// <param name="workspace">The <see cref="WorkspaceDTO" /> containing data used to populate the model view</param>
        public override void PopulateCompanySpecificWorkspaceProperties(ICreateWorkspaceModelView theModel, WorkspaceDTO workspace)
        {
            if (theModel == null)
            {
                throw new ArgumentNullException(nameof(theModel));
            }
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            // nothing to do here
        }

        /// <summary>
        /// Populates properties with company specific data
        /// </summary>
        /// <param name="theModel">The <see cref="IWorkspaceIdentificationModelView"/> to populate</param>
        /// <param name="workspace">The <see cref="WorkspaceDTO"/> containing data used to populate the model view</param>
        public override void PopulateCompanySpecificWorkspaceProperties(IWorkspaceIdentificationModelView theModel, WorkspaceDTO workspace)
        {
            if (theModel == null)
            {
                throw new ArgumentNullException(nameof(theModel));
            }
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            WorkspaceIdentificationMSTModelView workspaceIdentificationMSTModelView = theModel as WorkspaceIdentificationMSTModelView;
            if (workspaceIdentificationMSTModelView != null)
            {
                workspace.AllowGridEdit = workspaceIdentificationMSTModelView.AllowGridEdit;
            }
        }

        /// <summary>
        /// Gets the Workspace Identification view name for the MST mode
        /// </summary>
        public override string WorkspaceIdentificationViewName
        {
            get
            {
                return WebConstants.VIEW_WORKSPACE_IDENTIFICATION_MST;
            }
        }

        /// <summary>
        /// Updates the Zone Travel Rates to be the System default rates.
        /// </summary>
        /// <param name="workspaceId">The id for workspace to update.</param>
        public override void CopySystemZoneTravelRates(int workspaceId)
        {
            this.zoneTravelRatesFeesLoader.CopySystemDefaultFees(workspaceId);
            this.zoneTravelRatesFeesLoader.CopySystemDefaultRates(workspaceId);
        }

        /// <summary>
        /// Gets the last updated time the Zone Travel was updated; Null if it is latest.
        /// </summary>
        /// <param name="workspaceId">The id for workspace to update.</param>
        public override DateTime? GetLastupdatedTimeZoneTravel(int workspaceId)
        {
            DateTime? latestDate = null;
            if (this.zoneTravelRatesFeesLoader.AreCurrentEscalationRatesOutOfDate(workspaceId))
            {
                latestDate = this.systemEscalationRatesLoader.GetAll().Select(r => r.UpdateDate).Max();
            }

            if (this.zoneTravelRatesFeesLoader.AreCurrentFeesOutOfDate(workspaceId))
            {
                DateTime feesLatest = this.systemFeesLoader.getAllFeesAndCosts().Select(f => f.UpdateDate).Max();
                if (!latestDate.HasValue || latestDate < feesLatest)
                {
                    latestDate = feesLatest;
                }
            }

            if(latestDate != null)
            {
                // Convert to EST (timezone used for messages such as the maintenance banner)
                latestDate = ((DateTime)latestDate).AddHours(Convert.ToInt32(ConfigurationUtilities.GetAppSetting("DatabaseESTOffset")));
            }

            return latestDate;
        }

        /// <summary>
        /// Gets the last updated time the Offload was updated; Null if it is latest.
        /// </summary>
        /// <param name="workspaceId">The id for workspace to update.</param>
        public override DateTime? GetLastupdatedTimeOffload(int workspaceId)
        {
            DateTime? latestDate = null;
            if(this.offloadRatesLoader.AreCurrentOffloadRatesOutOfDate(workspaceId))
            {
                latestDate = this.offloadRatesLoader.GetAllSystemRates().Select(r => r.UpdateDate).Max();

                // Convert to EST (timezone used for messages such as the maintenance banner)
                latestDate = ((DateTime)latestDate).AddHours(Convert.ToInt32(ConfigurationUtilities.GetAppSetting("DatabaseESTOffset")));
            }
            
            return latestDate;
        }
    }
}

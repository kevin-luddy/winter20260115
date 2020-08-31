// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.ActionLogic._ModelView;
    using GenBOE.ActionLogic.BLL;
    using GenBOE.ActionLogic.BOETransitions;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.ActionLogic.ModelView.BOE;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.classes;

    public class BOELaborControllerLogicMST : BOELaborControllerLogic
    {
        private Logger _log = new Logger(typeof(GenBOEControllerLogic));

        /// <summary>
        /// Loader for MST Metrics.
        /// </summary>
        private readonly IMSTMetricLoader _mstMetricsLoader;
        private readonly IRteTemplateDataLoader rteTemplateDataLoader;
        
        /// <summary>
        /// Injection constructor.
        /// </summary>
        /// <param name="inBoeTaskElementRecalc"></param>
        /// <param name="inBoeStateMachine"></param>
        /// <param name="inBoeMediator"></param>
        /// <param name="inBoeTaskElementMediator"></param>
        /// <param name="inVarSelectBoeToSumCalc"></param>
        /// <param name="inResourceLoader"></param>
        /// <param name="inFactory"></param>
        /// <param name="inWorkspaceVariableDTODataLoader"></param>
        /// <param name="inuserLoader"></param>
        /// <param name="inBoeLoader"></param>
        /// <param name="inTaskElementDataLoader"></param>
        /// <param name="inPermissionsLoader"></param>
        /// <param name="perfOrgLoader"></param>
        /// <param name="inTaskVariableLoader"></param>
        /// <param name="inMSTMetricLoader"></param>
        public BOELaborControllerLogicMST(
            BoeTaskElementRecalculation inBoeTaskElementRecalc,
            IBOEStateMachine inBoeStateMachine,
            IBoeMediator inBoeMediator,
            IBoeTaskElementMediator inBoeTaskElementMediator,
            IVariableSelectBOEtoSumCalculation inVarSelectBoeToSumCalc,
            IResourceDTODataLoader inResourceLoader,
            IFullObjectFactory inFactory,
            IWorkspaceVariableDTODataLoader inWorkspaceVariableDTODataLoader,
            IUserDTODataLoader inuserLoader,
            IBoeDTODataLoader inBoeLoader,
            IBoeTaskElementDTODataLoader inTaskElementDataLoader,
            IPermissionsDTODataLoader inPermissionsLoader,
            IPerformingOrgDTODataLoader perfOrgLoader,
            IOrdinaryVariableLoader inTaskVariableLoader,
            IMSTMetricLoader inMSTMetricLoader,
            TaskElementValidation taskElementValidation,
            IVariableCircularReferenceChecker circularReferenceChecker,
            ICommonDataMapper commonDataMapper,
            IRteTemplateDataLoader rteTemplateDataLoader
            )
            : base(inBoeTaskElementRecalc,
                inBoeStateMachine,
                inBoeMediator,
                inBoeTaskElementMediator,
                inVarSelectBoeToSumCalc,
                inResourceLoader,
                inFactory,
                inWorkspaceVariableDTODataLoader,
                inBoeLoader,
                inTaskElementDataLoader,
                inuserLoader,
                inPermissionsLoader,
                perfOrgLoader,
                inTaskVariableLoader,
                taskElementValidation,
                circularReferenceChecker,
                commonDataMapper,
                rteTemplateDataLoader)
        {
            this._mstMetricsLoader = inMSTMetricLoader;
            this.rteTemplateDataLoader = rteTemplateDataLoader;
        }

        /// <summary>
        /// Gets the valid <see cref="MOQType" />'s for the MST configuration
        /// </summary>
        /// <returns>
        /// valid <see cref="MOQType" />'s for this company configuration
        /// </returns>
        public override ICollection<MOQType> GetMOQTypes()
        {
            return new MOQType[]
            {
                MOQType.MSTHistoricalPerformance,
                MOQType.MSTComparisonAnalogyMethod,
                MOQType.MSTCostEstimatingRelationships,
                MOQType.MSTParametricCostModels,
                MOQType.MSTStandardTimeEstimating,
                MOQType.MSTFactorUnitMethod,
                MOQType.MSTLevelOfEffortSupport,
                MOQType.MSTEngineeringJudgmentalEstimates
            };
        }

        /// <summary>
        /// Returns the correct MOQ help text for the MST configuration
        /// </summary>
        /// <returns>MST configuration MOQ help text</returns>
        public override string GetMOQTypesHelpText()
        {
            return CommonConstants.BOE_MOQ_TYPES_HELP_TEXT_MST;
        }

        /// <summary>
        /// Returns the correct MOQ Equation label text for the MST configuration
        /// </summary>
        /// <returns>Space Systems configuration MOQ Equation label text</returns>
        public override string GetMOQEquationLabel()
        {
            return CommonConstants.BOE_MOQ_EQUATION_LABEL;
        }

        /// <summary>
        /// Returns the correct MOQ text label for the MST configuration
        /// </summary>
        /// <returns>Space Systems configuration MOQ text label text</returns>
        public override string GetMOQTextLabel()
        {
            return CommonConstants.BOE_MOQ_TEXT_LABEL_SPACE_SYSTEMS;
        }

        /// <summary>
        /// Populates the passed in <see cref="MOQEquationModelView"/> with metrics
        /// </summary>
        /// <param name="ids">The TaskElement id's for which metrics will be retrieved</param>
        /// <param name="model">The <see cref="MOQEquationModelView"/> that will be populated</param>
        public override void GetMetricByTaskElementIds(Collection<int> ids, MOQEquationModelView model)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            model.PMMetricsUsed = this._mstMetricsLoader.GetByTaskElementIds(ids);
            this.SetShowMetricLink(model);
        }

        /// <summary>
        /// Gets view and model view data for displaying MST metric details from PMM.
        /// </summary>
        /// <param name="metricId">Metric Id.</param>
        /// <returns>Metric view and model view detail data.</returns>
        public override ViewResultData GetHistoricalMetricsDetails(int metricId)
        {
            ViewResultData viewResultData = new ViewResultData();

            MSTMetricDetailsDTO metric = this._mstMetricsLoader.GetMetricDetailsByIds(new Collection<int>() { metricId }).FirstOrDefault();

            if (metric != null)
            {
                viewResultData.Model = metric;
                viewResultData.ViewName = WebConstants.VIEW_ADD_HISTORICAL_METRIC_TO_BOE_MST;

                return viewResultData;
            }
            else
            {
                throw new ArgumentException("There is no Metric with the ID given.");
            }
        }
        
        /// <summary>
        /// Gets view and model view data for displaying MST metric details from external PMM System.
        /// </summary>
        /// <param name="metricId">Metric Id.</param>
        /// <returns>Metric view and model view detail data.</returns>
        public override ViewResultData GetHistoricalMetricsDetailsFromSource(int metricId)
        {
            ViewResultData viewResultData = new ViewResultData();

            MSTMetricDetailsDTO metric = this._mstMetricsLoader.GetByIds(new Collection<int>() { metricId }).FirstOrDefault();

            if (metric != null)
            {
                viewResultData.Model = metric;
                viewResultData.ViewName = WebConstants.VIEW_ADD_HISTORICAL_METRIC_TO_BOE_MST;

                return viewResultData;
            }
            else
            {
                throw new ArgumentException("There is no Metric with the ID given.");
            }
        }

        /// <summary>
        /// Gets the MOQ model view loaded with MST specific metrics previously saved to genBOE for the task element.
        /// </summary>
        /// <param name="taskElement">Task element object.</param>
        /// <param name="workspace">Workspace object.</param>
        /// <returns>Loaded MOQEquationModelView.</returns>
        public override MOQEquationModelView GetMOQModelView(BoeTaskElementDTO taskElement, FullWorkspace workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (taskElement == null)
            {
                throw new ArgumentNullException(nameof(taskElement));
            }

            MOQEquationModelView toReturn = new MOQEquationModelView(taskElement, this.VariableSelectBOEtoSumCalculation, workspace);
            toReturn.MoqTemplateAnswers = this.rteTemplateDataLoader.GetByBoeIdAndTaskId(workspace.Id, taskElement.BoeID, taskElement.Id).Where(t => t.SourceId == (int)RteTemplateSource.TaskMOQ).ToList();
            toReturn.PMMetricsUsed = this._mstMetricsLoader.GetByTaskElementIds(new Collection<int> { taskElement.Id });
            this.SetShowMetricLink(toReturn);
            return toReturn;
        }

        /// <summary>
        /// Sets the show metrics link.
        /// </summary>
        /// <returns>The <see cref="MOQEquationModelView"/> populated with the correct company specific value for ShowSearchMetricsLink.</returns>
        public override void SetShowMetricLink(MOQEquationModelView model)
        {
            if (model == null) { throw new ArgumentNullException(nameof(model)); }
            model.ShowSearchMetricsLink = true;
        }

        /// <summary>
        /// Saves metrics Ids and associate to task element.
        /// </summary>
        /// <param name="taskElementID">The id of the task element to save the metric to.</param>
        /// <param name="metricIDs">The PMM measure id of the metric to save to the task element</param>
        public override void SaveHistoricalMetricsToTaskElement(int taskElementID, ICollection<int> metricIDs)
        {
            this._mstMetricsLoader.Save(taskElementID, metricIDs);
        }

        /// <summary>
        /// Returns a <see cref="bool"/> indicating if the read only flag should be overridden
        /// </summary>
        /// <param name="ws">the <see cref="FullWorkspace"/> being viewed</param>
        /// <param name="boe">the <see cref="FullWorkspace"/> being viewed</param>
        /// <returns>true if read only should be overridden, false otherwise</returns>
        public override bool OverrideReadOnly(FullWorkspace ws, FullBoe boe)
        {
            if (ws == null)
            {
                throw new ArgumentNullException(nameof(ws));
            }

            if (boe == null)
            {
                throw new ArgumentNullException(nameof(boe));
            }

            bool toReturn = false;

            // (bug 27457) if the app is in "mst", author could also be a workspace admin
            if ((ws.WorkspaceState == WorkspaceState.Locked || ws.WorkspaceState == WorkspaceState.Working)
                && (boe.State == BOEState.DraftLocked || boe.State == BOEState.Draft))
            {
                UserDTO currentUser = this.UserLoader.GetUserForActiveUser();
                Collection<PermissionsDTO> boePermissions = this.PermissionsLoader.GetBOEPermissions(new List<int>() { boe.Id });

                if (boePermissions.Any(x => x.ETIUserId == currentUser.UserID && (x.Role == Role.Author || x.Role == Role.SubcontractorAuthor || x.Role == Role.WorkspaceAdmin)))
                {
                    toReturn = true;
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Populates the passed in <see cref="LaborTaskModelView"/> with metric search dialog parameters.
        /// </summary>
        /// <param name="model">The <see cref="LaborTaskModelView"/> that will be populated.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Catch any error so the genBOE does not break because it cannot retrieve data from PMM. Otherwise simply opening a task would fail when not connected to PMM.")]
        public override void GetMetricSearchDialogParameters(LaborTaskModelView model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            MSTMetricSearchCriteriaDTO searchCriteria;

            //PMM Connection string may not be configured for external/classified MST Installations
            bool pmmIsConfigured = ConfigurationUtilities.GetConnectionString("BI_Datamart_PMMEntities").ConnectionString.Any();

            if (pmmIsConfigured)
            {
                //Verify data can be retrieved from PMM.
                //Catching all errors will prevent the page from failing when opening a task if there is any issue pulling data from external source
                try
                {
                    searchCriteria = this._mstMetricsLoader.GetMSTMetricSearchCriteria();
                    model.MetricsSearchDialogParameters = new MetricsSearchDialogParametersModelView(searchCriteria);
                    model.MetricsSearchDialogParameters.DialogTitle = CommonConstants.MSTDialogTitle;
                    model.MetricsSearchDialogParameters.MSTSearchHelpLink = ConfigurationUtilities.GetAppSetting("MSTSearchCriteriaHelp", string.Empty);
                    model.MetricsPagingActionName = WebConstants.ACTION_PAGE_HISTORICAL_METRIC_SEARCH_RESULTS_MST;
                }
                catch (Exception ex)
                {
                    this._log.Error(ex, "Failed to retrieve data from PMM Database.");
                    pmmIsConfigured = false;
                }
            }

            if (!pmmIsConfigured)//Note that pmmIsConfigured may be changed in try/catch above
            {
                searchCriteria = new MSTMetricSearchCriteriaDTO();
                model.MetricsSearchDialogParameters = new MetricsSearchDialogParametersModelView(searchCriteria);
                model.MetricsSearchDialogParameters.MetricStoreConnected = false;
            }

            model.MetricsSearchDialogParameters.SearchMetricsDialogIdSuffix = CommonConstants.MSTMetricsDialogSuffix;
        }

        /// <summary>
        /// Returns a list of labels to be used in the MOQ Types page.
        /// </summary>
        /// <returns>Labels for MOQ Type Data Table Fields</returns>
        public override MoqTypeTableDataLabels GetMoqTypeLabels()
        {
            return new MoqTypeTableDataLabels() 
            {
                ContractNumber = "Contract Number"
            };
        }
    }
}

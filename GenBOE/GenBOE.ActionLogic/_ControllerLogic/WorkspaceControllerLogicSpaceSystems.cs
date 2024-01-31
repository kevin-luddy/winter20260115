// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using GenBOE.ActionLogic.BLL;
	using GenBOE.ActionLogic.BOETransitions;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.Common.Calculations;
	using GenBOE.ActionLogic.IO.Export;
	using GenBOE.ActionLogic.IO.Import;
	using GenBOE.ActionLogic.ModelView.Workspace;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.DTO;
	using GenBOE.DataBridge.Reference;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.Exceptions;
	using IES.Common.PickList;

	/// <summary>
	/// Workspace Controller logic class for Space Systems behavior 
	/// </summary>
	public class WorkspaceControllerLogicSpaceSystems : WorkspaceControllerLogic
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="workspaceLoader">The workspace loader.</param>
		/// <param name="inuserLoader">The inuser loader.</param>
		/// <param name="inResourceLoader">The in resource loader.</param>
		/// <param name="inTmResourceRateDataLoader">The in tm resource rate data loader.</param>
		/// <param name="inBoeTaskElementRecalc">The in BOE task element recalc.</param>
		/// <param name="inUseDataLoader">The in use data loader.</param>
		/// <param name="retriever">The retriever.</param>
		/// <param name="factory">The factory.</param>
		/// <param name="inCommonDataMapper">The in common data mapper.</param>
		/// <param name="inPermissionLoader">The in permission loader.</param>
		/// <param name="inBoeImporter">The in BOE importer.</param>
		/// <param name="inBOELaborControllerLogic">The in BOE labor controller logic.</param>
		/// <param name="fullWsRecalc">The full ws recalc.</param>
		/// <param name="inWorkspaceVariableLoader">The in workspace variable loader.</param>
		/// <param name="inCustomFieldValueLoader">CF value loader</param>
		/// <param name="customFieldLoader">The custom field loader.</param>
		/// <param name="offloadRatesDTOLoader">Offload Rates Loader</param>
		/// <param name="projectMapDataLoader">The project map data loader.</param>
		/// <param name="boePickListMapper">The BOE pick list mapper.</param>
		/// <param name="ptmPickListMapper">The PTM pick list mapper.</param>
		/// <param name="contractTypeLoader">Contract Type Loader</param>
		/// <param name="workspaceExporter">WS Exporter</param>
		/// <param name="moqTypeLoader">Moq Type Loader</param>

		public WorkspaceControllerLogicSpaceSystems(IWorkspaceDTODataLoader workspaceLoader,
			IUserDTODataLoader inuserLoader,
			IResourceDTODataLoader inResourceLoader,
			ITMResourceRateDTODataLoader inTmResourceRateDataLoader,
			BoeTaskElementRecalculation inBoeTaskElementRecalc,
			IInUseDataLoader inUseDataLoader,
			IFullObjectFactory factory,
			ICommonDataMapper inCommonDataMapper,
			IPermissionsDTODataLoader inPermissionLoader,
			FullBoeDataImporter inBoeImporter,
			IBOELaborControllerLogic inBOELaborControllerLogic,
			IFullWorkspaceRecalculation fullWsRecalc,
			IWorkspaceVariableDTODataLoader inWorkspaceVariableLoader,
			ICustomFieldValueDTODataLoader inCustomFieldValueLoader,
			ICustomFieldDTODataLoader customFieldLoader,
			IProjectMapDataLoader projectMapDataLoader,
			IPickListMapper boePickListMapper,
			IPickListMapper ptmPickListMapper,
			ContractTypeLoader contractTypeLoader,
			WorkspaceExporter workspaceExporter,
			IMoqTypeDataLoader moqTypeLoader,
			IBOEStateMachine boeStateMachine,
			IBoeMediator boeMediator)
			: base(
				workspaceLoader,
				inuserLoader,
				inResourceLoader,
				inTmResourceRateDataLoader,
				inBoeTaskElementRecalc,
				inUseDataLoader,
				factory,
				inCommonDataMapper,
				inPermissionLoader,
				inBoeImporter,
				inBOELaborControllerLogic,
				fullWsRecalc,
				inWorkspaceVariableLoader,
				inCustomFieldValueLoader,
				customFieldLoader,
				projectMapDataLoader,
				boePickListMapper,
				ptmPickListMapper,
				contractTypeLoader,
				workspaceExporter,
				moqTypeLoader,
				boeStateMachine,
				boeMediator
		)
		{
			// nothing to do here
		}

		/// <summary>
		/// Creates the <see cref="IWorkspaceIdentificationModelView"/> for the Space Systems mode
		/// </summary>
		/// <param name="workspace">The <see cref="FullWorkspace"/> used to populate the model view</param>
		/// <returns>A populated <see cref="IWorkspaceIdentificationModelView"/> for the Space Systems mode</returns>
		public override IWorkspaceIdentificationModelView GetWorkspaceIdentificationModelView(FullWorkspace workspace)
		{
			if (workspace == null)
			{
				throw new ArgumentNullException(nameof(workspace));
			}

			UserDTO costVolumeLeadDTO = this.UserLoader.GetUserByID(workspace.CostVolumeLeadPricerUserID);

			return new WorkspaceIdentificationSpaceModelView(workspace, costVolumeLeadDTO);
		}

		/// <summary>
		/// Creates the <see cref="CreateWorkspaceSpaceModelView"/> for the SSC mode
		/// </summary>
		/// <returns>A new <see cref="CreateWorkspaceSpaceModelView"/></returns>
		public override ICreateWorkspaceModelView GetCreateWorkspaceModelView()
		{
			return new CreateWorkspaceSpaceModelView();
		}

		/// <summary>
		/// Returns a dictionary of resource primary keys and ids for the workspace.  This is specific subset for the workspace resource rate page.
		/// </summary>
		/// <param name="workspace">The workspace.</param>
		/// <returns>Returns a dictionary of resource primary keys and ids for the workspace.</returns>
		public override IDictionary<string, Tuple<bool, int>> GetWorkspaceResourcesForWorkspaceResourceRateTM(FullWorkspace workspace)
		{
			if (workspace == null)
			{
				throw new ArgumentNullException(nameof(workspace));
			}

			SortedDictionary<string, Tuple<bool, int>> resources = new SortedDictionary<string, Tuple<bool, int>>();

			// get resources that allow mapping
			ResourceDTO[] nonMappableResources = (from x in workspace.ResourcesForWsResourceListId
										where (x.ElementOfCost == ElementOfCostType.Sub ||
										x.ElementOfCost == ElementOfCostType.IWTA) &&
										(x.RateType == RateType.Hours)
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
			// For Space Systems, a BOE can only be exported for Workoffline if they are the author or workspace administrator.
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
		/// <param name="ws">Workspace - not used in SSC</param>
		/// <returns>Collection of template types</returns>
		public override ICollection<ExcelReportTemplateType> GetPicklistReportTemplateTypes(FullWorkspace ws)
		{
			Collection<ExcelReportTemplateType> toReturn = new Collection<ExcelReportTemplateType> { ExcelReportTemplateType.MASTER };

			return toReturn;
		}

		/// <summary>
		/// Gets the Workspace Identification view name for the SSC mode
		/// </summary>
		public override string WorkspaceIdentificationViewName
		{
			get
			{
				return WebConstants.VIEW_WORKSPACE_IDENTIFICATION_SPACE;
			}
		}

		/// <summary>
		/// Populates the company specific Workspace properties
		/// </summary>
		/// <param name="theModel">The model view containing the data</param>
		/// <param name="workspace">The <see cref="WorkspaceDTO"/> to be populated</param>
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

			CreateWorkspaceSpaceModelView theModelSsc = theModel as CreateWorkspaceSpaceModelView;

			if (theModelSsc != null)
			{
				workspace.ProposalClass = new PickListDto { Id = theModelSsc.ProposalClass };
				workspace.SelectedContractTypes = theModelSsc.SelectedContractTypes;
				workspace.RevisedSubmittalDate = !string.IsNullOrEmpty(theModelSsc.RevisedSubmittalDate)
					? (DateTime?)Convert.ToDateTime(theModelSsc.RevisedSubmittalDate) : null;
			}
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

			WorkspaceIdentificationSpaceModelView theModelSsc = theModel as WorkspaceIdentificationSpaceModelView;

			if (theModelSsc != null)
			{
				workspace.ProposalClass = new PickListDto { Id = theModelSsc.ProposalClassType };
				workspace.SelectedContractTypes = theModelSsc.SelectedContractTypes;
				workspace.ProposalTitle = theModelSsc.ProposalTitle;
				if (!string.IsNullOrWhiteSpace(theModelSsc.RevisionWorkspaceName))
				{
					workspace.WorkspaceName = theModelSsc.WorkspaceName + theModelSsc.RevisionWorkspaceName;
				}

				if (Utilities.IsPTMIntegrated && theModelSsc.TrackingNumber != workspace.TrackingNumber)
				{
					// only change the URL/Shortname if integrated into PTM and the trackingnumber changed
					workspace.Shortname = theModelSsc.ShortName;
				}

				workspace.TrackingNumber = theModelSsc.TrackingNumber;
				workspace.RevisedSubmittalDate = !string.IsNullOrEmpty(theModelSsc.RevisedSubmittalDate)
					? (DateTime?)Convert.ToDateTime(theModelSsc.RevisedSubmittalDate) : null;
			}
		}

		/// <summary>
		/// Performs Validation for SaveWorkspaceIdentification
		/// </summary>
		/// <param name="ws">FullWorkspace</param>
		/// <param name="workspaceDetails">IWorkspaceIdentificationModelView</param>
		/// <param name="isAdmin">Is System Admin?</param>
		/// <param name="ptmTrackingNumberNotRequired">Is PTM Tracking Number Not Required</param>
		/// <returns>Validation messages for the save.</returns>
		public override ICollection<ValidationMessage> SaveWorkspaceIdentificationValidation(FullWorkspace ws, IWorkspaceIdentificationModelView workspaceDetails, bool isAdmin, bool ptmTrackingNumberNotRequired)
		{
			if (ws == null)
			{
				throw new ArgumentNullException(nameof(ws));
			}

			if (workspaceDetails == null)
			{
				throw new ArgumentNullException(nameof(workspaceDetails));
			}

			Collection<ValidationMessage> validationMessage = base.SaveWorkspaceIdentificationValidation(ws, workspaceDetails, isAdmin).ToCollection();

			// sequence of contractTypes set to be deleted
			ICollection<int> contractTypeDeleteList = ws.SelectedContractTypes
				.Except(((WorkspaceIdentificationSpaceModelView)workspaceDetails).SelectedContractTypes).ToList();

			// sequence of contractTypes in our contractTypeDeleteList that are associated w/CLINS
			ICollection<int> contractTypeAssociatedWithClinsList = ws.Clins
				.Where(x => contractTypeDeleteList.Contains(x.ContractType))
				.Select(x => x.ContractType).ToList();

			if (contractTypeAssociatedWithClinsList.Any())
			{
				validationMessage.Add(new ValidationMessage("SelectedContractTypes", "Cannot remove Contract Types at Workspace level when in use at CLIN level: " + string.Join(",", contractTypeAssociatedWithClinsList)));
			}

			if (Utilities.IsPTMIntegrated &&
				// We bypass this check for existing workspaces (without initial tracking number)
				(!isAdmin && !ptmTrackingNumberNotRequired && !string.IsNullOrEmpty(ws.TrackingNumber) && string.IsNullOrEmpty(workspaceDetails.TrackingNumber))
				// Value typed in by the user did not match available options 
				|| workspaceDetails.TrackingNumber == "INVALID")
			{
				validationMessage.Add(new ValidationMessage("TrackingNumber", "Please select a valid Tracking Number"));
			}

			return validationMessage;
		}
	}
}

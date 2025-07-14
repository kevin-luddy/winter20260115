// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using GenBOE.ActionLogic.BLL;
	using GenBOE.ActionLogic.BOETransitions;
	using GenBOE.ActionLogic.ControllerLogic;
	using GenBOE.ActionLogic.IO.Export;
	using GenBOE.ActionLogic.IO.Import;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.Validation;
	using GenBOE.ActionLogic.WBS.BOE;
	using GenBOE.DataBridge.Common;
	using GenBOE.DataBridge.DTO;
	using GenBOE.DataBridge.DTO.Request;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.classes;

	public class BOELaborControllerLogicSpace : BOELaborControllerLogic
	{
		/// <summary>
		/// Injection constructor
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
		/// <param name="perfOrgLoader"></param>
		/// <param name="inTaskElementDataLoader"></param>
		/// <param name="inPermissionsLoader"></param>
		/// <param name="inTaskVariableLoader"></param>
		/// <param name="iesSapClient">IES SAP Client</param>
		/// <param name="tokenservice">Token Service</param>
		/// <param name="cache">Cache</param>
		/// <param name="skillMixDTOLoader"></param>
		public BOELaborControllerLogicSpace(
			Common.Calculations.BoeTaskElementRecalculation inBoeTaskElementRecalc,
			IBOEStateMachine inBoeStateMachine,
			IBoeMediator inBoeMediator,
			IBoeTaskElementMediator inBoeTaskElementMediator,
			Common.Calculations.IVariableSelectBOEtoSumCalculation inVarSelectBoeToSumCalc,
			IResourceDTODataLoader inResourceLoader,
			IFullObjectFactory inFactory,
			IWorkspaceVariableDTODataLoader inWorkspaceVariableDTODataLoader,
			IUserDTODataLoader inuserLoader,
			IBoeDTODataLoader inBoeLoader,
			IBoeTaskElementDTODataLoader inTaskElementDataLoader,
			IPermissionsDTODataLoader inPermissionsLoader,
			IPerformingOrgDTODataLoader perfOrgLoader,
			IOrdinaryVariableLoader inTaskVariableLoader,
			TaskElementValidation taskElementValidation,
			IVariableCircularReferenceChecker circularReferenceChecker,
			ICommonDataMapper commonDataMapper,
			IRteTemplateDataLoader rteTemplateDataLoader,
			IMoqTypeDataLoader moqTypeDataLoader,
			ITMResourceRateDTODataLoader tmResourceRateDTODataLoader,
			IRequestDataLoader requestDataLoader,
			IValidateBOE validateBOE,
			IMoqTableExporter moqTableExporter,
			IMoqTableImporter moqTableImporter,
			GenBOE.ActionLogic.IESSAPClient.IESSAPClient iesSapClient,
			ITokenService tokenservice,
			ICache cache) : base(inBoeTaskElementRecalc,
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
				rteTemplateDataLoader,
				moqTypeDataLoader,
				tmResourceRateDTODataLoader,
				requestDataLoader,
				validateBOE,
				moqTableExporter,
				moqTableImporter,
				iesSapClient,
				tokenservice,
				cache)
		{
		}

		/// <summary>
		/// THESE ARE LEGACY MOQ TYPES AS OF 10/2020
		/// 
		/// Gets the valid <see cref="MOQType" />'s for the SSC configuration
		/// </summary>
		/// <returns>
		/// valid <see cref="MOQType" />'s for this company configuration
		/// </returns>
		internal override ICollection<MOQType> GetMOQTypes()
		{
			return new MOQType[]
			{
				MOQType.SSCAnalogySimilarTo,
				MOQType.SSCBottomUp,
				MOQType.SSCCostEstimatingRelationships,
				MOQType.SSCHistoricalExperienceFactor,
				MOQType.SSCLaborStandardsAndRealizationPerformanceFactors,
				MOQType.SSCLevelOfEffortSupport,
				MOQType.SSCDataDrivenCostModelsEquations,
				MOQType.SSCActual,
				MOQType.SSCQuote
			};
		}

		/// <summary>
		/// Returns the correct MOQ help text for the Space Systems configuration
		/// </summary>
		/// <returns>Space Systems configuration MOQ help text</returns>
		public override string GetMOQTypesHelpText()
		{
			return CommonConstants.BOE_MOQ_TYPES_HELP_TEXT_SPACE_SYSTEMS;
		}

		/// <summary>
		/// Returns the correct MOQ Equation label text for the Space Systems configuration
		/// </summary>
		/// <returns>Space Systems configuration MOQ Equation label text</returns>
		public override string GetMOQEquationLabel()
		{
			return CommonConstants.BOE_MOQ_EQUATION_LABEL;
		}

		/// <summary>
		/// Returns the correct MOQ text label for the Space Systems configuration
		/// </summary>
		/// <returns>Space Systems configuration MOQ text label text</returns>
		public override string GetMOQTextLabel()
		{
			return CommonConstants.BOE_MOQ_TEXT_LABEL_SPACE_SYSTEMS;
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
			// Bug 26713: Should be editable under the following conditions:
			//   Workspace is either working or locked AND boe is in draft
			//   AND user is boe author (including subcontractor authors) 
			//      OR (bug 27457) if the app is in "space-mode", author could also be a workspace admin
			if ((ws.WorkspaceState == WorkspaceState.Locked || ws.WorkspaceState == WorkspaceState.Working)
				&& (boe.State == BOEState.DraftLocked || boe.State == BOEState.Draft))
			{
				UserDTO currentUser = this.UserLoader.GetUserForActiveUser();
				Collection<PermissionsDTO> boePermissions = this.PermissionsLoader.GetBOEPermissions(new List<int>() { boe.Id });
				Collection<PermissionsDTO> wsPermissions = this.PermissionsLoader.GetWorkspacePermissions(ws.Id);

				bool isWsAdmin = wsPermissions.Any(x => x.ETIUserId == currentUser.UserID && x.WorkspaceId == ws.Id && x.Role == Role.WorkspaceAdmin);
				bool hasAppropriateBoePermissions = boePermissions.Any(x => x.ETIUserId == currentUser.UserID && (x.Role == Role.Author || x.Role == Role.SubcontractorAuthor));

				if (isWsAdmin || hasAppropriateBoePermissions)
				{
					toReturn = true;
				}
			}

			return toReturn;
		}
	}
}

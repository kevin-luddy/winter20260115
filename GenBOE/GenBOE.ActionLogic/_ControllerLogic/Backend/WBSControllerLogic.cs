// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.ActionLogic.ControllerLogic.Backend
{
	using GenBOE.ActionLogic.BLL;
	using GenBOE.ActionLogic.BOETransitions;
	using GenBOE.ActionLogic.Common;
	using GenBOE.ActionLogic.Common.Calculations;
	using GenBOE.ActionLogic.Common.Email;
	using GenBOE.ActionLogic.IO.Export;
	using GenBOE.ActionLogic.Validation;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.Exceptions;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.IO;
	using System.Linq;
	using System.Transactions;

	/// <summary>
	/// WBS Controller Logic
	/// </summary>
	public class WBSControllerLogic
	{
		#region Properties and Constructor
		/// <summary>
		/// Full object factory
		/// </summary>
		protected IFullObjectFactory factory { get; set; }

		/// <summary>
		/// Validation Helper
		/// </summary>
		protected IValidationHelper validationHelper { get; set; }

		/// <summary>
		/// Variable Select BOE to Sum Calculator
		/// </summary>
		protected IVariableSelectBOEtoSumCalculation variableSelectBOEtoSumCalculation { get; set; }

		/// <summary>
		/// BOE Task Element Recalculation
		/// </summary>
		protected IBoeTaskElementRecalculation boeTaskElementRecalculation { get; set; }

		/// <summary>
		/// BOE Task Element Mediator
		/// </summary>
		private IBoeTaskElementMediator boeTaskElementMediator { get; set; }

		/// <summary>
		/// BOE State Machine
		/// </summary>
		private IBOEStateMachine boeStateMachine { get; set; }

		/// <summary>
		/// BOE Mediator
		/// </summary>
		private IBoeMediator boeMediator { get; set; }

		/// <summary>
		/// WBS DTO Data Loader
		/// </summary>
		private IWbsDTODataLoader wbsLoader { get; set; }

		/// <summary>
		/// Workspace Variable DTO Data Loader
		/// </summary>
		private IWorkspaceVariableDTODataLoader workspaceVariableLoader { get; set; }

		/// <summary>
		/// BOE DTO Data Loader
		/// </summary>
		private IBoeDTODataLoader boeLoader;

		/// <summary>
		/// BOE Emailer
		/// </summary>
		private IBoeEmailer emailer { get; set; }

		/// <summary>
		/// WBS Exporter
		/// </summary>
		private WbsExporter wbsExporter { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public WBSControllerLogic(IFullObjectFactory factory, IValidationHelper validationHelper,
			IVariableSelectBOEtoSumCalculation variableSelectBOEtoSumCalculation, IBoeTaskElementRecalculation boeTaskElementRecalculation,
			IBoeTaskElementMediator boeTaskElementMediator, IBOEStateMachine boeStateMachine, IBoeMediator boeMediator,
			IWbsDTODataLoader wbsDTODataLoader, IWorkspaceVariableDTODataLoader workspaceVariableDTODataLoader,
			IBoeDTODataLoader boeLoader, WbsExporter wbsExporter, IBoeEmailer boeEmailer)
		{
			this.factory = factory;
			this.validationHelper = validationHelper;
			this.variableSelectBOEtoSumCalculation = variableSelectBOEtoSumCalculation;
			this.boeTaskElementRecalculation = boeTaskElementRecalculation;
			this.boeTaskElementMediator = boeTaskElementMediator;
			this.boeStateMachine = boeStateMachine;
			this.boeMediator = boeMediator;
			this.wbsLoader = wbsDTODataLoader;
			this.workspaceVariableLoader = workspaceVariableDTODataLoader;
			this.boeLoader = boeLoader;
			this.wbsExporter = wbsExporter;
			this.emailer = boeEmailer;
		}
		#endregion

		/// <summary>
		/// Export WBS logic
		/// </summary>
		/// <param name="ws">Full Workspace</param>
		/// <returns>Excel file as FileStream</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public FileStream ExportWBSs(FullWorkspace ws)
		{
			if (ws == null)
			{
				throw new ArgumentNullException(nameof(ws));
			}

			FileStream fs = null;
			// Perform Action
			// Get Default Performing Orgs Data
			ICollection<FullWbs> WbsDTOs = ws.WbsElementsNoMultiWbs.ToCollection();

			Dictionary<int, string> clinStrings = new Dictionary<int, string>();

			foreach (FullWbs wbs in WbsDTOs)
			{
				ICollection<ClinDTO> clins = ws.Clins.Where(x => wbs.ClinIDs.Contains(x.Id)).ToList<ClinDTO>();

				string clinString = string.Join(", ", clins.Select(w => w.ClinNumber));
				clinStrings.Add(wbs.Id, clinString);
			}

			// Get the WBS template file name
			// Assume that "Templates" is a subdirectory of your application's root directory
			string templateDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Export");

			// Ensure the template directory exists
			if (!Directory.Exists(templateDir))
			{
				throw new InvalidOperationException($"Template directory '{templateDir}' does not exist.");
			}

			// Get WBS template file name
			string templateFileName = Path.Combine(templateDir, "WBSs.xlsx");

			// Call the export function in the business layer and get back the file name of the populated template.
			string exportedFileName = wbsExporter.ExportToExcelFile(templateFileName, WbsDTOs.ToCollection<WbsDTO>(), clinStrings);

			fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
			return fs;
		}

		/// <summary>
		/// Export WBS template logic
		/// </summary>
		/// <returns>Excel file as FileStream</returns>
		public FileStream ExportWBSTemplate()
		{
			Collection<WbsDTO> WbsDTOs = new Collection<WbsDTO>();

			Dictionary<int, string> clinStrings = new Dictionary<int, string>();

			// Get the WBS template file name
			// Assume that "Templates" is a subdirectory of your application's root directory
			string templateDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Export");

			// Ensure the template directory exists
			if (!Directory.Exists(templateDir))
			{
				throw new InvalidOperationException($"Template directory '{templateDir}' does not exist.");
			}

			// Get WBS template file name
			string templateFileName = Path.Combine(templateDir, "WBSs.xlsx");

			// Call the export function in the business layer and get back the file name of the populated template.
			string exportedFileName = wbsExporter.ExportToExcelFile(templateFileName, WbsDTOs.ToCollection<WbsDTO>(), clinStrings);

			FileStream fs = new FileStream(exportedFileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
			return fs;
		}

		/// <summary>
		/// Save WBS
		/// </summary>
		/// <param name="ws">Full Workspace</param>
		/// <param name="wbsToBeUpserted">WBS to be upserted</param>
		/// <returns></returns>
		public void SaveWBS(FullWorkspace ws, ManageWBSModelView wbsToBeUpserted)
		{
			// Perform Action
			// check if the input is null
			if (wbsToBeUpserted == null)
			{
				throw new ArgumentNullException(nameof(wbsToBeUpserted));
			}

			if (ws == null)
			{
				throw new ArgumentNullException(nameof(ws));
			}

			Collection<FullBoe> multiBOEs = ws.Boes.Where(b => b.IsMultiClinWbs).ToCollection();
			Collection<ValidationMessage> validationErrors = ValidateWBS(ws, wbsToBeUpserted, multiBOEs);
			if (validationErrors.Count > 0)
			{
				throw new GenValidationException(validationErrors);
			}

			// save wbs
			// if the WBS # is changed, this could effect any BOE Task Element that has a task sum of BOE variable that uses this WBS
			List<int> WbsIDsToRecalculateLaborSpread = new List<int>();
			//associated BOEs to the WBS that should be sent an email if WBS # or title has changed

			Collection<BoeDTO> newBOEs = new Collection<BoeDTO>();
			Collection<FullBoe> boesWithStateChange = new Collection<FullBoe>();
			Collection<FieldChanged> changed = new Collection<FieldChanged>();


			FullWbs wbsObjects = this.factory.CreateFullWbs(wbsToBeUpserted.WbsID);

			WbsDTO wbsDTOs = this.ConvertToWBSDTO(wbsToBeUpserted, ws, WbsIDsToRecalculateLaborSpread, newBOEs, boesWithStateChange, changed, wbsObjects);

			// BOE's Labor Spread values that need to be recalculated because of any changes made during this save
			List<BoeTaskElementDTO> boeTaskElementsToRecalculate = new List<BoeTaskElementDTO>();

			// Dictionary to keep track of task variable IDs that need to be updated and their old variable total
			Dictionary<int, decimal> WorkspaceVarOldValueD = new Dictionary<int, decimal>();

			ICollection<FullWbs> wbsObjectsForLaborSpreadRecalculation = this.factory.CreateFullWbses(WbsIDsToRecalculateLaborSpread);

			//find any boes that have resources using the clin
			//for each wbs lets check to see if its being used by a multiboe
			Collection<FullBoe> boesUsingWbs =
							(from b in multiBOEs
							 from l in b.TaskElements
							 from x in l.taskElementLabors
							 where x.WBSID.HasValue && x.WBSID.Value == wbsObjects.Id
							 select b).ToCollection<FullBoe>();

			//filter out the duplicates 
			boesUsingWbs = boesUsingWbs.Distinct().ToCollection();

			foreach (FullWbs wbs in wbsObjectsForLaborSpreadRecalculation)
			{
				if (wbs.WorkspaceVariableIds.Any())
				{
					ICollection<WorkspaceVariableDTO> workspaceVariables = ws.WorkspaceVariables.Where(i => wbs.WorkspaceVariableIds.Contains(i.Id)).ToCollection<WorkspaceVariableDTO>();
					foreach (WorkspaceVariableDTO workspaceVar in workspaceVariables)
					{
						DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
						data.FillData(null, new List<WorkspaceVariableDTO>() { workspaceVar }, ws);

						decimal oldTotalValue = this.variableSelectBOEtoSumCalculation.GetWorkspaceVarLabelTotal(workspaceVar, data);
						WorkspaceVarOldValueD.Add(workspaceVar.Id, oldTotalValue);
					}
				}
			}

			List<ChangedValueContainer> SendBOEUpdatedToAuthorsAndApproversEmails = new List<ChangedValueContainer>();

			ws.LoadBoes();

			using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
			{
				this.wbsLoader.Save(wbsDTOs);

				foreach (FullWbs wbsObject in wbsObjectsForLaborSpreadRecalculation)
				{

					boeTaskElementsToRecalculate.AddRange(this.boeTaskElementRecalculation.RecalculateLaborWithWBS(wbsObject, VariableType.Task, ws));
					boeTaskElementsToRecalculate.AddRange(this.boeTaskElementRecalculation.RecalculateLaborWithWBS(wbsObject, VariableType.Workspace, ws));
				}

				// get the unique BOE IDs from boeTaskElementsToRecalculate so we can set their state back to Draft
				Collection<int> BoeIDsToCheck = new Collection<int>(boeTaskElementsToRecalculate.Where(x => x.BoeID > 0).Select(x => x.BoeID).ToList());

				ICollection<FullBoe> boes = ws.Boes.Where(x => BoeIDsToCheck.Contains(x.Id)).ToList();
				//get all the boes that need to be sent back to draft.
				boes = boes.Concat(boesUsingWbs).ToCollection();


				foreach (FullBoe boe in boes)
				{
					BOEState oldBOEState = boe.State;
					if (boe.State == BOEState.Approved || boe.State == BOEState.AwaitingApproval || boe.State == BOEState.DraftLocked)
					{
						BOEState newBOEState = BOEState.Draft;

						// Validate the Awaiting Approval or Approved to Draft state transition
						string validationMessage;

						if (!this.boeStateMachine.PerformStateTransitionValidation(boe, ws, oldBOEState, newBOEState, out validationMessage))
						{
							// not valid ... communicate to user
							throw new GenValidationException(validationMessage);
						}

						// If the transition is valid, set the BOE to Draft and save it
						boe.Updateable = UpdateType.Upsert;
						boe.State = newBOEState;
						this.boeMediator.MediatedSave(ws, boe);

						// Perform common state transition actions
						this.boeStateMachine.PerformStateTransitionAction(boe, ws, oldBOEState, boe.State);
					}
				}

				foreach (FullBoe boe in boesWithStateChange)
				{
					BOEState currentState = boe.State;
					BOEState newBOEState = BOEState.Draft;
					// move the BOE back to DRAFT
					boe.State = newBOEState;

					changed.Add(new FieldChanged
					{
						Field = "Status",
						OldValue = currentState.ToString(),
						NewValue = boe.State.ToString()
					});

					boe.Updateable = UpdateType.Upsert;

					string errorMessage;
					if (this.boeStateMachine.PerformStateTransitionValidation(boe, ws, currentState, boe.State, out errorMessage))
					{
						this.boeMediator.MediatedSave(ws, boe);

						// save of BOE worked .. perform transition steps and send email
						this.boeStateMachine.PerformStateTransitionAction(boe, ws, currentState, boe.State);

						SendBOEUpdatedToAuthorsAndApproversEmails.Add(new ChangedValueContainer
						{
							BOE = boe,
							Changes = changed
						});
					}
					else
					{
						// we can't move the BOE back to DRAFT for some reason ... abort
						// pull this error message from the state machine itself
						throw new ValidationException(errorMessage);
					}
				}

				this.boeTaskElementMediator.MediatedSaveTaskElements(new Collection<BoeTaskElementDTO>(boeTaskElementsToRecalculate), ws);
				if (WorkspaceVarOldValueD.Any())
				{
					ICollection<WorkspaceVariableDTO> workspaceVariables = ws.WorkspaceVariables.Where(i => WorkspaceVarOldValueD.Keys.Contains(i.Id)).ToCollection<WorkspaceVariableDTO>();
					foreach (WorkspaceVariableDTO workspaceVar in workspaceVariables)
					{
						DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
						data.FillData(null, new List<WorkspaceVariableDTO>() { workspaceVar }, ws);

						workspaceVar.WorkspaceVariableValue = this.variableSelectBOEtoSumCalculation.GetWorkspaceVarLabelTotal(workspaceVar, data);
						workspaceVar.Updateable = UpdateType.Upsert;
						this.workspaceVariableLoader.SaveWorkspaceVariables(new Collection<WorkspaceVariableDTO> { workspaceVar });
					}
				}
				scope.Complete();
			}

			// Send BOE E-mails
			foreach (ChangedValueContainer SendBOEUpdatedToAuthorsAndApproversEmail in SendBOEUpdatedToAuthorsAndApproversEmails)
			{
				this.emailer.SendBOEUpdatedToAuthorsAndApprovers(this.factory.CreateFullBoe(SendBOEUpdatedToAuthorsAndApproversEmail.BOE), SendBOEUpdatedToAuthorsAndApproversEmail.Changes);
			}

			if (newBOEs.Any())
			{
				this.SaveNewBOEs(ws, newBOEs);
			}
		}

		/// <summary>
		/// Saves BOEs created from the ManageWBS Page
		/// </summary>
		/// <param name="ws">current workspace</param>
		/// <param name="newBOEs">new boeDTO's </param>
		/// <returns></returns>
		private Collection<BoeDTO> SaveNewBOEs(FullWorkspace ws, Collection<BoeDTO> newBOEs)
		{
			foreach (BoeDTO newBOE in newBOEs)
			{
				int xrefID = this.boeLoader.GetWbsClinBoeXrefId(newBOE.WBSID, newBOE.CLINID, null);
				newBOE.WCBID = xrefID;
			}

			newBOEs = new Collection<BoeDTO>((from b in newBOEs
											  where b.WCBID != 0
											  select b).ToArray());

			if (newBOEs.Any())
			{
				using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
				{
					this.boeMediator.MediatedSaveBOEs(ws, newBOEs);
					scope.Complete();
				}
			}
			return newBOEs;
		}

		/// <summary>
		/// Validates WBS
		/// </summary>
		/// <param name="ws">current workspace</param>
		/// <param name="wbs">wbs to save </param>
		/// <param name="multiBOEs">full boe collection </param>
		/// <returns>Validation messages if any exist</returns>
		private Collection<ValidationMessage> ValidateWBS(FullWorkspace ws, ManageWBSModelView wbs, Collection<FullBoe> multiBOEs)
		{
			Collection<ValidationMessage> errors = new Collection<ValidationMessage>();

			if (!wbs.Deleted && !string.IsNullOrEmpty(wbs.WbsNumber))
			{
				string message = validationHelper.WBSRenumberValidation(ws.Id, wbs.WbsID, wbs.WbsNumber);
				if (message != null)
				{
					errors.Add(new ValidationMessage("WbsNumber", message));
				}
			}
			else if (wbs.Deleted)
			{
				bool inUse = multiBOEs
					.SelectMany(b => b.TaskElements)
					.AsParallel()
					.Any(l => l.taskElementLabors.Any(x => x.WBSID == wbs.WbsID));

				if (inUse)
				{
					errors.Add(new ValidationMessage("InUse", $"'{wbs.WbsString}' cannot be deleted because it is being used."));
				}
			}

			return errors;
		}

		/// <summary>
		/// Converts ManageWBSModelView to WBSDTO
		/// </summary>
		/// <param name="wbsCollection">ManageWBSModelView collection</param>
		/// <param name="ws">current workspace</param>
		/// <param name="WbsIDsToRecalculateLaborSpread">WBS updated</param>
		/// <param name="newBOEs">NewBOES</param>
		/// <param name="boesWithStateChange">BOE's Chaning</param>
		/// <param name="changed">Field Change collection</param>
		/// <param name="wbsObjects">full objects</param>
		/// <returns></returns>
		private WbsDTO ConvertToWBSDTO(ManageWBSModelView manageWbsModelView, FullWorkspace ws, List<int> WbsIDsToRecalculateLaborSpread, Collection<BoeDTO> newBOEs, Collection<FullBoe> boesWithStateChange, Collection<FieldChanged> changed, FullWbs wbsObjects)
		{
			int boeIDSeed = -1;
			bool wbsNumberChanged = false;
			bool wbsTitleChanged = false;

			FullWbs wbs;
			if (manageWbsModelView.WbsID < 0 && !manageWbsModelView.Deleted)
			{
				wbs = this.factory.CreateFullWbs();
				wbs.Updateable = UpdateType.Upsert;
				wbs.Id = manageWbsModelView.WbsID;
				wbs.WorkspaceID = ws.Id;
			}
			else
			{
				wbs = wbsObjects;
				DataRelationshipVerifier.VerifyDataRelation(wbs, ws.Id);

				//mktodo check boes in workspace to see if they are multi and using 

				if (manageWbsModelView.Deleted)
				{
					wbs.Updateable = UpdateType.Deleted;
				}
				else
				{
					wbs.Updateable = UpdateType.Upsert;
				}

				if (wbs.WbsNumber != manageWbsModelView.WbsNumber)
				{
					changed.Add(new FieldChanged
					{
						Field = "WBS #",
						OldValue = wbs.WbsNumber,
						NewValue = manageWbsModelView.WbsNumber
					});

					wbsNumberChanged = true;
				}

				if (wbs.WbsTitle != manageWbsModelView.WbsTitle)
				{
					changed.Add(new FieldChanged
					{
						Field = "WBS Title",
						OldValue = wbs.WbsTitle,
						NewValue = manageWbsModelView.WbsTitle
					});

					wbsTitleChanged = true;
				}
			}

			// if the WBS# has changed, need to recalculate labor spread for any BOE that had either the old WBS or the new WBS referenced
			// need to find the parent and children of the old wbs and the new wbs
			if (wbsNumberChanged)
			{
				FullWbs wbsObject = this.factory.CreateFullWbs(wbs);

				WbsIDsToRecalculateLaborSpread.Add(wbs.Id);
				WbsIDsToRecalculateLaborSpread.AddRange(wbsObject.AllParentWbs.Select(a => a.Id).ToList());
			}

			wbs.WbsNumber = manageWbsModelView.WbsNumber;
			wbs.WbsTitle = manageWbsModelView.WbsTitle;
			wbs.UpdateDate = manageWbsModelView.UpdateDate;

			if (manageWbsModelView.ClinIDs != null && manageWbsModelView.ClinIDs.Any())
			{
				Collection<int> wbsCLINs = new Collection<int>();
				foreach (int clinID in manageWbsModelView.ClinIDs)  // for each NEWLY-selected CLIN
				{
					ClinDTO selectedClin = this.factory.CreateFullClin(clinID);
					DataRelationshipVerifier.VerifyDataRelation(selectedClin, ws.Id);
					wbsCLINs.Add(clinID);

					if (manageWbsModelView.HasBOE && !manageWbsModelView.Deleted && manageWbsModelView.WbsID >= 0 && !wbs.ClinIDs.Contains(clinID))
					{
						// Business rule:  Automatically create a BOE when a new CLIN is assigned to a WBS that already has associated BOEs.
						// This seems to have existed since v1.0, circa March 2011.

						BoeDTO newBOE = new BoeDTO();

						newBOE.Id = boeIDSeed;
						newBOE.CLINID = selectedClin.Id;
						newBOE.WBSID = wbs.Id;
						newBOE.WorkspaceID = ws.Id;
						newBOE.StartDate = selectedClin.StartDate.HasValue ? selectedClin.StartDate.Value : ws.ContractStartDate;
						newBOE.EndDate = selectedClin.EndDate.HasValue ? selectedClin.EndDate.Value : ws.ContractEndDate;
						newBOE.Updateable = UpdateType.Upsert;
						newBOE.WCBID = boeIDSeed--;

						newBOEs.Add(newBOE);
					}
				}

				wbs.ClinIDs = wbsCLINs;
			}
			else
			{
				wbs.ClinIDs = new Collection<int>();
			}

			// if the number/title changed, need to move the associated BOEs back to draft.                  
			if (manageWbsModelView.HasBOE && (wbsNumberChanged || wbsTitleChanged))
			{
				foreach (FullBoe boe in wbs.Boes)
				{
					boesWithStateChange.Add(boe);
				}
			}

			return wbs;
		}

		/// <summary>
		/// ChangedValueContainer - pulled from the front end. 
		/// </summary>
		private struct ChangedValueContainer
		{
			public BoeDTO BOE { get; set; }
			public Collection<FieldChanged> Changes { get; set; }
		}
	}

}

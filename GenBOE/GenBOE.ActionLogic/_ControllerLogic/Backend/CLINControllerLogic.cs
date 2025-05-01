// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.ActionLogic._ControllerLogic.Backend
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Transactions;
	using GenBOE.ActionLogic.BLL;
	using GenBOE.ActionLogic.BOETransitions;
	using GenBOE.ActionLogic.Common.Calculations;
	using GenBOE.ActionLogic.Common.Email;
	using GenBOE.ActionLogic.ModelView.Clin;
	using GenBOE.ActionLogic.Validation;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using GenBOE.Objects;
	using IES.Common;
	using IES.Common.Exceptions;
	using IES.Common.PickList;

	/// <summary>
	/// CLIN Controller Logic
	/// </summary>
	public class CLINControllerLogic
	{
		#region Properties and Constructor
		/// <summary>
		/// Full object factory
		/// </summary>
		protected IFullObjectFactory _factory { get; set; }

		/// <summary>
		/// Validation Helper
		/// </summary>
		protected IValidationHelper _validationHelper { get; set; }

		/// <summary>
		/// Variable Select BOE to Sum Calculator
		/// </summary>
		protected IVariableSelectBOEtoSumCalculation _variableSelectBOEtoSumCalculation { get; set; }

		/// <summary>
		/// BOE Task Element Recalculation
		/// </summary>
		protected IBoeTaskElementRecalculation _boeTaskElementRecalculation { get; set; }

		/// <summary>
		/// BOE Task Element Mediator
		/// </summary>
		private IBoeTaskElementMediator _boeTaskElementMediator { get; set; }

		/// <summary>
		/// BOE State Machine
		/// </summary>
		private IBOEStateMachine _boeStateMachine { get; set; }

		/// <summary>
		/// BOE Mediator
		/// </summary>
		private IBoeMediator _boeMediator { get; set; }

		/// <summary>
		/// CLIN DTO Data Loader
		/// </summary>
		private IClinDTODataLoader _clinLoader { get; set; }

		/// <summary>
		/// Workspace Variable DTO Data Loader
		/// </summary>
		private IWorkspaceVariableDTODataLoader _workspaceVariableLoader { get; set; }

		/// <summary>
		/// Contract Type Loader
		/// </summary>
		private ContractTypeLoader _contractTypeLoader { get; set; }

		/// <summary>
		/// BOE Emailer
		/// </summary>
		private IBoeEmailer _emailer { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public CLINControllerLogic(IFullObjectFactory factory, IValidationHelper validationHelper, 
			IVariableSelectBOEtoSumCalculation variableSelectBOEtoSumCalculation, IBoeTaskElementRecalculation boeTaskElementRecalculation, 
			IBoeTaskElementMediator boeTaskElementMediator, IBOEStateMachine boeStateMachine, IBoeMediator boeMediator, 
			IClinDTODataLoader clinDTODataLoader, IWorkspaceVariableDTODataLoader workspaceVariableDTODataLoader, 
			ContractTypeLoader contractTypeLoader, IBoeEmailer boeEmailer)
		{
			this._factory = factory;
			this._validationHelper = validationHelper;
			this._variableSelectBOEtoSumCalculation = variableSelectBOEtoSumCalculation;
			this._boeTaskElementRecalculation = boeTaskElementRecalculation;
			this._boeTaskElementMediator = boeTaskElementMediator;
			this._boeStateMachine = boeStateMachine;
			this._boeMediator = boeMediator;
			this._clinLoader = clinDTODataLoader;
			this._workspaceVariableLoader = workspaceVariableDTODataLoader;
			this._contractTypeLoader = contractTypeLoader;
			this._emailer = boeEmailer;
		}
		#endregion

		/// <summary>
		/// Save CLIN
		/// </summary>
		/// <param name="ws"></param>
		/// <param name="inUpdatedClin"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="GenValidationException"></exception>
		/// <exception cref="ValidationException"></exception>
		public ManageCLINModelView SaveCLIN(FullWorkspace ws, ManageCLINModelView inUpdatedClin)
		{
			ManageCLINModelView toReturn = new ManageCLINModelView();

			// Perform Action
			// check if the input is null
			if (inUpdatedClin == null)
			{
				throw new ArgumentNullException(nameof(inUpdatedClin));
			}

			if (ws == null)
			{
				throw new ArgumentNullException(nameof(ws));
			}

			// Dictionary to keep track of task variable IDs that need to be updated and their old variable total
			Dictionary<int, decimal> WorkspaceVarOldValueD = new Dictionary<int, decimal>();

			// BOE's Labor Spread values that need to be recalculated because of any changes made during this save
			List<BoeTaskElementDTO> boeTaskElementsToRecalculate = new List<BoeTaskElementDTO>();

			Collection<ValidationMessage> ValidationErrors = new Collection<ValidationMessage>();

			// only validate data if it exists
			if (inUpdatedClin.ClinID > 0 || (inUpdatedClin.ClinID < 0 && !inUpdatedClin.Deleted))
			{
				// Project updatedClin to be DTO
				FullClin updatedClin = inUpdatedClin.ClinID > 0 ? this._factory.CreateFullClin(inUpdatedClin.ClinID) : this._factory.CreateFullClin();
				updatedClin.ClinNumber = inUpdatedClin.ClinNumber;
				updatedClin.ClinTitle = inUpdatedClin.ClinTitle;
				updatedClin.Updateable = inUpdatedClin.Deleted ? UpdateType.Deleted : UpdateType.Upsert;
				updatedClin.UpdateDate = inUpdatedClin.UpdateDate;
				updatedClin.WorkspaceID = ws.Id;
				updatedClin.ContractType = inUpdatedClin.ContractType;

				// only apply dates if the CLIN is new or not in use
				if (updatedClin.Id < 0 || !updatedClin.InUse)
				{
					updatedClin.StartDate = string.IsNullOrEmpty(inUpdatedClin.StartDate) ? (DateTime?)null : inUpdatedClin.StartDate.ToDateTimeMidMonth();
					updatedClin.EndDate = string.IsNullOrEmpty(inUpdatedClin.EndDate) ? (DateTime?)null : inUpdatedClin.EndDate.ToDateTimeMidMonth();
				}

				// Don't bother validating deletions
				if (updatedClin.Updateable != UpdateType.Deleted)
				{
					// Validate CLIN
					CLINValidator validator = new CLINValidator(this._factory);
					Collection<string> validationerrors = validator.validation(updatedClin, (Collection<Dictionary<string, string>>)null);

					if (validationerrors.Count > 0)
					{
						foreach (string message in validationerrors)
						{
							ValidationErrors.Add(new ValidationMessage("Clin", message));
						}
					}

					// validate dates if its a new clin
					if (updatedClin.Id < 0)
					{
						string startEndDateValidation = this._validationHelper.StartEndDateValidation(updatedClin.StartDate, updatedClin.EndDate);

						if (startEndDateValidation != null)
						{
							ValidationErrors.Add(new ValidationMessage("Clin", startEndDateValidation));
						}
					}
				}

				//get the potential multiboes
				Collection<FullBoe> MultiBOEs = ws.Boes.Where(x => x.IsMultiClinWbs).ToCollection();

				//find any boes that have resources using the clin
				Collection<FullBoe> boesUsingClin = (from b in MultiBOEs
													 from l in b.TaskElements
													 from x in l.taskElementLabors
													 where x.CLINID.HasValue && x.CLINID == updatedClin.Id
													 select b)
													.ToCollection<FullBoe>();
				//if there are duplicates lets filter those out.
				boesUsingClin = boesUsingClin.Distinct().ToCollection<FullBoe>();

				//if we have any boes and the clin is being deleted stop the process
				if (boesUsingClin.Any() && updatedClin.Updateable == UpdateType.Deleted)
				{
					ValidationErrors.Add(new ValidationMessage("Clin", "'" + updatedClin.ClinString + "' cannot be deleted because it is being used by a Resource Type in a BOE that has Resource Level WBS/CLIN selected."));
				}

				if (ValidationErrors.Count > 0)
				{
					throw new GenValidationException(ValidationErrors);
				}

				ClinDTO oldClin = this._factory.CreateFullClin(inUpdatedClin.ClinID);

				// Save the CLINs
				using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
				{
					ICollection<WorkspaceVariableDTO> workspaceVariablesOld = new Collection<WorkspaceVariableDTO>();
					if (updatedClin.Updateable == UpdateType.Deleted)
					{
						if (updatedClin.WorkspaceVariableIds.Any())
						{
							workspaceVariablesOld = ws.WorkspaceVariables.Where(i => updatedClin.WorkspaceVariableIds.Contains(i.Id)).ToCollection<WorkspaceVariableDTO>();
							foreach (WorkspaceVariableDTO workspaceVar in workspaceVariablesOld)
							{
								DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
								data.FillData(null, new List<WorkspaceVariableDTO>() { workspaceVar }, ws);

								decimal oldTotalValue = this._variableSelectBOEtoSumCalculation.GetWorkspaceVarLabelTotal(workspaceVar, data);
								WorkspaceVarOldValueD.Add(workspaceVar.Id, oldTotalValue);
							}
						}

						boeTaskElementsToRecalculate.AddRange(this._boeTaskElementRecalculation.RecalculateLaborWithClin(updatedClin, VariableType.Task, ws));
						boeTaskElementsToRecalculate.AddRange(this._boeTaskElementRecalculation.RecalculateLaborWithClin(updatedClin, VariableType.Workspace, ws));

						// save all the task elements that were effected by a CLIN deletion
						this._boeTaskElementMediator.MediatedSaveTaskElements(new Collection<BoeTaskElementDTO>(boeTaskElementsToRecalculate), ws);
					}

					// get the unique BOE IDs from boeTaskElementsToRecalculate so we can set their state back to Draft
					Collection<int> BoeIDsToCheck = new Collection<int>(boeTaskElementsToRecalculate.Where(x => x.BoeID > 0).Select(x => x.BoeID).ToList());

					ICollection<FullBoe> boesToCheck = this._factory.CreateFullBoes(BoeIDsToCheck);
					//add on the boes with the multiboes
					boesToCheck = boesToCheck.Concat(boesUsingClin).ToCollection<FullBoe>();


					foreach (FullBoe boe in boesToCheck)
					{
						bool StateChange = false;
						BOEState oldBOEState = boe.State;
						BOEState newBOEState = BOEState.Draft;
						if (boe.State == BOEState.Approved || boe.State == BOEState.AwaitingApproval || boe.State == BOEState.DraftLocked)
						{
							// Validate the Awaiting Approval or Approved to Draft state transition
							string validationMessage = string.Empty;
							if (!this._boeStateMachine.PerformStateTransitionValidation(this._factory.CreateFullBoe(boe), ws, oldBOEState, newBOEState, out validationMessage))
							{
								throw new ValidationException(validationMessage);
							}

							// If the transition is valid, set the BOE to Draft and save it
							boe.State = newBOEState;
							StateChange = true;
						}

						if (StateChange)
						{
							boe.Updateable = UpdateType.Upsert;
							this._boeMediator.MediatedSave(ws, boe);
							this._boeStateMachine.PerformStateTransitionAction(this._factory.CreateFullBoe(boe), ws, oldBOEState, boe.State);
						}
					}

					this._clinLoader.Save(updatedClin);

					if (WorkspaceVarOldValueD.Any())
					{
						foreach (WorkspaceVariableDTO workspaceVar in workspaceVariablesOld)
						{
							DataClassForSumOfBOEsCalculation data = new DataClassForSumOfBOEsCalculation();
							data.FillData(null, new List<WorkspaceVariableDTO>() { workspaceVar }, ws);

							workspaceVar.WorkspaceVariableValue = this._variableSelectBOEtoSumCalculation.GetWorkspaceVarLabelTotal(workspaceVar, data);
							workspaceVar.Updateable = UpdateType.Upsert;
							this._workspaceVariableLoader.SaveWorkspaceVariables(new Collection<WorkspaceVariableDTO> { workspaceVar });
						}
					}

					scope.Complete();
				}

				// Send emails and change BOE statuses for In Use CLINs
				// This is purposely outside the transaction so that if the email fails to send then the transaction does not fail
				if (inUpdatedClin.ClinID > 0 && !inUpdatedClin.Deleted)
				{
					this.ProcessInUseUpdatedCLIN(ws, oldClin, this._factory.CreateFullClin(inUpdatedClin.ClinID));
				}

				if (inUpdatedClin.ClinID > 0 && !inUpdatedClin.Deleted)
				{
					// just make sure we have the latest version of the DTO. (the returned DTO has the updated ID, but not the updated UpdateDate)
					FullClin modifiedClin = this._factory.CreateFullClin(updatedClin.Id);
					ICollection<PickListDto> contractTypes = this._contractTypeLoader.GetPickListValues();
					string contract = Utilities.GetPickListText(modifiedClin.ContractType, contractTypes, Constants.CONTRACT_TYPE_NOT_SET_STRING);

					toReturn = new ManageCLINModelView(modifiedClin, contract);
				}
			}
			else
			{
				throw new GenValidationException("No CLINs have been updated as part of SaveEditCLIN");
			}

			return toReturn;
		}

		/// <summary>
		/// Checks an in use CLIN that was updated for changes. If there were changes, then authors and approvers of
		/// the CLIN's BOEs are notified accordingly. Awaiting Approval/Approved BOEs are moved back to draft
		/// </summary>
		/// <param name="currentWorkspace">The current workspace</param>
		/// <param name="oldCLIN">The old CLIN DTO</param>
		/// <param name="newCLIN">The new CLIN DTO</param>
		private void ProcessInUseUpdatedCLIN(FullWorkspace currentWorkspace, ClinDTO oldCLIN, FullClin newCLIN)
		{
			if (newCLIN.InUse) // check in use
			{
				IReadOnlyCollection<FullBoe> boesForUpdatedClin = newCLIN.Boes;

				foreach (FullBoe boeForClin in boesForUpdatedClin)
				{
					// check boe state
					if (boeForClin.State == BOEState.Draft || boeForClin.State == BOEState.DraftLocked || boeForClin.State == BOEState.AwaitingApproval || boeForClin.State == BOEState.Approved)
					{
						// CLIN was in use, BOE in draft state ... send the email!
						bool updateNeeded = false;

						// obtain the changed fields
						Collection<FieldChanged> changed = new Collection<FieldChanged>();
						if (oldCLIN.ClinNumber != newCLIN.ClinNumber)
						{
							updateNeeded = true;
							changed.Add(new FieldChanged
							{
								Field = "CLIN #",
								OldValue = oldCLIN.ClinNumber,
								NewValue = newCLIN.ClinNumber
							});
						}
						if (oldCLIN.ClinTitle != newCLIN.ClinTitle)
						{
							updateNeeded = true;
							changed.Add(new FieldChanged
							{
								Field = "CLIN Title",
								OldValue = oldCLIN.ClinTitle,
								NewValue = newCLIN.ClinTitle
							});
						}
						if (oldCLIN.StartDate != newCLIN.StartDate)
						{
							if (newCLIN.StartDate > boeForClin.StartDate || newCLIN.StartDate > boeForClin.EndDate)
							{
								updateNeeded = true;
							}
							changed.Add(new FieldChanged
							{
								Field = "CLIN Start Date",
								OldValue = oldCLIN.StartDate.HasValue ? oldCLIN.StartDate.Value.ToString("MM/yyyy") : string.Empty,
								NewValue = newCLIN.StartDate.HasValue ? newCLIN.StartDate.Value.ToString("MM/yyyy") : string.Empty
							});
						}
						if (oldCLIN.EndDate != newCLIN.EndDate)
						{
							if (newCLIN.EndDate < boeForClin.EndDate || newCLIN.EndDate < boeForClin.StartDate)
							{
								updateNeeded = true;
							}
							changed.Add(new FieldChanged
							{
								Field = "CLIN End Date",
								OldValue = oldCLIN.EndDate.HasValue ? oldCLIN.EndDate.Value.ToString("MM/yyyy") : string.Empty,
								NewValue = newCLIN.EndDate.HasValue ? newCLIN.EndDate.Value.ToString("MM/yyyy") : string.Empty
							});
						}
						if (oldCLIN.ContractType != newCLIN.ContractType)
						{
							updateNeeded = true;
							changed.Add(new FieldChanged
							{
								Field = "Contract Type",
								OldValue = oldCLIN.ContractType.ToString(),
								NewValue = newCLIN.ContractType.ToString()
							});
						}

						if (boeForClin.State == BOEState.AwaitingApproval || boeForClin.State == BOEState.Approved || boeForClin.State == BOEState.DraftLocked)
						{
							BOEState currentState = boeForClin.State;
							BOEState newBOEState = BOEState.Draft;
							// move the BOE back to DRAFT
							boeForClin.State = newBOEState;
							boeForClin.Updateable = UpdateType.Upsert;

							if (updateNeeded)
							{
								if (this._boeStateMachine.PerformStateTransitionValidation(this._factory.CreateFullBoe(boeForClin), currentWorkspace, currentState, boeForClin.State, out string errorMessage))
								{
									this._boeMediator.MediatedSave(currentWorkspace, boeForClin);

									// save of BOE worked .. perform transition steps and send email
									this._boeStateMachine.PerformStateTransitionAction(this._factory.CreateFullBoe(boeForClin), currentWorkspace, currentState, boeForClin.State);
								}
								else
								{
									// we can't move the BOE back to DRAFT for some reason ... abort
									// pull this error message from the state machine itself
									throw new ValidationException(errorMessage);
								}
							}
						}

						if (updateNeeded)
						{
							this._emailer.SendCLINUpdatedToAuthorsAndApprovers(this._factory.CreateFullBoe(boeForClin), changed);
						}
					}
				}
			}
		}

	}
}

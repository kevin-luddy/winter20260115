// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Threading.Tasks;
	using System.Transactions;
	using System.Web.Configuration;
	using System.Web.Mvc;
	using GenTRAC.ActionLogic.Email;
	using GenTRAC.ActionLogic.GeneralHelper;
	using GenTRAC.ActionLogic.Mediator;
	using GenTRAC.ActionLogic.ModelView;
	using GenTRAC.ActionLogic.ModelView.Contracts;
	using GenTRAC.ActionLogic.ModelView.Proposals;
	using GenTRAC.DataBridge.Common.Security;
	using GenTRAC.DataBridge.DTO;
	using GenTRAC.DataBridge.DTO.Contracts;
	using GenTRAC.Objects;
	using GenTRAC.Objects.FullObject;
	using IES.Common;

	/// <summary>
	/// Contracts controller logic
	/// </summary>
	public class ContractsControllerLogic : GenTRACControllerLogic
	{
		#region properties

		/// <summary>
		/// The logger
		/// </summary>
		private Logger log = new Logger(typeof(ContractsControllerLogic));

		/// <summary>
		/// the name of the contracts information form, needed for validation
		/// </summary>
		public const string CONTRACTS_INFO_FORM = "contractsInfoForm";

		/// <summary>
		/// The Contracts Loader
		/// </summary>
		private IContractsLoader contractsLoader = null;


		/// <summary>
		/// The Cage Codes Loader
		/// </summary>
		private ICageCodesLoader cageCodesLoader = null;

		/// <summary>
		/// The emailer.
		/// </summary>
		private readonly IPtmEmailer emailer;

		/// <summary>
		/// Injected Approvals Logic service
		/// </summary>
		private readonly ApprovalsControllerLogic approvalsLogic;

		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="securityAccess'">Security Access</param>
		/// <param name="proposalLoader">Proposal Loader</param>
		/// <param name="userMapper">User Mapper</param>
		/// <param name="objectFactory">Object Factory</param>
		/// <param name="approvalsLoader">Approvals Loader</param>
		/// <param name="proposalChecklistLoader">Proposal Checklist Loader</param>
		/// <param name="checklistMediator">Checklist Mediator</param>
		/// <param name="proposalMediator">Proposal Mediator</param>
		/// <param name="contractsLoader">Contracts Loader</param>
		/// <param name="inEmailer">Emailer</param>
		/// <param name="inApprovalsLogic">Injected Approvals Logic</param>
		public ContractsControllerLogic(
			ISecurityAccess securityAccess,
			IProposalLoader proposalLoader,
			IUserMapper userMapper,
			IFullObjectFactory objectFactory,
			IApprovalsLoader approvalsLoader,
			IProposalChecklistLoader proposalChecklistLoader,
			IChecklistMediator checklistMediator,
			IProposalMediator proposalMediator,
			IContractsLoader contractsLoader,
			ICageCodesLoader cageCodesLoader,
			IPtmEmailer inEmailer,
			ApprovalsControllerLogic inApprovalsLogic)
			: base(securityAccess, proposalLoader, userMapper, objectFactory, approvalsLoader, proposalChecklistLoader, checklistMediator, proposalMediator)
		{
			this.contractsLoader = contractsLoader;
			this.cageCodesLoader = cageCodesLoader;
			this.emailer = inEmailer;
			this.approvalsLogic = inApprovalsLogic;
		}

		/// <summary>
		/// Get Contracts Data
		/// </summary>
		/// <param name="proposalId">Proposal Id</param>
		/// <returns>Contracts Tab Data</returns>
		public async Task<ContractsModelView> GetDataForProposalContracts(int proposalId)
		{
			if (proposalId < 0)
			{
				throw new ArgumentNullException(nameof(proposalId));
			}

			FullProposal fullProposal = await GetFullProposalAsync(proposalId);
			ContractsDto dto = this.contractsLoader.GetContractForProposal(proposalId);
			ContractsModelView model = ConvertContractsDtoToModel(dto);

			// in the event that a contract entry hasn't yet been created for the proposal, create a blank one for the evaluation of calculated properties
			if (dto == null)
			{
				dto = new ContractsDto
				{
					ProposalId = proposalId
				};
			}

			// Get security info to ensure our read-only users can do just that
			SecurityPermissionsRequested perms = new SecurityPermissionsRequested
			{
				PageToCheck = PtmSecurityPage.Contracts,
				ProposalId = proposalId,
			};
			SecurityAuthorization highestAccess = this.SecurityAccess.IsAuthorized(perms, out _);
			
			// populate calculated properties
			model.EppOptions = this.GetEppSelectOptions(model.EppDelegationAuthority);
			model.InsuranceProposedDirectOptions = this.GetInsuranceProposedOptions(model.IsInsuranceDirect);
			model.InsuranceTypeOptions = this.GetInsuranceTypeOptions(model.InsuranceType);
			model.SetLostButtonEnabled = this.IsValidForLostStatus(dto, fullProposal);
			model.NoBidButtonEnabled = this.IsValidForNoBidStatus(fullProposal);
			model.CompleteButtonEnabled = this.IsValidForCompleteStatus(dto, fullProposal);
			model.IsNoBid = fullProposal.ProposalStatus == ProposalStatus.NoBid;
			model.HasAccessToSetNoBid = this.IsContractsUser(fullProposal.CurrentUser.Id, fullProposal.Permissions) || this.SecurityAccess.CurrentUserHasRole(PtmRole.Admin, null);
			model.IsReadOnly = fullProposal.ProposalStatus == ProposalStatus.Completed || highestAccess == SecurityAuthorization.Read;
			model.IsRomNte = fullProposal.IsRomNte;

			// Load additional values
			model.PreviouslySubmittedRoms = this.ProposalLoader.GetRomProposalOptions(model.PreviouslySubmittedROM);

			// Load in Cage Codes data
			model.CageCodes = GetAllCageCodesData();
			model.CageCode = dto.CageCode;

			if (model.PreviouslySubmittedROM.HasValue)
			{
				Tuple<DateTime?, decimal?> previousRomDateAndValue = this.GetRomDateAndValue(model.PreviouslySubmittedROM.Value);
				model.previousROMDt = previousRomDateAndValue?.Item1;
				model.PreviousROMValueDecimal = previousRomDateAndValue?.Item2;
			}
			if(this.SecurityAccess.CurrentUserHasRole(PtmRole.Admin, null))
			{
				model.IsReadOnly = false;
			}
			return model;
		}

		/// <summary>
		/// Get ROM Date and Value
		/// </summary>
		/// <param name="proposalId">(previously) selected rom (proposal id)</param>
		/// <returns>Submittal date and value</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public Tuple<DateTime?, decimal?> GetRomDateAndValue(int proposalId)
		{
			if (proposalId < 0)
			{
				throw new ArgumentNullException(nameof(proposalId));
			}

			return this.ProposalLoader.GetRomDateAndValue(proposalId);
		}

		/// <summary>
		/// Gets all the Cage Code data in a view model
		/// </summary>
		/// <returns>All cage codes view models</returns>
		public ICollection<CageCodeModelView> GetAllCageCodesData()
		{
			ICollection<CageCodeDTO> cageCodesDTOs = cageCodesLoader.GetAllCageCodesData();
			ICollection<CageCodeModelView> cageCodeViewModels = new List<CageCodeModelView>();

			foreach (CageCodeDTO cageCodesDTO in cageCodesDTOs)
			{
				cageCodeViewModels.Add(new CageCodeModelView()
				{
					CageCode = cageCodesDTO.CageCode,
					Address1 = cageCodesDTO.Address1,
					Address2 = cageCodesDTO.Address2,
					City = cageCodesDTO.City,
					State = cageCodesDTO.State,
					Zip = cageCodesDTO.Zip
				});
			}

			return cageCodeViewModels;
		}

		/// <summary>
		/// Get data by cage code
		/// </summary>
		/// <param name="cageCode"></param>
		/// <returns>Cage code view model</returns>
		public CageCodeModelView GetDataByCageCode(string cageCode)
		{
			CageCodeDTO cageCodesDTO = cageCodesLoader.GetDataByCageCode(cageCode);

			CageCodeModelView cageCodeViewModel = new CageCodeModelView()
			{
				CageCode = cageCodesDTO.CageCode,
				Address1 = cageCodesDTO.Address1,
				Address2 = cageCodesDTO.Address2,
				City = cageCodesDTO.City,
				State = cageCodesDTO.State,
				Zip = cageCodesDTO.Zip
			};

			return cageCodeViewModel;
		}

		#region Contract Validate / Save

		/// <summary>
		/// Perform validation of the Contracts ModelView that isn't covered by ModelState
		/// </summary>
		/// <param name="model">model to validate</param>
		/// <param name="proposal">Full proposal</param>
		/// <returns>collection of validation messages</returns>
		public ICollection<string> ValidateContractModelView(ContractsModelView model, FullProposal proposal)
		{
			_ = model ?? throw new ArgumentNullException(nameof(model));
			_ = proposal ?? throw new ArgumentNullException(nameof(proposal));
			ICollection<string> validationMessages = new Collection<string>();
			
			DateTime? dateSubmittedToContracts = proposal.ProposalChecklistData?.FirstOrDefault()?.EstimatingSubmitsToContractsDate;

			if (model.CustomerSubmittalDt.HasValue && dateSubmittedToContracts.HasValue
				&& model.CustomerSubmittalDt.Value.Date < dateSubmittedToContracts.Value.Date)
			{
				validationMessages.Add(Constants.INVALID_PROPOSAL_SUBMITTAL_DATE);
			}

			if (!proposal.IsRomNte)
			{
				if (model.NegotiationsSubmittedDt.HasValue && proposal.AgreementDate.HasValue
					&& model.NegotiationsSubmittedDt < proposal.AgreementDate)
				{
					validationMessages.Add(Constants.INVALID_NEGOTIATIONS_SUBMITTED);
				}
			}

			return validationMessages;
		}

		/// <summary>
		/// Saves the contract.
		/// </summary>
		/// <param name="proposalId">The proposal identifier.</param>
		/// <param name="model">The model.</param>
		public int? SaveContract(ContractsModelView model)
		{
			_ = model ?? throw new ArgumentNullException(nameof(model));

			int? contractId = null;

			using (StopwatchTimer sw = new StopwatchTimer("ContractsControllerLogic.SaveContract", this.log))
			{
				ContractsDto contract = this.ConvertContractsModelToDto(model);

				using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, Convert.ToInt32(WebConfigurationManager.AppSettings["TransactionTimeout"])) }))
				{
					contractId = this.contractsLoader.Save(contract);
					scope.Complete();
				}
			}

			return contractId;
		}

		/// <summary>
		/// Generate the options for the EPP option select box
		/// </summary>
		/// <param name="eppDelegationAuthority">The current value for the contract</param>
		/// <returns>Collection of SelectListItems</returns>
		public ICollection<SelectListItem> GetEppSelectOptions(EppDelegationAuthority? eppDelegationAuthority)
		{
			return this.contractsLoader.GetEppSelectValues(eppDelegationAuthority);
		}

		/// <summary>
		/// Gets the Insurance Type Options
		/// </summary>
		/// <param name="insuranceType"></param>
		/// <returns></returns>
		private ICollection<SelectListItem> GetInsuranceTypeOptions(InsuranceType? insuranceType)
		{
			ICollection<SelectListItem> result = new List<SelectListItem>();

			InsuranceType[] enums = (InsuranceType[])Enum.GetValues(typeof(InsuranceType));

			// add blank option
			result.Add(new SelectListItem { Value = null, Text = null, Selected = insuranceType == null });

			foreach (InsuranceType item in enums)
			{
				result.Add(new SelectListItem
				{
					Value = item.ToString(),
					Text = item.GetDescription<InsuranceType>(),
					Selected = item == insuranceType
				});
			}

			return result;
		}

		/// <summary>
		/// Gets the Insurance Proposed Options
		/// </summary>
		/// <param name="isInsuranceDirect"></param>
		/// <returns></returns>
		private ICollection<SelectListItem> GetInsuranceProposedOptions(TripleBooleanState? isInsuranceDirect)
		{
			ICollection<SelectListItem> result = new List<SelectListItem>();

			TripleBooleanState[] enums = (TripleBooleanState[])Enum.GetValues(typeof(TripleBooleanState));

			// add blank option
			result.Add(new SelectListItem { Value = null, Text = null, Selected = isInsuranceDirect == null });

			foreach (TripleBooleanState item in enums)
			{
				result.Add(new SelectListItem
				{
					Value = item.ToString(),
					Text = item.GetDescription<TripleBooleanState>(),
					Selected = item == isInsuranceDirect
				});
			}

			return result;
		}

		/// <summary>
		/// Orchestrates the setting of the proposal status to "Lost"
		/// </summary>
		/// <param name="proposalId">Proposal Id for status update</param>
		/// <param name="messages">List of validation errors (if applicable)</param>
		/// <returns></returns>
		public async Task SetProposalLost(int proposalId, List<string> messages)
		{
			FullProposal fullProposal = await GetFullProposalAsync(proposalId);
			ContractsDto contracts = this.contractsLoader.GetContractForProposal(proposalId);

			if (this.ValidForLostProposalStatusSave(fullProposal, contracts, messages))
			{
				using (StopwatchTimer sw = new StopwatchTimer("ContractsControllerLogic.SetProposalLost", this.log))
				{
					using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, Convert.ToInt32(WebConfigurationManager.AppSettings["TransactionTimeout"])) }))
					{
						this.SetProposalStatusToLost(proposalId);
						scope.Complete();
					}
				}

				// return control and send email async
				await this.SendContractsStatusChangeEmails(proposalId);
			}
		}

		/// <summary>
		/// Converts the page viewmodel into the Dto
		/// </summary>
		/// <param name="model">Contracts Model View</param>
		/// <param name="isComplete">Are we completing the proposal</param>
		/// <returns>Contracts DTO</returns>
		[SuppressMessage("StyleCop.CSharp.ReadabilityRules", "*")]
		public ContractsDto ConvertContractsModelToDto(ContractsModelView model)
		{
			if (model == null)
			{
				return new ContractsDto();
			}

			ContractsDto dto = new ContractsDto();
			dto.Updateable = UpdateType.Upsert;

			dto.Id = model.Id;
			dto.ProposalId = model.ProposalId;
			dto.PreviouslySubmittedROM = model.PreviouslySubmittedROM;
			dto.CustomerDueDate = model.CustomerDueDate == null ? (DateTime?)null : DateTime.Parse(model.CustomerDueDate);
			dto.CustomerSubmittalDate = model.CustomerSubmittalDate == null ? (DateTime?)null : DateTime.Parse(model.CustomerSubmittalDate);
			dto.ContractsCorrespondenceLogNumber = model.ContractsCorrespondenceLogNumber;
			dto.FinalNegotiatedValue = model.FinalNegotiatedValueLong;
			dto.NegotiationsSubmitted = model.NegotiationsSubmitted == null ? (DateTime?)null : DateTime.Parse(model.NegotiationsSubmitted);
			dto.UpdateDateLong = model.LastUpdatedDateLong;
			dto.EppDelegationAuthority = model.EppDelegationAuthority == null ? (int?)null : (int)model.EppDelegationAuthority;
			dto.ProgramEppDate = model.ProgramEppDate;
			dto.LobEppDate = model.LobEppDate;
			dto.PreSpaceEppDate = model.PreSpaceEppDate;
			dto.SpaceEppDate = model.SpaceEppDate;
			dto.PreCorporateEppDate = model.PreCorporateEppDate;
			dto.CorporateEppDate = model.CorporateEppDate;
			dto.EppRosDelegationNotes = model.EppRosDelegationNotes;
			dto.LmWon = model.LmWon;
			dto.ModCompletedDate = model.ModCompletedDate;
			dto.CageCode = model.CageCode;
			dto.NegotiatedInsurance = string.IsNullOrEmpty(model.NegotiatedInsurance) ? null : (long?)long.Parse(model.NegotiatedInsurance.Replace(",", string.Empty));
			dto.ProposedInsurance = string.IsNullOrEmpty(model.ProposedInsurance) ? null : (long?)long.Parse(model.ProposedInsurance.Replace(",", string.Empty));
			dto.IsInsuranceDirect = model.IsInsuranceDirect;
			// Insurance Type value is only valid if IsInsuranceDirect is Yes
			if (dto.IsInsuranceDirect == TripleBooleanState.Yes)
			{
				dto.InsuranceType = model.InsuranceType;
			}

			return dto;
		}

		/// <summary>
		/// Converts the dto to a page view model
		/// </summary>
		/// <param name="dto"></param>
		/// <returns>Contracts view model</returns>
		[SuppressMessage("StyleCop.CSharp.ReadabilityRules", "*")]
		private ContractsModelView ConvertContractsDtoToModel(ContractsDto dto)
		{
			if (dto == null)
			{
				return new ContractsModelView();
			}

			ContractsModelView model = new ContractsModelView();

			model.Id = dto.Id;
			model.ProposalId = dto.ProposalId;
			model.PreviouslySubmittedROM = dto.PreviouslySubmittedROM;
			model.CustomerDueDt = dto.CustomerDueDate;
			model.CustomerSubmittalDt = dto.CustomerSubmittalDate;
			model.ContractsCorrespondenceLogNumber = dto.ContractsCorrespondenceLogNumber;
			model.FinalNegotiatedValueLong = dto.FinalNegotiatedValue == null ? dto.FinalNegotiatedValue : long.Parse(dto.FinalNegotiatedValue.ToString());
			model.NegotiationsSubmittedDt = dto.NegotiationsSubmitted;
			model.LastUpdatedDateLong = dto.UpdateDateLong;
			model.EppDelegationAuthority = dto.EppDelegationAuthority == null ? (EppDelegationAuthority?)null : (EppDelegationAuthority)dto.EppDelegationAuthority;
			model.ProgramEppDate = dto.ProgramEppDate;
			model.LobEppDate = dto.LobEppDate;
			model.PreSpaceEppDate = dto.PreSpaceEppDate;
			model.SpaceEppDate = dto.SpaceEppDate;
			model.PreCorporateEppDate = dto.PreCorporateEppDate;
			model.CorporateEppDate = dto.CorporateEppDate;
			model.EppRosDelegationNotes = dto.EppRosDelegationNotes;
			model.LmWon = dto.LmWon;
			model.ModCompletedDate = dto.ModCompletedDate;
			model.CageCode = dto.CageCode;
			model.IsInsuranceDirect = dto.IsInsuranceDirect;
			model.InsuranceType = dto.InsuranceType;
			model.NegotiatedInsurance = dto.NegotiatedInsurance.ToString();
			model.ProposedInsurance = dto.ProposedInsurance.ToString();

			return model;
		}

		/// <summary>
		/// Determines whether the proposal is in a valid state to have "Set Lost" status set.
		/// </summary>
		/// <param name="contractInfo">Contract data object</param>
		/// <param name="fullProposal">The full proposal object</param>
		/// <returns>true if valid for Lost status</returns>
		private bool IsValidForLostStatus(ContractsDto contractInfo, FullProposal fullProposal)
		{
			bool valid = false;

			if ((fullProposal.ProposalStatus == ProposalStatus.PendingCertification || fullProposal.ProposalStatus == ProposalStatus.PendingAward)
				&& (!contractInfo.LmWon.HasValue || !contractInfo.LmWon.Value) // null or false
				&& contractInfo.CustomerSubmittalDate.HasValue && contractInfo.CustomerDueDate.HasValue)
			{
				valid = true;
			}

			return valid;
		}

		/// <summary>
		/// Determines whether the proposal is in a valid state to have "No Bid" status set. Can only be set if in In Progress,
		/// Pending Certification, or Pending Contractual Award statuses.
		/// </summary>
		/// <param name="fullProposal">The full proposal object</param>
		/// <returns>True if in a valid status</returns>
		private bool IsValidForNoBidStatus(FullProposal fullProposal)
		{
			return fullProposal.ProposalStatus == ProposalStatus.InProgress || fullProposal.ProposalStatus == ProposalStatus.PendingCertification || fullProposal.ProposalStatus == ProposalStatus.PendingAward;
		}

		/// <summary>
		/// Determines whether the proposal is in a valid state to have the "Complete" status set. 
		/// </summary>
		/// <param name="contractInfo">Contract data object</param>
		/// <param name="fullProposal">The full proposal object</param>
		/// <returns>true if button should be enabled.</returns>
		private bool IsValidForCompleteStatus(ContractsDto dto, FullProposal fullProposal)
		{
			return this.ContractDataValidForCompleteProposalSave(dto, fullProposal, null) &&
				(
					(dto.LmWon.HasValue && dto.LmWon.Value && fullProposal.ProposalStatus == ProposalStatus.PendingAward)
					|| (fullProposal.IsRomNte && (fullProposal.ProposalStatus == ProposalStatus.PendingAward))
					);
		}

		/// <summary>
		/// Sends templated email regarding the Lost status to the estimators
		/// </summary>
		/// <param name="proposalId">Proposal Id</param>
		public async Task SendContractsStatusChangeEmails(int proposalId)
		{
			FullProposal fullProposal = await GetFullProposalAsync(proposalId);
			EmailInformationDto emailInfo = new EmailInformationDto();
			emailInfo.ProposalId = proposalId;

			EmailContent emailContent = new EmailContent();
			if (fullProposal.ProposalStatus == ProposalStatus.Lost)
			{
				emailContent.Body = Emails.STATUS_LOST_SET.Body;
				emailContent.Subject = Emails.STATUS_LOST_SET.Subject;
			}
			else if (fullProposal.ProposalStatus == ProposalStatus.NoBid)
			{
				emailContent.Body = Emails.STATUS_NO_BID_SET.Body;
				emailContent.Subject = Emails.STATUS_NO_BID_SET.Subject;
			}
			else if (fullProposal.ProposalStatus == ProposalStatus.Completed)
			{
				emailContent.Body = Emails.STATUS_COMPLETED_SET.Body;
				emailContent.Subject = Emails.STATUS_COMPLETED_SET.Subject;
			}
			else
			{
				log.Error($"SendContractsStatusChangeEmails was called on a proposal in {fullProposal.ProposalStatus.GetDescription<ProposalStatus>()} status.");
				throw new InvalidOperationException("Attempting to send an email for an invalid status.");
			}

			// Set data to be used in the replacements
			string[] subjectReplaceTokens = new string[] { fullProposal.ProposalTitle };
			string[] bodyReplaceTokens = new string[] { fullProposal.ProposalTitle, emailInfo.ProposalContractsUrl.ToString() };

			ProposalPermissionDto leadPricer = fullProposal.Permissions.First(x => x.Role == PtmRole.Pricer);
			ProposalPermissionDto backUpPricer = fullProposal.Permissions.FirstOrDefault(x => x.Role == PtmRole.BackupPricer);

			// Send the email notifications
			UserDTO leadEstimator = UserMapper.GetById(leadPricer.UserId);
			this.emailer.SendEmail(emailContent, leadEstimator.EmailAddress, new Collection<UserDTO>(), subjectReplaceTokens, bodyReplaceTokens, null, " for: " + fullProposal.TrackingNumber);

			if (backUpPricer != null)
			{
				UserDTO backupEstimator = UserMapper.GetById(backUpPricer.UserId);
				this.emailer.SendEmail(emailContent, backupEstimator.EmailAddress, new Collection<UserDTO>(), subjectReplaceTokens, bodyReplaceTokens, null, " for: " + fullProposal.TrackingNumber);
			}
		}

		/// <summary>
		/// Loads the Full Proposal data using thread pool versus the main
		/// </summary>
		/// <param name="proposalId">Proposal Id</param>
		public async Task<FullProposal> GetFullProposalAsync(int proposalId)
		{
			Task<FullProposal> fullProposal = Task<FullProposal>.Run(() => this.GetFullProposalDto(proposalId));
			fullProposal.Wait();

			return fullProposal.Result;
		}

		/// <summary>
		/// Set proposal to Lost status
		/// </summary>
		/// <param name="proposalId">ID of Proposal</param>
		private void SetProposalStatusToLost(int proposalId)
		{
			using (StopwatchTimer sw = new IES.Common.StopwatchTimer("ContractsControllerLogic.SetProposalStatusToLost", this.log))
			{
				FullProposal fullProposal = GetFullProposalAsync(proposalId).Result;
				fullProposal.ProposalStatus = ProposalStatus.Lost;
				fullProposal.ProposalCompletedDate = DateTime.Now;
				fullProposal.Updateable = IES.Common.UpdateType.Upsert;
				this.ProposalMediator.SaveProposal(fullProposal);
			}
		}

		/// <summary>
		/// Set proposal to No Bid status
		/// </summary>
		/// <param name="proposalId">ID of Proposal</param>
		public void SetProposalToNoBid(int proposalId)
		{
			using (StopwatchTimer sw = new IES.Common.StopwatchTimer("ContractsControllerLogic.SetProposalToNoBid", this.log))
			{
				// set status no bid
				FullProposal fullProposal = GetFullProposalAsync(proposalId).Result;
				fullProposal.ProposalStatus = ProposalStatus.NoBid;
				fullProposal.NoBidDate = DateTime.Now;
				fullProposal.Updateable = IES.Common.UpdateType.Upsert;
				this.ProposalMediator.SaveProposal(fullProposal);
			}
		}

		/// <summary>
		/// Revert the Proposal from No Bid back to In Progress
		/// </summary>
		/// <param name="proposalId">ID of Proposal to revert</param>
		public void RevertProposalFromNoBid(int proposalId)
		{
			using (StopwatchTimer sw = new IES.Common.StopwatchTimer("ContractsControllerLogic.RevertProposalFromNoBid", this.log))
			{
				// set status in progress
				FullProposal proposal = this.GetFullProposalDto(proposalId);
				proposal.ProposalStatus = ProposalStatus.InProgress;
				proposal.NoBidDate = null;
				proposal.Updateable = IES.Common.UpdateType.Upsert;
				this.ProposalMediator.SaveProposal(proposal);
			}

			// reset workflow
			this.approvalsLogic.ResetWorkflow(proposalId);
		}

		/// <summary>
		/// Checks permissions and status to ensure that the proposal is valid for setting "Lost" status.
		/// a) Must have the primary or backup contracts role or be an administrator
		/// b) The status must currently be Pending Certification or Pending Contractual Award.
		/// </summary>
		/// <param name="fullProposal">Proposal</param>
		/// <param name="contracts">Contracts dto</param>
		/// <param name="messages">Response object to be returned</param>
		/// <returns>true if valid for setting the status</returns>
		public bool ValidForLostProposalStatusSave(FullProposal fullProposal, ContractsDto contracts, List<string> messages)
		{
			_ = fullProposal ?? throw new ArgumentNullException(nameof(fullProposal));
			_ = contracts ?? throw new ArgumentNullException(nameof(contracts));

			bool isValid = true;

			bool isLeadOrBackupContractsUser = this.IsContractsUser(fullProposal.CurrentUser.Id, fullProposal.Permissions);
			bool isAdmin = this.SecurityAccess.CurrentUserHasRole(PtmRole.Admin, null);

			if (!isLeadOrBackupContractsUser && !isAdmin)
			{
				messages?.Add(Constants.INSUFFICIENT_PERMISSIONS_FOR_LOST);
				isValid = false;
			}

			if (fullProposal.ProposalStatus != ProposalStatus.PendingCertification && fullProposal.ProposalStatus != ProposalStatus.PendingAward)
			{
				messages?.Add(Constants.INVALID_STATUS_FOR_LOST);
				isValid = false;
			}

			if (!contracts.CustomerDueDate.HasValue)
			{
				messages?.Add(Constants.DUE_DATE_REQUIRED_FOR_LOST);
				isValid = false;
			}

			if (!contracts.CustomerSubmittalDate.HasValue)
			{
				messages?.Add(Constants.SUBMITTAL_DATE_REQUIRED_FOR_LOST);
				isValid = false;
			}

			return isValid;
		}

		/// <summary>
		/// Ensures the proposal is in an acceptable status and the user has permission to set "No Bid"
		/// </summary>
		/// <param name="fullProposal">Full proposal object</param>
		/// <param name="messages">List to which error messages will be added. If null, ignored</param>
		/// <returns>true if the prosal is valid for the No Bid status</returns>
		/// <exception cref="ArgumentNullException">if Full Proposal is null</exception>
		public bool ValidForNoBidProposalStatusSave(FullProposal fullProposal, List<string> messages)
		{
			_ = fullProposal ?? throw new ArgumentNullException(nameof(fullProposal));

			bool isValid = true;

			bool isLeadOrBackupContractsUser = this.IsContractsUser(fullProposal.CurrentUser.Id, fullProposal.Permissions);
			bool isAdmin = this.SecurityAccess.CurrentUserHasRole(PtmRole.Admin, null);

			if (!isLeadOrBackupContractsUser && !isAdmin)
			{
				messages?.Add("Insufficient permissions to set proposal as No Bid.");
				isValid = false;
			}

			ProposalStatus currentStatus = fullProposal.ProposalStatus;
			if (currentStatus != ProposalStatus.InProgress && currentStatus != ProposalStatus.PendingAward && currentStatus != ProposalStatus.PendingCertification)
			{
				messages?.Add($"{currentStatus} is not a valid status for setting No Bid.");
				isValid = false;
			}

			return isValid;
		}

		/// <summary>
		/// Validates whether the proposal is valid for Completed Status
		/// </summary>
		/// <param name="dto">Contracts data</param>
		/// <param name="fullProposal">The full proposal object</param>
		/// <param name="messages">Validation error messages (out)</param>
		/// <returns>True if valid</returns>
		/// <exception cref="ArgumentNullException">Data missing</exception>
		public bool ContractDataValidForCompleteProposalSave(ContractsDto dto, FullProposal fullProposal, List<string> messages)
		{
			_ = dto ?? throw new ArgumentNullException(nameof(dto));
			_ = fullProposal ?? throw new ArgumentNullException(nameof(fullProposal));
			messages = messages ?? new List<string>();

			bool isValid = true;
			
			// Customer Due Date required for validation
			if (dto.CustomerDueDate is null)
			{
				isValid = false;
				messages.Add(Constants.INVALID_CUSTOMER_DUE_DATE);
			}

			// Cage Code required for validation
			if (dto.CageCode == null)
			{
				isValid = false;
				messages.Add(Constants.INVALID_CAGE_CODE);
			}

			EppDelegationDatesHelper edc = new EppDelegationDatesHelper();

			switch ((EppDelegationAuthority?)dto?.EppDelegationAuthority)
			{
				case EppDelegationAuthority.Program:
				case EppDelegationAuthority.LoB:
				case EppDelegationAuthority.Space:
				case EppDelegationAuthority.Corporate:
					if (!edc.AreRequiredDatesPopulated(dto, messages) || !edc.AreEnteredDatesSequential(dto, messages))
					{
						isValid = false;
					}
					break;

				default:
					string msg = $"EPP Delegation Authority is unset or not valid.";
					log.Info(msg + $" ({dto?.EppDelegationAuthority})");
					messages.Add(msg);
					isValid = false;
					break;
			}

			if (!fullProposal.IsRomNte && dto.LmWon.HasValue && dto.LmWon.Value)
			{
				// Final Negotiated Value is required
				if (dto.FinalNegotiatedValue == null)
				{
					isValid = false;
					messages.Add(Constants.INVALID_FINAL_NEGOTIATED_VALUE);
				}

				// Date Confirmation Of Negotiations Submitted is required
				if (dto.NegotiationsSubmitted == null || dto.NegotiationsSubmitted == DateTime.MinValue)
				{
					isValid = false;
					messages.Add(Constants.INVALID_NEGOTIATIONS_SUBMITTED_DATE);
				}

				// Mod Completion Date is required
				if (dto.ModCompletedDate == null || dto.ModCompletedDate == DateTime.MinValue)
				{
					isValid = false;
					messages.Add(Constants.INVALID_MOD_COMPLETION_DATE);
				}
			}

			// LM Win / Loss is required
			if (!fullProposal.IsRomNte && dto.LmWon == null)
			{
				isValid = false;
				messages.Add(Constants.INVALID_LM_WIN_LOSS);
			}

			if (!fullProposal.IsRomNte)
			{
				if (dto.IsInsuranceDirect is null)
				{
					isValid = false;
					messages.Add(Constants.INVALID_INSURANCE_DIRECT);
				}
				else if (dto.IsInsuranceDirect == TripleBooleanState.Yes && dto.InsuranceType is null)
				{
					isValid = false;
					messages.Add(Constants.INVALID_INSURANCE_TYPE);
				}

				if (dto.ProposedInsurance is null)
				{
					isValid = false;
					messages.Add(Constants.INVALID_PROPOSED_INSURANCE);
				}

				if (dto.NegotiatedInsurance is null)
				{
					isValid = false;
					messages.Add(Constants.INVALID_NEGOTIATED_INSURANCE);
				}
			}

			messages.AddRange(ValidateContractModelView(ConvertContractsDtoToModel(dto), fullProposal));

			return isValid;
		}

		/// <summary>
		/// Is the userId in the proposal's permissions as a Contracts administrator (lead/back-up)
		/// </summary>
		/// <param name="userId">User Id</param>
		/// <param name="propPermissions">Permissions for the proposal</param>
		/// <returns>True if the user is a Contracts admin</returns>
		private bool IsContractsUser(int userId, ICollection<ProposalPermissionDto> propPermissions)
		{
			return propPermissions.Any(x => (x.Role == PtmRole.ContractsPOC || x.Role == PtmRole.BackupContractsPOC) && x.UserId == userId);
		}

		#endregion Contract Validate / Save
	}
}

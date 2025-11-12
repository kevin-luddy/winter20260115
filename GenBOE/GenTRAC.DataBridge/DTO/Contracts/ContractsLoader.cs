// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web.Mvc;
	using GenTRAC.Models;
	using IES.Common;

	/// <summary>
	/// Contracts Loader
	/// </summary>
	public class ContractsLoader : DataLoader<ContractsDto>, IContractsLoader
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public ContractsLoader()
		{
			this.Log = new Logger(typeof(ContractsLoader));
		}

		/// <summary>
		/// Saves a Proposal Contract
		/// </summary>
		/// <param name="dtoToUpsert">Proposal Contract to save.</param>
		/// <returns>Id of the upserted item.</returns>
		protected override int? Upsert(ContractsDto dtoToUpsert)
		{
			int? toReturn = null;

			using (StopwatchTimer sw = new StopwatchTimer("ContractsLoader.Upsert", Log))
			{
				if (dtoToUpsert != null)
				{
					// save
					using (genTRACEntities dbModel = new genTRACEntities())
					{
						toReturn = dbModel.upsertProposalContractsData(
							dtoToUpsert.Id,
							dtoToUpsert.UpdateDate,
							dtoToUpsert.ProposalId,
							dtoToUpsert.PreviouslySubmittedROM,
							dtoToUpsert.CustomerSubmittalDate,
							dtoToUpsert.ContractsCorrespondenceLogNumber,
							dtoToUpsert.FinalNegotiatedValue,
							dtoToUpsert.NegotiationsSubmitted,
							dtoToUpsert.EppDelegationAuthority,
							dtoToUpsert.ScheduledActualBidEppDate,
							dtoToUpsert.ScheduledActualProgramEppDate,
							dtoToUpsert.ScheduledActualMissionSegmentEppDate,
							dtoToUpsert.ScheduledActualLobEppDate,
							dtoToUpsert.ScheduledActualPreSpaceEppDate,
							dtoToUpsert.ScheduledActualSpaceEppDate,
							dtoToUpsert.ScheduledActualPreCorporateEppDate,
							dtoToUpsert.ScheduledActualCorporateEppDate,
							dtoToUpsert.PlannedBidEppDate,
							dtoToUpsert.PlannedProgramEppDate,
							dtoToUpsert.PlannedMissionSegmentEppDate,
							dtoToUpsert.PlannedLobEppDate,
							dtoToUpsert.PlannedPreSpaceEppDate,
							dtoToUpsert.PlannedSpaceEppDate,
							dtoToUpsert.PlannedPreCorporateEppDate,
							dtoToUpsert.PlannedCorporateEppDate,
							dtoToUpsert.EppRosDelegationNotes,
							dtoToUpsert.LmWon,
							dtoToUpsert.ModCompletedDate,
							dtoToUpsert.CageCode,
							dtoToUpsert.CustomerDueDate,
							dtoToUpsert.IsInsuranceDirect.HasValue ? (int?)dtoToUpsert.IsInsuranceDirect : null,
							dtoToUpsert.InsuranceType.HasValue ? (int?)dtoToUpsert.InsuranceType : null,
							dtoToUpsert.ProposedInsurance,
							dtoToUpsert.NegotiatedInsurance
							).FirstOrDefault();
					}
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Deletes a Proposal Contract
		/// Note: This is not implemented, is deleted when Proposal is deleted.
		/// </summary>
		/// <param name="dtoToDelete">Proposal Contract to delete.</param>
		/// <returns>Id of the deleted item.</returns>
		/// <exception cref="System.NotImplementedException">This method should never be called.</exception>
		protected override int? Delete(ContractsDto dtoToDelete)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets all Contracts for the proposal.
		/// </summary>
		/// <param name="proposalId">The proposal ID.</param>
		/// <returns>
		/// A Contract for the given proposal.
		/// </returns>
		public ContractsDto GetContractForProposal(int proposalId)
		{
			ContractsDto toReturn;

			using (StopwatchTimer sw = new StopwatchTimer("ContractsLoader.GetContractForProposal", Log))
			{
				using (genTRACEntities dbModel = new genTRACEntities())
				{
					toReturn = dbModel.ProposalContractsDatas
						.Select(x => new ContractsDto()
						{
							Id = x.ProposalContractsDataId,
							UpdateDate = x.UpdateDT,
							ProposalId = x.ProposalID,
							PreviouslySubmittedROM = x.PreviouslySubmittedROM,
							CustomerDueDate = x.CustomerDueDate,
							CustomerSubmittalDate = x.CustomerSubmittalDate,
							ContractsCorrespondenceLogNumber = x.ContractsCorrespondLogNumber,
							FinalNegotiatedValue = x.FinalNegotiatedValue,
							NegotiationsSubmitted = x.FinalNegotiatedDate,
							EppDelegationAuthority = x.EppDelegationAuthority,
							ScheduledActualBidEppDate = x.ScheduledActualBidEppDate,
							ScheduledActualProgramEppDate = x.ScheduledActualProgramEppDate,
							ScheduledActualMissionSegmentEppDate = x.ScheduledActualMissionSegmentEppDate,
							ScheduledActualLobEppDate = x.ScheduledActualLobEppDate,
							ScheduledActualPreSpaceEppDate = x.ScheduledActualPreSpaceEppDate,
							ScheduledActualSpaceEppDate = x.ScheduledActualSpaceEppDate,
							ScheduledActualPreCorporateEppDate = x.ScheduledActualPreCorporateEppDate,
							ScheduledActualCorporateEppDate = x.ScheduledActualCorporateEppDate,
							PlannedBidEppDate = x.PlannedBidEppDate,
							PlannedProgramEppDate = x.PlannedProgramEppDate,
							PlannedMissionSegmentEppDate = x.PlannedMissionSegmentEppDate,
							PlannedLobEppDate = x.PlannedLobEppDate,
							PlannedPreSpaceEppDate = x.PlannedPreSpaceEppDate,
							PlannedSpaceEppDate = x.PlannedSpaceEppDate,
							PlannedPreCorporateEppDate = x.PlannedPreCorporateEppDate,
							PlannedCorporateEppDate = x.PlannedCorporateEppDate,
							EppRosDelegationNotes = x.EppRosDelegationNotes,
							LmWon = x.LmWon,
							ModCompletedDate = x.ModCompletedDate,
							CageCode = x.CageCode,
							IsInsuranceDirect = x.IsInsuranceDirect.HasValue ? (TripleBooleanState?)x.IsInsuranceDirect : null,
							InsuranceType = x.InsuranceType.HasValue ? (InsuranceType?)x.InsuranceType : null,
							NegotiatedInsurance = x.NegotiatedInsurance,
							ProposedInsurance = x.ProposedInsurance
						}).FirstOrDefault(x => x.ProposalId == proposalId);
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Gets all objects based on the IDs that were passed in
		/// </summary>
		/// <param name="ids">IDs</param>
		/// <returns>
		/// Corresponding Data
		/// </returns>
		public override ICollection<ContractsDto> GetByIds(ICollection<int> ids)
		{
			ICollection<ContractsDto> toReturn;

			using (StopwatchTimer sw = new StopwatchTimer(this.Log))
			{
				using (genTRACEntities dbModel = new genTRACEntities())
				{
					toReturn = dbModel.ProposalContractsDatas
						.Where(x => ids.Contains(x.ProposalContractsDataId))
						.Select(x => new ContractsDto
						{
							Id = x.ProposalContractsDataId,
							UpdateDate = x.UpdateDT,
							ProposalId = x.ProposalID,
							PreviouslySubmittedROM = x.PreviouslySubmittedROM,
							CustomerDueDate = x.CustomerDueDate,
							CustomerSubmittalDate = x.CustomerSubmittalDate,
							ContractsCorrespondenceLogNumber = x.ContractsCorrespondLogNumber,
							FinalNegotiatedValue = x.FinalNegotiatedValue,
							NegotiationsSubmitted = x.FinalNegotiatedDate,
							EppDelegationAuthority = x.EppDelegationAuthority,
							ScheduledActualBidEppDate = x.ScheduledActualBidEppDate,
							ScheduledActualProgramEppDate = x.ScheduledActualProgramEppDate,
							ScheduledActualMissionSegmentEppDate = x.ScheduledActualMissionSegmentEppDate,
							ScheduledActualLobEppDate = x.ScheduledActualLobEppDate,
							ScheduledActualPreSpaceEppDate = x.ScheduledActualPreSpaceEppDate,
							ScheduledActualSpaceEppDate = x.ScheduledActualSpaceEppDate,
							ScheduledActualPreCorporateEppDate = x.ScheduledActualPreCorporateEppDate,
							ScheduledActualCorporateEppDate = x.ScheduledActualCorporateEppDate,
							PlannedBidEppDate = x.PlannedBidEppDate,
							PlannedProgramEppDate = x.PlannedProgramEppDate,
							PlannedMissionSegmentEppDate = x.PlannedMissionSegmentEppDate,
							PlannedLobEppDate = x.PlannedLobEppDate,
							PlannedPreSpaceEppDate = x.PlannedPreSpaceEppDate,
							PlannedSpaceEppDate = x.PlannedSpaceEppDate,
							PlannedPreCorporateEppDate = x.PlannedPreCorporateEppDate,
							PlannedCorporateEppDate = x.PlannedCorporateEppDate,
							EppRosDelegationNotes = x.EppRosDelegationNotes,
							LmWon = x.LmWon,
							ModCompletedDate = x.ModCompletedDate,
							CageCode = x.CageCode,
							IsInsuranceDirect = x.IsInsuranceDirect.HasValue ? (TripleBooleanState?)x.IsInsuranceDirect : null,
							InsuranceType = x.InsuranceType.HasValue ? (InsuranceType?)x.InsuranceType : null,
							NegotiatedInsurance = x.NegotiatedInsurance,
							ProposedInsurance = x.ProposedInsurance
						}).ToList();
				}
			}

			return toReturn;
		}

		/// <summary>
		/// Gets a list of select items representing the EppDelegationAuthority enumeration values
		/// </summary>
		/// <param name="selectedValue">Enum option that should be selected by default</param>
		/// <returns>List of SelectListItems</returns>
		public ICollection<SelectListItem> GetEppSelectValues(EppDelegationAuthority? selectedValue)
		{
			ICollection<SelectListItem> result = new List<SelectListItem>();

			EppDelegationAuthority[] enums = (EppDelegationAuthority[])Enum.GetValues(typeof(EppDelegationAuthority));

			// add blank option
			result.Add(new SelectListItem { Value = null, Text = null, Selected = selectedValue == null });

			foreach (EppDelegationAuthority item in enums)
			{
				result.Add(new SelectListItem
					{
						Value = item.ToString(),
						Text = item.GetDescription<EppDelegationAuthority>(),
						Selected = item == selectedValue
					});
			}

			return result;
		}
	}
}

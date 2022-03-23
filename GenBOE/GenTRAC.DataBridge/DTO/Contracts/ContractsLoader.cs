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
    class ContractsLoader : DataLoader<ContractsDto>, IContractsLoader
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
                            dtoToUpsert.ProgramEppDate,
                            dtoToUpsert.LobEppDate,
                            dtoToUpsert.PreSpaceEppDate,
                            dtoToUpsert.SpaceEppDate,
                            dtoToUpsert.PreCorporateEppDate,
                            dtoToUpsert.CorporateEppDate,
                            dtoToUpsert.EppRosDelegationNotes,
                            dtoToUpsert.LmWon,
                            dtoToUpsert.ModCompletedDate
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
            ContractsDto toReturn = new ContractsDto();

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
                            CustomerSubmittalDate = x.CustomerSubmittalDate,
                            ContractsCorrespondenceLogNumber = x.ContractsCorrespondLogNumber,
                            FinalNegotiatedValue = x.FinalNegotiatedValue,
                            NegotiationsSubmitted = x.FinalNegotiatedDate,
                            EppDelegationAuthority = x.EppDelegationAuthority,
                            ProgramEppDate = x.ProgramEppDate,
                            LobEppDate = x.LobEppDate,
                            PreSpaceEppDate = x.PreSpaceEppDate,
                            SpaceEppDate = x.SpaceEppDate,
                            PreCorporateEppDate = x.PreCorporateEppDate,
                            CorporateEppDate = x.CorporateEppDate,
                            EppRosDelegationNotes = x.EppRosDelegationNotes,
                            LmWon = x.LmWon,
                            ModCompletedDate = x.ModCompletedDate
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
                        .Select(x => new ContractsDto
                        {
                            Id = x.ProposalContractsDataId,
                            UpdateDate = x.UpdateDT,
                            ProposalId = x.ProposalID,
                            PreviouslySubmittedROM = x.PreviouslySubmittedROM,
                            CustomerSubmittalDate = x.CustomerSubmittalDate,
                            ContractsCorrespondenceLogNumber = x.ContractsCorrespondLogNumber,
                            FinalNegotiatedValue = x.FinalNegotiatedValue,
                            NegotiationsSubmitted = x.FinalNegotiatedDate,
                            EppDelegationAuthority = x.EppDelegationAuthority,
                            ProgramEppDate = x.ProgramEppDate,
                            LobEppDate = x.LobEppDate,
                            PreSpaceEppDate = x.PreSpaceEppDate,
                            SpaceEppDate = x.SpaceEppDate,
                            PreCorporateEppDate = x.PreCorporateEppDate,
                            CorporateEppDate = x.CorporateEppDate,
                            EppRosDelegationNotes = x.EppRosDelegationNotes,
                            LmWon = x.LmWon,
                            ModCompletedDate = x.ModCompletedDate
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
        public ICollection<SelectListItem> GetEppSelectValues(EppDelegationAuthority selectedValue)
        {
            ICollection<SelectListItem> result = new List<SelectListItem>();

            EppDelegationAuthority[] enums = (EppDelegationAuthority[])Enum.GetValues(typeof(EppDelegationAuthority));

            // add blank option
            result.Add(new SelectListItem { Value = "0", Text = "(Select)", Selected = (int)selectedValue == 0 });

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

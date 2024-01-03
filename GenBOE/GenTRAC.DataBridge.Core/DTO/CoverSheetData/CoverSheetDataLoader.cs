// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System.Collections.Generic;
    using System.Linq;
    using GenTRAC.Models;
    using IES.Core;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// Cover Sheet Data Dto Loader
	/// </summary>
	public class CoverSheetDataLoader : ICoverSheetDataLoader
    {
        /// <summary>
        /// Logger
        /// </summary>
        public ILogger Log { get; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CoverSheetDataLoader(ILogger<CoverSheetDataLoader> logger)
		{
			this.Log = logger;
		}

        /// <summary>
        /// Gets Cover Sheet data by a given proposal id.
        /// </summary>
        /// <param name="id">Proposal Id</param>
        /// <returns>Cover Sheet DTO</returns>
        public CoverSheetDataDto GetCoverSheetDataById(int id)
        {
            CoverSheetDataDto coverSheetData = new CoverSheetDataDto();

            using (StopwatchTimer sw = new StopwatchTimer("CoverSheetDataLoader.GetCoverSheetDataById", Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    coverSheetData = dbModel.Proposals.Where(x => x.ProposalID == id).Select(prop => new CoverSheetDataDto
                    {
                        IsCCPDRequired = prop.CCPDRequired,
                        ContractActionType = (ContractActionType?)prop.ContractActionType,
                        OtherContractActionType = prop.ContractActionTypeOtherText,
                        ContractTypeGroup = prop.ContractTypeGroupID ?? 0,
                        CoverSheetApproverSignedDate = prop.CoverSheetApproverSignedDT,
                        CostThroughCom = prop.ProposalChecklists.FirstOrDefault().CostThroughCom,
                        ProfitFee = prop.ProposalChecklists.FirstOrDefault().Profit,
                        LMSpaceTotalPrice = prop.ProposalChecklists.FirstOrDefault().ISGSTotalPrice,

                        CustomerSubmittalDate = prop.ProposalContractsDatas1.FirstOrDefault().CustomerSubmittalDate,
                        CageCode = prop.ProposalContractsDatas1.FirstOrDefault().CageCode,
                        OfferorAddress = new List<string>() {
                                prop.ProposalContractsDatas1.FirstOrDefault().CageCode1.Address1,
                                prop.ProposalContractsDatas1.FirstOrDefault().CageCode1.Address2,
                                prop.ProposalContractsDatas1.FirstOrDefault().CageCode1.City,
                                prop.ProposalContractsDatas1.FirstOrDefault().CageCode1.State,
                                prop.ProposalContractsDatas1.FirstOrDefault().CageCode1.Zip
                            },
                        CoverSheetApproverNtid = prop.ProposalUserRoles.FirstOrDefault(x => x.RoleID == (int)PtmRole.CoverSheetApprover).genTRACUser.NTID,
                        ContractsLead = prop.ProposalUserRoles.FirstOrDefault(x => x.RoleID == (int)PtmRole.ContractsPOC).genTRACUser.NTID
                    }).FirstOrDefault();

                    if(string.IsNullOrEmpty(coverSheetData.CageCode))
                    {
                        coverSheetData.OfferorAddress = null;
                    }
                }
            }

            return coverSheetData;
        }
    }
}
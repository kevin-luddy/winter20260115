// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System.Collections.Generic;
    using GenTRAC.DataBridge.DTO.Contracts;
    using GenTRAC.Objects;
    using GenTRAC.Objects.FullObject;
    using IES.Common;

    /// <summary>
    /// Cover Sheet Data Dto Loader
    /// </summary>
    public class CoverSheetDataLoader : ICoverSheetDataLoader
    {
        /// <summary>
        /// Logger
        /// </summary>
        public Logger Log { get; }

        /// <summary>
        /// Gets or sets the Contracts Loader.
        /// </summary>
        private IContractsLoader ContractsLoader { get; set; }

        /// <summary>
        /// Gets or sets the Proposal Loader.
        /// </summary>
        private IProposalLoader ProposalLoader { get; set; }

        /// <summary>
        /// Gets or sets the Proposal Checklist Loader.
        /// </summary>
        private IProposalChecklistLoader ProposalChecklistLoader { get; set; }

        /// <summary>
        /// Gets or sets the Cage Codes Loader.
        /// </summary>
        private ICageCodesLoader CageCodesLoader { get; set; }

        /// <summary>
        /// Object Factory
        /// </summary>
        protected IFullObjectFactory ObjectFactory { get; set; }

        /// <summary>
        /// The user mapper
        /// </summary>
        protected IUserMapper UserMapper { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CoverSheetDataLoader()
        {
            this.Log = new Logger(typeof(CoverSheetDataLoader));
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="proposalLoader">Proposal Loader</param>
        /// <param name="proposalChecklistLoader">Checklist Loader</param>
        /// <param name="contractsLoader">Contracts Loader</param>
        /// <param name="cageCodesLoader">Cage Codes Loader</param>
        /// <param name="objectFactory">Object Factory</param>
        /// <param name="userMapper">User Mapper</param>
        public CoverSheetDataLoader(IProposalLoader proposalLoader, IProposalChecklistLoader proposalChecklistLoader, IContractsLoader contractsLoader,
            ICageCodesLoader cageCodesLoader, IFullObjectFactory objectFactory, IUserMapper userMapper) : this()
        {
            this.ProposalLoader = proposalLoader;
            this.ProposalChecklistLoader = proposalChecklistLoader;
            this.ContractsLoader = contractsLoader;
            this.CageCodesLoader = cageCodesLoader;
            this.ObjectFactory = objectFactory;
            this.UserMapper = userMapper;
        }

        /// <summary>
        /// Gets Cover Sheet data by a given proposal id.
        /// </summary>
        /// <param name="id">Proposal Id</param>
        /// <returns>Cover Sheet DTO</returns>
        public CoverSheetDataDto GetCoverSheetDataById(int id)
        {
            CoverSheetDataDto coverSheetDto = new CoverSheetDataDto();

            ProposalDto proposal = ProposalLoader.GetById(id);
            ProposalChecklistDto proposalChecklist = ProposalChecklistLoader.GetById(id);
            ContractsDto contracts = ContractsLoader.GetById(id);
            CageCodeDTO cageCodesDTO = CageCodesLoader.GetDataByCageCode(contracts.CageCode);

            // Get data from Proposal dto
            coverSheetDto.IsCCPDRequired = proposal.IsCCPDRequired;
            coverSheetDto.ContractActionType = proposal.ContractActionType;
            coverSheetDto.ContractTypeGroup = proposal.ContractTypeGroup;
            coverSheetDto.CoverSheetApproverSignedDate = proposal.CoverSheetApproverSignedDate;

            // Get data from Proposal Checklist dto
            coverSheetDto.CostThroughCom = proposalChecklist.CostThroughCom;
            coverSheetDto.ProfitFee = proposalChecklist.Profit;
            coverSheetDto.LMSpaceTotalPrice = proposalChecklist.SubmittedValue;

            // Get data from Contracts dto
            coverSheetDto.CustomerSubmittalDate = contracts.CustomerSubmittalDate;
            coverSheetDto.CageCode = contracts.CageCode;
            coverSheetDto.OfferorAddress.AddRange(new List<string>() { cageCodesDTO.Address1, cageCodesDTO.Address2, cageCodesDTO.City, cageCodesDTO.State, cageCodesDTO.Zip });

            // Get Cover Sheet Approver and Contracts Lead NTID by creating full proposal and checking permissions
            FullProposal fullProposal = ObjectFactory.CreateFullProposal(proposal);

            foreach (ProposalPermissionDto permission in fullProposal.Permissions)
            {
                UserDTO user = this.UserMapper.GetById(permission.UserId);

                switch (permission.Role)
                {
                    case PtmRole.CoverSheetApprover:
                        if (user != null)
                        {
                            coverSheetDto.CoverSheetApproverNtid = user.Ntid;
                        }
                        break;
                    case PtmRole.ContractsPOC:
                        if (user != null)
                        {
                            coverSheetDto.ContractsLead = user.Ntid;
                        }
                        break;
                    default:
                        break;
                }
            }

            return coverSheetDto;
        }
    }
}
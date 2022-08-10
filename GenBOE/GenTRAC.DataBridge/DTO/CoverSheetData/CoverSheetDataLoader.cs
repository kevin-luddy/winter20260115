// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System.Collections.Generic;
    using System.Linq;
    using GenTRAC.DataBridge.DTO.Contracts;
    using GenTRAC.Models;
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
        /// Gets or sets the Proposal Loader.
        /// </summary>
        private IProposalLoader ProposalLoader { get; set; }

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
        /// <param name="objectFactory">Object Factory</param>
        /// <param name="userMapper">User Mapper</param>
        public CoverSheetDataLoader(IProposalLoader proposalLoader, IFullObjectFactory objectFactory, IUserMapper userMapper) : this()
        {
            this.ProposalLoader = proposalLoader;
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
            CoverSheetDataDto coverSheetData = new CoverSheetDataDto();

            using (StopwatchTimer sw = new StopwatchTimer("CoverSheetDataLoader.GetCoverSheetDataById", Log))
            {
                using (genTRACEntities dbModel = new genTRACEntities())
                {
                    coverSheetData = (from props in dbModel.Proposals
                                      join pcl in dbModel.ProposalChecklists on props.ProposalID equals pcl.ProposalID into propspcl
                                      from pcl in propspcl.DefaultIfEmpty()
                                      join pc in dbModel.ProposalContractsDatas on (pcl == null ? 0 : pcl.ProposalID) equals pc.ProposalID into propspc
                                      from pc in propspc.DefaultIfEmpty()
                                      where props.ProposalID == id
                                      select new CoverSheetDataDto
                                      {
                                          IsCCPDRequired = props.CCPDRequired,
                                          ContractActionType = (ContractActionType?)props.ContractActionType,
                                          ContractTypeGroup = props.ContractTypeGroupID.HasValue ? props.ContractTypeGroupID.Value : 0,
                                          CoverSheetApproverSignedDate = props.CoverSheetApproverSignedDT,
                                          CostThroughCom = pcl.CostThroughCom,
                                          ProfitFee = pcl.Profit,
                                          LMSpaceTotalPrice = pcl.ISGSTotalPrice,
                                          CustomerSubmittalDate = pc.CustomerSubmittalDate,
                                          CageCode = pc.CageCode
                                      }).FirstOrDefault();

                    if (coverSheetData.CageCode != null)
                    {
                        // Get data for offeror's address from Cage Codes
                        CageCodeDTO cageCodeData = dbModel.CageCodes.Where(x => x.CageCode1 == coverSheetData.CageCode).Select(x => new CageCodeDTO()
                        {
                            Address1 = x.Address1,
                            Address2 = x.Address2,
                            City = x.City,
                            State = x.State,
                            Zip = x.Zip,
                        }).FirstOrDefault();

                        coverSheetData.OfferorAddress = new List<string>() { cageCodeData.Address1, cageCodeData.Address2, cageCodeData.City, cageCodeData.State, cageCodeData.Zip };
                    }

                    FullProposal fullProposal = ObjectFactory.CreateFullProposal(ProposalLoader.GetById(id));

                    if (fullProposal != null)
                    {
                        // Get Cover Sheet Approver and Contracts Lead NTID by creating full proposal and checking permissions
                        foreach (ProposalPermissionDto permission in fullProposal.Permissions)
                        {
                            UserDTO user = this.UserMapper.GetById(permission.UserId);

                            switch (permission.Role)
                            {
                                case PtmRole.CoverSheetApprover:
                                    if (user != null)
                                    {
                                        coverSheetData.CoverSheetApproverNtid = user.Ntid;
                                    }
                                    break;
                                case PtmRole.ContractsPOC:
                                    if (user != null)
                                    {
                                        coverSheetData.ContractsLead = user.Ntid;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }

            return coverSheetData;
        }
    }
}
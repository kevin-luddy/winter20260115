// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using IES.Standard;
    using IES.Standard.PickList;

    /// <summary>
    /// Pick List Mapper
    /// </summary>
    public class PtmPickListMapper : PickListMapper
    {
        #region Pick List Loaders

        /// <summary>
        /// LU Table Loader
        /// </summary>
        private ProposalTypeLULoader proposalLuLoader;

        /// <summary>
        /// LU Table Loader
        /// </summary>
        private ProposalClassLULoader proposalClassLuLoader;

        /// <summary>
        /// LU Table Loader
        /// </summary>
        private TypeOfRequestLULoader typeOfRequestLuLoader;

        /// <summary>
        /// LU Table Loader.
        /// </summary>
        private LineOfBusinessDataLoader lineOfBusinessLuLoader;

        /// <summary>
        /// The program area lu loader.
        /// </summary>
        private ProgramAreaDataLoader programAreaLuLoader;

        /// <summary>
        /// The contract type lu loader
        /// </summary>
        private ContractTypeLULoader contractTypeLuLoader;

        /// <summary>
        /// The contract type group lu loader
        /// </summary>
        private ContractTypeGroupLULoader contractTypeGroupLULoader;

        #endregion

        /// <summary>
        /// Default ctor
        /// </summary>
        /// <param name="proposalLuLoader">Proposal Lu Loader</param>
        /// <param name="levelOfCommitmentLoader">Level of Commitment Loader</param>
        /// <param name="typeOfRequestLoader">Type Of Request Loader</param>
        /// <param name="lineOfBusinessLuLoader">The LOB Loader</param>
        /// <param name="programAreaLuLoader">The Program Area Loader.</param>
        /// <param name="contractTypeLuLoader">The contract type lu loader.</param>
        /// <param name="contractTypeGroupLULoader">The contract type group lu loader.</param>
        public PtmPickListMapper(ProposalTypeLULoader proposalLuLoader, ProposalClassLULoader levelOfCommitmentLoader, TypeOfRequestLULoader typeOfRequestLoader, 
            LineOfBusinessDataLoader lineOfBusinessLuLoader, ProgramAreaDataLoader programAreaLuLoader,
            ContractTypeLULoader contractTypeLuLoader, ContractTypeGroupLULoader contractTypeGroupLULoader,
            ILogger logger) : base(logger)
        {
            this.proposalLuLoader = proposalLuLoader;
            this.proposalClassLuLoader = levelOfCommitmentLoader;
            this.typeOfRequestLuLoader = typeOfRequestLoader;
            this.lineOfBusinessLuLoader = lineOfBusinessLuLoader;
            this.programAreaLuLoader = programAreaLuLoader;
            this.contractTypeGroupLULoader = contractTypeGroupLULoader;
            this.contractTypeLuLoader = contractTypeLuLoader;
        }

        /// <summary>
        /// Gets loader for the specified pick list type
        /// </summary>
        /// <param name="pickListType">Pick list that we want to work on</param>
        /// <returns>The correct pick list</returns>
        protected override IPickListLoader GetPickListSettings(PickListEnum pickListType)
        {
            IPickListLoader loader = null;
            switch (pickListType)
            {
                case PickListEnum.ProposalType:
                    loader = this.proposalLuLoader;
                    break;
                case PickListEnum.TypeOfRequest:
                    loader = this.typeOfRequestLuLoader;
                    break;
                case PickListEnum.ProposalClass:
                    loader = this.proposalClassLuLoader;
                    break;
                case PickListEnum.LineOfBusiness:
                    loader = this.lineOfBusinessLuLoader;
                    break;
                case PickListEnum.ProgramArea:
                    loader = this.programAreaLuLoader;
                    break;
                case PickListEnum.ContractType:
                    loader = this.contractTypeLuLoader;
                    break;
                case PickListEnum.ContractTypeGroup:
                    loader = this.contractTypeGroupLULoader;
                    break;
                default:
                    break;
            }

            return loader;
        }
    }
}
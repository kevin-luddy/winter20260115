// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.Core.DTO.PickLists.Common
{
	using GenTRAC.DataBridge.Core.DTO.OrgData;
	using GenTRAC.DataBridge.Core.DTO.PickLists;
	using IES.Common.Core.Enums;
	using IES.Common.Core.PickList;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// Pick List Mapper
	/// </summary>
	public class PtmPickListMapper : PickListMapper
	{
		#region Pick List Loaders

		/// <summary>
		/// LU Table Loader
		/// </summary>
		private readonly ProposalTypeLULoader proposalLuLoader;

		/// <summary>
		/// LU Table Loader
		/// </summary>
		private readonly ProposalClassLULoader proposalClassLuLoader;

		/// <summary>
		/// LU Table Loader
		/// </summary>
		private readonly TypeOfRequestLULoader typeOfRequestLuLoader;

		/// <summary>
		/// LU Table Loader.
		/// </summary>
		private readonly LineOfBusinessDataLoader lineOfBusinessLuLoader;

		/// <summary>
		/// The program area lu loader.
		/// </summary>
		private readonly ProgramAreaDataLoader programAreaLuLoader;

		/// <summary>
		/// The contract type lu loader
		/// </summary>
		private readonly ContractTypeLULoader contractTypeLuLoader;

		/// <summary>
		/// The contract type group lu loader
		/// </summary>
		private readonly ContractTypeGroupLULoader contractTypeGroupLULoader;

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
			ILogger<PtmPickListMapper> logger) : base(logger)
		{
			this.proposalLuLoader = proposalLuLoader;
			proposalClassLuLoader = levelOfCommitmentLoader;
			typeOfRequestLuLoader = typeOfRequestLoader;
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
					loader = proposalLuLoader;
					break;
				case PickListEnum.TypeOfRequest:
					loader = typeOfRequestLuLoader;
					break;
				case PickListEnum.ProposalClass:
					loader = proposalClassLuLoader;
					break;
				case PickListEnum.LineOfBusiness:
					loader = lineOfBusinessLuLoader;
					break;
				case PickListEnum.ProgramArea:
					loader = programAreaLuLoader;
					break;
				case PickListEnum.ContractType:
					loader = contractTypeLuLoader;
					break;
				case PickListEnum.ContractTypeGroup:
					loader = contractTypeGroupLULoader;
					break;
				default:
					break;
			}

			return loader;
		}
	}
}
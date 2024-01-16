// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.Picklists
{
	using IES.Common.Core;
	using IES.Common.Core.PickList;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// Pick List Mapper
	/// </summary>
	public class BoePickListMapper : PickListMapper
	{
		#region Pick List Loaders

		/// <summary>
		/// The line of business loader
		/// </summary>
		private LineOfBusinessDataLoader lineOfBusinessLoader;

		/// <summary>
		/// LU Table Loader
		/// </summary>
		private ProposalClassLoader proposalClassLuLoader;

		/// <summary>
		/// The contract type lu loader
		/// </summary>
		private ContractTypeLoader contractTypeLuLoader;

		#endregion

		/// <summary>
		/// Default ctor
		/// </summary>
		public BoePickListMapper(LineOfBusinessDataLoader lineOfBusinessLoader, ProposalClassLoader proposalClassLuLoader,
			ContractTypeLoader contractTypeLuLoader, ILogger<BoePickListMapper> logger) : base(logger)
		{
			this.lineOfBusinessLoader = lineOfBusinessLoader;
			this.proposalClassLuLoader = proposalClassLuLoader;
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
				case PickListEnum.LineOfBusiness:
					loader = lineOfBusinessLoader;
					break;
				case PickListEnum.ProposalClass:
					loader = proposalClassLuLoader;
					break;
				case PickListEnum.ContractType:
					loader = contractTypeLuLoader;
					break;
				default:
					break;
			}

			return loader;
		}
	}
}
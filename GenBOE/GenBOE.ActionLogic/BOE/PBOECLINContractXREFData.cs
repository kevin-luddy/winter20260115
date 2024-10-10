// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
namespace GenBOE.ActionLogic.BOE
{

	public class PBOECLINContractXREFData
	{
		/// <summary>
		/// Id in database
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// CLIN Title
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Type of Contract in ContractTypeLU
		/// </summary>
		public int ContractTypeLUId { get; set; }

		/// <summary>
		/// Contract Type Name
		/// </summary>
		public string ContractTypeName { get; set; }
	}
}

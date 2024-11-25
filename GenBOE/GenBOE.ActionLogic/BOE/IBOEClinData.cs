// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.BOE
{
	using GenBOE.ActionLogic.IO.Export;

	/// <summary>
	/// IBOE CLIN Data
	/// </summary>
	public class IBOEClinData
	{
		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="row"></param>
		public IBOEClinData(IBOETableRow row)
		{
			if (row != null)
			{
				this.ContractType = row.ContractType;
				this.IWTAType = row.IWTAType;
				this.CLIN = row.CLIN;
				this.Value = row.Value;
			}
		}

		/// <summary>
		/// Contract Type
		/// </summary>
		public string ContractType { get; set; }

		/// <summary>
		/// IWTA Type
		/// </summary>
		public string IWTAType { get; set; }

		/// <summary>
		/// CLIN
		/// </summary>
		public string CLIN { get; set; }

		/// <summary>
		/// Rolled up value for this IWTA/CLIN Combo
		/// </summary>
		public decimal Value { get; set; }
	}
}

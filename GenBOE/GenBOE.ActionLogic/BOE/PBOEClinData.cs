// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2024 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.BOE
{
	using GenBOE.ActionLogic.IO.Export;

	/// <summary>
	/// PBOE ClIN Data Default Constructor 
	/// </summary>
	public class PBOEClinData
	{
		/// <summary>
		/// Constructor that takes in PBOETableRow
		/// </summary>
		/// <param name="row">PBOETableRow Model</param>
		public PBOEClinData(PBOETableRow row)
		{
			if (row != null)
			{
				this.WBS = row.WBS;
				this.ContractType = row.ContractType;
				this.CLIN = row.CLIN;
				this.Value = row.Value;
			}
		}

		/// <summary>
		/// The Contract type
		/// </summary>
		public string ContractType { get; set; }

		/// <summary>
		/// The WBS Number
		/// </summary>
		public string WBS { get; set; }

		/// <summary>
		/// The CLIN Name
		/// </summary>
		public string CLIN { get; set; }

		/// <summary>
		/// The rolled-up Value for this WBS/CLIN Combo.
		/// </summary>
		public decimal Value { get; set; }
	}
}

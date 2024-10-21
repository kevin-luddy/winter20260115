// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2023 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
	using System;
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// DTO containing CLIN and Contract Type XREF data for a PBOE
	/// </summary>
	[ExcludeFromCodeCoverage]
	[Serializable]
	public class PboeClinContractDto
	{
		/// <summary>
		/// Clin Title
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// Contract Type LU Name
		/// </summary>
		public string ContractType { get; set; }
	}
}

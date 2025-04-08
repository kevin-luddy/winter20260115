// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.DTO
{
	using GenBOE.DataBridge.Core.DTO.Common;
	using IES.Common;
	using IES.Common.Core.Constants;
	using IES.Common.Core.Models;
	using IES.Common.Core.Utilities;
	using System;
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// DTO that will contain all data for a single CLIN
	/// </summary>
	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class ClinDTO : UpdateableDTO, IWorkspaceMembership
	{
		public ClinDTO()
		{
			Id = -1;
			ClinNumber = string.Empty;
			ClinTitle = string.Empty;
			ClinPaddedNumber = string.Empty;
			StartDate = DateTime.MinValue;
			EndDate = DateTime.MinValue;
			WorkspaceID = 0;
			UpdateDate = DateTime.Now;
			InUse = false;
			ContractType = CommonConstants.CONTRACT_TYPE_NOT_SET;
		}

		public string ClinNumber { get; set; }
		public string ClinTitle { get; set; }

		/// <summary>
		/// This is the Clin Number + Clin Title
		/// </summary>
		public string ClinString
		{
			get { return CommonUtilities.FormatNumberTitleString(ClinNumber, ClinTitle, " "); }
		}

		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }

		public int WorkspaceID { get; set; }

		public bool InUse { get; set; }

		public string ClinPaddedNumber { get; set; }

		public int ContractType { get; set; }
	}
}

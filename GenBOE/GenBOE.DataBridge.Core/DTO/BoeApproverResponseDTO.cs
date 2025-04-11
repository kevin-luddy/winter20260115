using System;
using System.Diagnostics.CodeAnalysis;
using GenBOE.DataBridge.Core.DTO.Common;
using IES.Common;
using IES.Common.Core.Enums;
using IES.Common.Core.Models;

namespace GenBOE.DataBridge.Core.DTO
{
	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class BoeApproverResponseDTO : UpdateableDTO, IBOEMembership
	{
		public BoeApproverResponseDTO()
		{
			Id = -1;
			ETIUserID = -1;
			ApproverResponse = ApproverReponseType.None;
			BoeID = -1;
			ApproverResponded = false;
			CurrentUserETIUserID = -1;
		}

		public int ETIUserID { get; set; }
		public ApproverReponseType ApproverResponse { get; set; }
		public int BoeID { get; set; }
		public bool? ApproverResponded { get; set; }

		/// <summary>
		/// NOTE: This field is NOT populated in the loader, it is only for saves. 
		/// </summary>
		public int? CurrentUserETIUserID { get; set; }
	}
}

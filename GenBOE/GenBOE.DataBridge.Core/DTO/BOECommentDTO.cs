// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.DTO
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using GenBOE.DataBridge.Core.DTO.Common;
	using IES.Common;
	using IES.Common.Core.Models;

	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class BOECommentDTO : UpdateableDTO, IBOEMembership
	{
		public BOECommentDTO()
		{
			Id = -1;
			FieldID = -1;
			BOEComment = string.Empty;
			BOEResponseToCommentID = null;
			BOECommentETIUserID = -1;
			BoeID = -1;
		}

		public int FieldID { get; set; }
		public string BOEComment { get; set; }
		public int? BOEResponseToCommentID { get; set; }
		public int BOECommentETIUserID { get; set; }
		public int BoeID { get; set; }
	}
}

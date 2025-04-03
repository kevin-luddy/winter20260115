// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.DTO
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using IES.Common;
	using IES.Common.Core.Models;

	/// <summary>
	/// Location DTO
	/// </summary>
	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class LocationDTO : UpdateableDTO
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public LocationDTO()
		{
			this.Id = -1;
			LocationName = string.Empty;
			LastUpdatedBy = -1;
		}

		/// <summary>
		/// Gets or sets LocationName
		/// </summary>
		public string LocationName { get; set; }

		/// <summary>
		/// Gets or sets LastUpdatedBy
		/// </summary>
		public int? LastUpdatedBy { get; set; }
	}
}

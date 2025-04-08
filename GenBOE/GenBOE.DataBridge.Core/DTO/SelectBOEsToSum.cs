// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.DTO
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using IES.Common;
	using IES.Common.Core.Models;

	[ExcludeFromCodeCoverage]
	[Serializable()]
	/// The SelectBOEsToSum is used within a workspace and task variable if WorkspaceVarValueType equals SumOfBOEs
	/// NOTE: made this an UpdateableDTO so it would work within the new BulkSave framework
	public class SelectBOEsToSum : UpdateableDTO
	{
		public SelectBOEsToSum()
		{
		}

		public int? WBSID { get; set; }
		public int? CLINID { get; set; }
		public int? BoeID { get; set; }

		/// <summary>
		/// Primary key of the xREF table. Need this in order to support bulk delete.
		/// </summary>
		public long OVSumID { get; set; }

		/// <summary>
		/// Ordinary variable that pulls together the sum.
		/// NOTE: this was added to support the new Bulksave framework which requires each DTO to hold the id of it's parent. in this case, that's the ordinary
		/// variable id.
		/// </summary>
		public int? OrdinaryVariableID { get; set; }

		private ICollection<int> childBOEIDList = new List<int>();

		/// <summary>
		/// Used only as part of variable dependency resolution.
		/// </summary>
		public ICollection<int> ChildBoeIDs { get { return childBOEIDList; } }
	}
}

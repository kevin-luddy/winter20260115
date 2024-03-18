// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.PickList
{
	using System;
	using System.Collections.Generic;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Models;

	/// <summary>
	/// Pick list Dto
	/// </summary>
	[Serializable]
	public class PickListDto : UpdateableDTO
	{
		/// <summary>
		/// Default Ctor
		/// </summary>
		public PickListDto()
		{
			Id = -1;
			IsReadOnly = false;
			Updateable = UpdateType.None;
			ParentIds = new List<int>();
		}

		/// <summary>
		/// Text
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Is Active?
		/// </summary>
		public bool IsActive { get; set; }

		/// <summary>
		/// Is in use?
		/// </summary>
		public bool InUse { get; set; }

		/// <summary>
		/// Is Read-Only?
		/// </summary>
		public bool IsReadOnly { get; set; }

		/// <summary>
		/// Is Active text used in the grid
		/// </summary>
		public string IsActiveText
		{
			get
			{
				return IsActive ? "Active" : "Inactive";
			}
		}

		/// <summary>
		/// In Use text used in the grid
		/// </summary>
		public string InUseText
		{
			get { return InUse ? "In Use" : string.Empty; }
		}

		/// <summary>
		/// Read-Only text used in the grid
		/// </summary>
		public string IsReadOnlyText
		{
			get { return IsReadOnly ? "Read-Only" : string.Empty; }
		}

		/// <summary>
		/// Sets the parent identifier for single parents.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1044:PropertiesShouldNotBeWriteOnly")]
		internal int ParentId
		{
			set
			{
				ParentIds = new List<int> { value };
			}
		}

		/// <summary>
		/// Sets the parent ids using IEnumerable from the Database.
		/// </summary>
		internal IEnumerable<int> IenumParentIds { get; set; }

		/// <summary>
		/// Gets or sets the parent identifier.
		/// </summary>
		public ICollection<int> ParentIds { get; set; }

		/// <summary>
		/// Gets or sets the parent names.
		/// </summary>
		public ICollection<string> ParentNames { get; set; } = new List<string>();
	}
}

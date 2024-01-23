// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Interfaces
{
	using System;
	using System.Collections.Generic;
	using IES.Common.Core.Enums;

	/// <summary>
	/// Represents an object that can be Date Shifted.
	/// </summary>
	public interface IDateShiftable
	{
		// the start date
		DateTime? StartDate { get; set; }

		// the end date
		DateTime? EndDate { get; set; }

		/// <summary>
		/// Gets the children that can be shifted.
		/// </summary>
		ICollection<IDateShiftable> Children { get; }

		/// <summary>
		/// Gets a value indicating whether this instance has a spread of values.
		/// </summary>
		bool HasSpread { get; }

		/// <summary>
		/// Gets the date shift level.
		/// </summary>
		Level DateShiftLevel { get; }

		/// <summary>
		/// Update Type
		/// </summary>
		UpdateType Updateable { get; set; }
	}
}

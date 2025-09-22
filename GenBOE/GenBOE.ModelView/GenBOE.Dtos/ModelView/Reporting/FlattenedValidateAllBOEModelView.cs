// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
	/// <summary>
	/// Flattened model view for Validate ALL BOE
	/// </summary>
	public class FlattenedValidateAllBOEModelView
	{
		/// <summary>
		/// Whether this row is Header or not
		/// </summary>
		public bool IsHeader { get; set; }

		/// <summary>
		/// Grouping by BOEId
		/// </summary>
		public int BOEId { get; set; }

		/// <summary>
		/// Task Id (if applicable)
		/// </summary>
		public int TaskId { get; set; }

		/// <summary>
		/// Labor/ODC/Travel/Material
		/// </summary>
		public FlattenedValidateTaskType TaskType { get; set; }

		/// <summary>
		/// BOE text
		/// </summary>
		public string BOE { get; set; }

		/// <summary>
		/// Level text
		/// </summary>
		public string Level { get; set; }

		/// <summary>
		/// Section Text
		/// </summary>
		public string Section { get; set; }

		/// <summary>
		/// Message Text
		/// </summary>
		public string Message { get; set; }
	}

	/// <summary>
	/// Flattened Validate Task Type
	/// </summary>
	public enum FlattenedValidateTaskType
	{
		None = 0,
		Labor = 1,
		ODC = 2,
		Travel = 3,
		Material = 4
	}
}

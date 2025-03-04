// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.DTO
{
	using System;
	using GenBOE.DataBridge.Core.DTO.Common;
	using IES.Common.Core.Models;

	/// <summary>
	/// Material Task Element
	/// </summary>
	[Serializable()]
	public class MaterialDTO : UpdateableDTO, IBOEMembership
	{
		public MaterialDTO()
		{
			this.Id = -1;
			TaskID = string.Empty;
			TaskTitle = string.Empty;
			StartDate = null;
			EndDate = null;

			taskDescription = null;
			moqText = null;
			WasMoqTextSet = false;
			WasDescriptionSet = false;
			preventRteDbLoad = false;
		}

		public string TaskID { get; set; }
		public string TaskTitle { get; set; }

		#region RTE Fields

		private string taskDescription;
		private string moqText;

		/// <summary>
		/// Indicates whether the Description field was set (either from user, or via RTE load)
		/// </summary>
		public bool WasDescriptionSet { get; set; }

		/// <summary>
		/// Indicates whether the MoqText field was set (either from user, or via RTE load)
		/// </summary>
		public bool WasMoqTextSet { get; set; }

		/// <summary>
		/// This is used to mark when we make the DB call, to prevent all subsequent calls, as RTE fields were loaded already
		/// </summary>
		private bool preventRteDbLoad { get; set; }

		// the Description
		public string TaskDescription
		{
			get
			{
				return taskDescription;
			}
			set
			{
				taskDescription = value;
				WasDescriptionSet = true;
			}
		}

		// The Moq Text
		public string MoqText
		{
			get
			{
				return moqText;
			}
			set
			{
				moqText = value;
				WasMoqTextSet = true;
			}
		}

		#endregion

		public int BoeID { get; set; }

		// the task start date, the earliest Material Type Expend Date
		public DateTime? StartDate { get; set; }

		// the task end date, the latest Material Type Expend Date
		public DateTime? EndDate { get; set; }

		#region Required Validation Messages
		public const string MOQ_TEXT_REQUIRED = "{0} is required.";
		public const string RESOURCE_TYPE_FOR_TASK_ELEMENT_REQUIRED = "At least one material is required for a task element.";
		public const string MATERIAL_TYPE_EXPEND_DATE_INVALID = "Expend Date must be between {0} and {1}.";
		#endregion Required Validation Messages
	}
}

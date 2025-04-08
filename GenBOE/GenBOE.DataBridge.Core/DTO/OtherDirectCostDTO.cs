// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.DTO
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using GenBOE.DataBridge.Core.DTO.Common;
	using IES.Common;
	using IES.Common.Core.Interfaces;
	using IES.Common.Core.Models;

	[Serializable()]
	public class OtherDirectCostDTO : UpdateableDTO, IBOEMembership, IStartEndDates
	{
		public OtherDirectCostDTO()
		{
			this.Id = -1;
			TaskID = string.Empty;
			TaskTitle = string.Empty;
			ODCTypes = new Collection<OtherDirectCostType>();
			StartDate = DateTime.MinValue;
			EndDate = DateTime.MinValue;

			taskDescription = null;
			moqText = null;
			WasMoqTextSet = false;
			WasDescriptionSet = false;
			preventRteDbLoad = false;
		}

		public string TaskID { get; set; }

		/// <summary>
		/// The TaskID string field from User Input
		/// </summary>
		public string BOETaskID { get; set; }

		/// <summary>
		/// The value of the order in which the task will appear in the boe listing
		/// </summary>
		public int BOETaskElementOrder { get; set; }

		public string TaskTitle { get; set; }

		#region RTE Fields

		private string taskDescription;
		private string moqText;

		/// <summary>
		/// Indicates whether the Description field was set (either from user, or via RTE load)
		/// </summary>
		public bool WasDescriptionSet { get; set; }

		/// <summary>
		/// Indicates whether the Data Source field was set (either from user, or via RTE load)
		/// </summary>
		public bool WasMoqTextSet { get; set; }

		/// <summary>
		/// This is used to mark when we make the DB call, to prevent all subsequent calls, as RTE fields were loaded already
		/// </summary>
		private bool preventRteDbLoad { get; set; }

		// the BOE description
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

		// The data source
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

		public Collection<OtherDirectCostType> ODCTypes { get; set; }

		public int BoeID { get; set; }

		// the task start date, the earliest ODC Type start date
		public DateTime? StartDate { get; set; }

		// the task end date, the latest ODC Type end date
		public DateTime? EndDate { get; set; }

		#region Required Validation Messages
		public const string MOQ_TEXT_REQUIRED = "{0} is required.";
		public const string RESOURCE_CODE_REQUIRED = "Resource is required.";
		public const string PERFORM_ORG_REQUIRED = "Performing Org is required.";
		public const string SPREAD_REQUIRED = "At least one spread is required for each ODC Type.";
		public const string AT_LEAST_ONE_ODC_TYPE_REQUIRED = "At least one ODC is required for a task element.";

		#endregion Required Validation Messages
	}
}

// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic._ModelView.Backend
{
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.ActionLogic.Validation;
	using GenBOE.ActionLogic.ValidationAttributes;
	using GenBOE.DataBridge.DTO;
	using GenBOE.Dtos;
	using IES.Common;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel.DataAnnotations;

	/// <summary>
	/// BOE Header View Model for new Angular
	/// </summary>
	public class BOEHeaderViewModel: PersistedDataModelView
	{
		public BOEHeaderViewModel()
		{
			this.BOEID = -1;
			this.CLIN = string.Empty;
			this.WBS = string.Empty;
			this.CustomFieldValues = new Collection<BOECustomFieldModelView>();
			this.HeaderRteTemplateAnswers = new List<RTECustomTemplateQuestionAnswerModelView>();
			this.DataSource = string.Empty;
			this.HistoricMetricDisclosureChecked = false;
		}

		public BOEHeaderViewModel(BoeDTO inBoe, ICollection<RTECustomTemplateQuestionAnswerModelView> answers) : this()
		{
			if (inBoe != null)
			{
				this.BOEID = inBoe.Id;
				if (inBoe.StartDate.ToString("MM/yyyy") != DateTime.MinValue.ToString("MM/yyyy"))
				{
					this.StartDate = inBoe.StartDate;
				}
				if (inBoe.EndDate.ToString("MM/yyyy") != DateTime.MinValue.ToString("MM/yyyy"))
				{
					this.EndDate = inBoe.EndDate;
				}
				this.Title = inBoe.Title;
				this.UpdateDate = inBoe.UpdateDate;
				this.DataSource = inBoe.DataSource;
				this.HistoricMetricDisclosureChecked = inBoe.HistoricMetricDisclosureChecked;
				this.HeaderRteTemplateAnswers = answers;
			}
		}

		/// <summary>
		/// Gets or sets the RTE Custom Template Answers at BOE level.
		/// </summary>
		public ICollection<RTECustomTemplateQuestionAnswerModelView> HeaderRteTemplateAnswers { get; }

		/// <summary>
		/// Get or sets Clin
		/// </summary>
		public string CLIN { get; set; }

		/// <summary>
		/// Get or sets Start date
		/// </summary>
		[DisplayFormat(DataFormatString = "{0:MM/yyyy}")]
		public DateTime? StartDate { get; set; }

		/// <summary>
		/// Get or sets StaEndrt date
		/// </summary>
		[DisplayFormat(DataFormatString = "{0:MM/yyyy}")]
		public DateTime? EndDate { get; set; }

		/// <summary>
		/// Get or sets BOE Id
		/// </summary>
		public int BOEID { get; set; }

		/// <summary>
		/// Get or sets WBS
		/// </summary>
		public string WBS { get; set; }

		/// <summary>
		/// Get or set RTE field size character limit
		/// </summary>
		public int RteFieldSize { get; set; }

		/// <summary>
		/// Gets or sets the BOE state
		/// </summary>
		public BOEState State { get; set; }

		/// <summary>
		/// Gets the status
		/// </summary>
		public string Status
		{
			get
			{
				if (this.State == BOEState.None)
				{
					return string.Empty;
				}
				else if (this.State == BOEState.DraftLocked)
				{
					return BOEState.Draft.GetDescription();
				}
				else
				{
					return this.State.GetDescription();
				}
			}
		}

		/// <summary>
		/// Get or sets Historic Metric Disclosure Checked
		/// </summary>
		public bool HistoricMetricDisclosureChecked { get; set; }

		/// <summary>
		/// Gets or sets Custom Field Values
		/// </summary>
		public Collection<BOECustomFieldModelView> CustomFieldValues { get; set; }

		/// <summary>
		/// Sources of Data
		/// </summary>
		[HtmlTextLength(ValidationConstants.MAX_SOURCES_OF_DATA_LENGTH)]
		[RichText(RichTextDbColumn.BOE_DATA_SOURCE, "BOEID")]
		[Display(Name = "Sources of Data")]
		public string DataSource { get; set; }

		[StringLength(ValidationConstants.MAX_BOE_TITLE_LENGTH, ErrorMessage = "A maximum of 100 characters are allowed")]
		public string Title { get; set; }

		/// <summary>
		/// Indicates if the Title field is required
		/// </summary>
		public bool IsTitleRequired { get; set; }

		/// <summary>
		/// Gets the Sources of data label
		/// </summary>
		public string LabelSourcesOfData { get; set; }

		/// <summary>
		/// Gets BOEHeaderDescriptionModelView
		/// </summary>
		public BOEHeaderDescriptionModelView Description { get; set; }

		public AdjacentItems AdjacentBoeIds { get; set; }

		
	}
}

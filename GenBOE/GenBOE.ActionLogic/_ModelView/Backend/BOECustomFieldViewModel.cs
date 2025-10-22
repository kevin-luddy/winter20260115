// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView
{
	using System;
	using System.Collections.ObjectModel;
	using System.ComponentModel.DataAnnotations;

	/// <summary>
	/// Custom Field Selection MV
	/// </summary>
	public class BOECustomFieldViewModel : PersistedDataModelView
	{
		/// <summary>
		/// default contructor
		/// </summary>
		public BOECustomFieldViewModel()
		{
			this.SelectionID = -1;
			this.CustomFieldValueID = -1;
			this.CustomFieldID = -1;
			this.IsOpenEnded = false;
			this.OpenEndedValue = String.Empty;
			this.CustomFieldOptions = new Collection<BOECustomFieldOptionModelView>();
		}

		public Collection<BOECustomFieldOptionModelView> CustomFieldOptions { get; set; }

		/// <summary>
		/// gets/sets primary key in custom field value xref table
		/// </summary>
		public int SelectionID { get; set; }

		/// <summary>
		/// gets/sets custom field value ID
		/// </summary>
		public int CustomFieldValueID { get; set; }

		/// <summary>
		/// gets/sets custom field ID
		/// </summary>
		public int CustomFieldID { get; set; }

		/// <summary>
		/// gets/sets bool noting if field is open ended
		/// </summary>
		public bool IsOpenEnded { get; set; }

		/// <summary>
		/// Gets/Sets the value of an open ended Custom Field
		/// Not used for standard custom fields
		/// </summary>
		[StringLength(250, ErrorMessage = "A maximum of 250 characters is allowed for an open ended custom field.")]
		public string OpenEndedValue { get; set; }

		/// <summary>
		/// Get/Sets the fieldName for the Custom Field
		/// </summary>
		public string FieldName { get; set; }
	}
}
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Standard.Exceptions
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a validation object that can be consumed by the Generation JS lib.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ValidationMessage
    {
        /// <summary>
        /// Gets or sets a value indicating whether to treat this validation as a warning.
        /// </summary>
        public bool TreatAsWarning { get; set; }

        /// <summary>
        /// Optional field, currently used for more information
        /// </summary>
        public string FieldName { get; set; }
        
        /// <summary>
        /// What you want the validation to say.
        /// </summary>
        public string ValidationIssue { get; set; }

        /// <summary>
        /// Optional field, only to be used if the button pressed to perform the save is not in the same form that contains the validation message box you want to target.
        /// For example, in a composite widget you save all data at once and want to return validation to multiple forms.
        /// </summary>
        public string FormIDToTarget { get; set; }

        /// The PKID for the row throwing the error.  
        /// Either this or rowIndex should be set, there's no need to set both.
        /// </summary>
        public int? PkId { get; set; }
        
		/// <summary>
        /// The index of the row throwing the error.
        /// This property is optional
        /// </summary>
        public int? RowIndex { get; set; }

        /// <summary>
        /// The item being indexed.  Only has a value if RowIndex has a value.
        /// </summary>
        public string IndexedItem { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationMessage"/> class.
        /// </summary>
        public ValidationMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationMessage"/> class.
        /// </summary>
        /// <param name="validationIssue">The validation issue.</param>
        public ValidationMessage(string validationIssue)
        {
            this.ValidationIssue = validationIssue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationMessage"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="validationIssue">The validation issue.</param>
        public ValidationMessage(string fieldName, string validationIssue)
        {
            this.FieldName = fieldName;
            this.ValidationIssue = validationIssue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationMessage"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="validationIssue">The validation issue.</param>
        /// <param name="formIDToTarget">The form identifier to target.</param>
        public ValidationMessage(string fieldName, string validationIssue, string formIDToTarget)
        {
            this.FieldName = fieldName;
            this.ValidationIssue = validationIssue;
            this.FormIDToTarget = formIDToTarget;
        }
    }
}

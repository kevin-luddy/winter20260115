// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2018 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Common.Exceptions
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a validation object that can be consumed by the Generation JS lib.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ValidationMessage
    {
        /// <summary>
        /// Optional feild, currently has no use.
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

        /// <summary>
        /// The PKID for the row throwing the error.  
        /// Either this or rowIndex should be set, there's no need to set both.
        /// </summary>
        public int? PkId { get; set; }

        /// <summary>
        /// The index of the row throwing the error.
        /// Either this or pkid should be set, there's no need to set both.
        /// </summary>
        public int? RowIndex { get; set; }

        /// <summary>
        /// The item being indexed.  Only has a value if RowIndex has a value.
        /// </summary>
        public string IndexedItem { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ValidationMessage()
        {
        }

        /// <summary>
        /// Constructor with issue
        /// </summary>
        /// <param name="inValidationIssue">issue</param>
        public ValidationMessage(string inValidationIssue)
        {
            this.ValidationIssue = inValidationIssue;
        }

        /// <summary>
        /// Constructor with field and issue
        /// </summary>
        /// <param name="inFieldName">field</param>
        /// <param name="inValidationIssue">issue</param>
        public ValidationMessage(string inFieldName, string inValidationIssue)
        {
            this.FieldName = inFieldName;
            this.ValidationIssue = inValidationIssue;
        }

        /// <summary>
        /// Constructor with field, issue, and target form
        /// </summary>
        /// <param name="inFieldName">field</param>
        /// <param name="inValidationIssue">issue</param>
        /// <param name="inFormIDToTarget">target from</param>
        public ValidationMessage(string inFieldName, string inValidationIssue, string inFormIDToTarget)
        {
            this.FieldName = inFieldName;
            this.ValidationIssue = inValidationIssue;
            this.FormIDToTarget = inFormIDToTarget;
        }
    }
}

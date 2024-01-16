// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    using IES.Common.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Model View for the Version Comparison Grid rows
    /// </summary>
    public class VersionComparisonGridRowModelView
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public VersionComparisonGridRowModelView()
        {
            this.FieldChanged = string.Empty;
            this.OldValue = string.Empty;
            this.NewValue = string.Empty;
            this.ChangeType = RDMChangeType.None;
        }

        /// <summary>
        /// Gets or sets the revision unique section id used to sort the view models.
        /// </summary>
        [JsonIgnore]
        public int RevisionUniqueSectionId { get; set; }

        /// <summary>
        /// Gets/Sets the text for the field that was changed
        /// </summary>
        public string FieldChanged { get; set; }

        /// <summary>
        /// Gets/Sets the value from the previous version
        /// </summary>
        public string OldValue { get; set; }

        /// <summary>
        /// Gets/Sets the value from the current version
        /// </summary>
        public string NewValue { get; set; }

        /// <summary>
        /// Gets/Sets the change type - add, edit, or delete
        /// </summary>
        public RDMChangeType ChangeType { get; set; }

        /// <summary>
        /// Gets the Change Type Description String
        /// </summary>
        public string ChangeTypeDescription
        {
            get
            {
                return this.ChangeType.ToDescription();
            }
        }
    }
}
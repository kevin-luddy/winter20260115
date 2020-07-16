// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using System.Collections.Generic;
    using IES.Common;

    /// <summary>
    /// The RTE Custom Template Model View.
    /// </summary>
    public class RteCustomTemplateModelView : UpdateableDTO
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public RteCustomTemplateModelView ()
        {
            this.Questions = new List<RteCustomTemplateQuestionModelView>();
            this.Assigned = new List<int>();
        }

        /// <summary>
        /// Gets or sets the Workspace Name.
        /// </summary>
        public string WorkspaceName { get; set; }

        /// <summary>
        /// Gets or sets the Workspace Id.
        /// </summary>
        public int WorkspaceId { get; set; }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Author.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// The User Id for the Author.
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// Gets the Creation Date as string.
        /// </summary>
        public string CreationDate { get { return this.CreatedOn.ToString(Constants.DATE_FORMATTING_MONTH_DAY_YEAR); } }

        /// <summary>
        /// Gets the Last Updated Date as string.
        /// </summary>
        public string LastUpdatedDate {  get { return this.UpdateDate.ToString(Constants.DATE_FORMATTING_MONTH_DAY_YEAR); } }

        /// <summary>
        /// Gets or sets the Created On Date as DateTime.
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Gets or sets whether this custom template is in use.
        /// </summary>
        public bool InUse { get; set; }

        public ICollection<int> Assigned { get; set; }

        public IEnumerable<int> AssignedList { get; set; }

        /// <summary>
        /// Gets or sets the list of questions.
        /// </summary>
        public ICollection<RteCustomTemplateQuestionModelView> Questions { get; set; }
    }
}

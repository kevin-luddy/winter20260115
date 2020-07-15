// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System.Collections.Generic;
    using IES.Common;

    /// <summary>
    /// Model View for RTE Custom Template Questions.
    /// </summary>
    public class RteCustomTemplateQuestionModelView : UpdateableDTO
    {
        public RteCustomTemplateQuestionModelView() 
        {
            this.SortOrder = 1000;
        }

        /// <summary>
        /// Gets or sets the Question text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets whether the question is required.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Gets or sets the sort order of the Question.
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Gets or sets the Template Id.
        /// </summary>
        public int TemplateId { get; set; }

        /// <summary>
        /// Source IDs that the template is used by
        /// </summary>
        public ICollection<RteTemplateSource> SourceIds { get; set; }
    }
}

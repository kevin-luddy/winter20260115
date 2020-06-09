// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using System.Collections.Generic;
    using IES.Common;

    /// <summary>
    /// Model View for RTE Custom Template Questions and Answers.
    /// </summary>
    [Serializable()]
    public class RTECustomTemplateQuestionAnswerModelView : UpdateableDTO
    {

        public RTECustomTemplateQuestionAnswerModelView()
        {
            this.SortOrder = 1000;
        }

        public RTECustomTemplateQuestionAnswerModelView(RTECustomTemplateQuestionAnswerModelView question, int boeId, int? taskId, int sourceId)
        {
            if (question == null)
            {
                throw new ArgumentNullException(nameof(question));
            }

            this.Id = -1;
            this.QuestionId = question.QuestionId;
            this.QuestionText = question.QuestionText;
            this.Required = question.Required;
            this.SortOrder = question.SortOrder;
            this.TemplateId = question.TemplateId;
            this.BoeId = boeId;
            this.TaskId = taskId;
            this.SourceId = sourceId;
        }

        /// <summary>
        /// 
        /// </summary>
        public int QuestionId { get; set; }

        /// <summary>
        /// Gets or sets the Question text.
        /// </summary>
        public string QuestionText { get; set; }

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
        /// Gets or sets the Answer text, the Rich Text DB Column is not used.
        /// </summary>
        [RichText(RichTextDbColumn.None, "Id")]
        public string AnswerText { get; set; }

        /// <summary>
        /// Gets or sets the BOE that the answer is tied to.
        /// </summary>
        public int BoeId { get; set; }

        /// <summary>
        /// Gets or sets the Task Id that the answer is tied to, this can be null.
        /// </summary>
        public int? TaskId { get; set; }

        /// <summary>
        /// Gets or sets the Template Source Id.
        /// </summary>
        public int SourceId { get; set; }

        /// <summary>
        /// Gets or sets the enumeration for Source Ids.
        /// </summary>
        internal IEnumerable<int> SourceIdEnum { get; set; } 

        /// <summary>
        /// Gets or sets the List of Source Ids (used during retrieval from database).
        /// </summary>
        internal ICollection<int> SourceIdList { get; set; }
    }
}

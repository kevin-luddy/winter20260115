// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using IES.Standard;

    /// <summary>
    /// Model View for RTE Custom Template Questions and Answers.
    /// </summary>
    [Serializable()]
    public class RTECustomTemplateAnswerModelView : UpdateableDTO
    {
        /// <summary>
        /// Question Id
        /// </summary>
        public int QuestionId { get; set; }

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
        public RteTemplateSource Source { get; set; }
    }
}

// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
    using System.Collections.Generic;
    using GenBOE.Dtos;

    /// <summary>
    /// Model View for RTE Template ascx control.
    /// </summary>
    public class RteTemplateModelView
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="templates">The templates.</param>
        /// <param name="templateName">The name for the template.</param>
        /// <param name="nonTemplateValue">The non-template value.</param>
        public RteTemplateModelView(ICollection<RTECustomTemplateQuestionAnswerModelView> templates, string templateName, string nonTemplateValue)
        {
            this.TemplateName = templateName;
            this.Templates = templates;
            this.NonTemplateValue = nonTemplateValue;
        }

        /// <summary>
        /// Gets or sets the templates.
        /// </summary>
        public ICollection<RTECustomTemplateQuestionAnswerModelView> Templates { get; set; }

        /// <summary>
        /// Gets or sets the template name.
        /// </summary>
        public string TemplateName { get; set; }

        /// <summary>
        /// Gets or sets the Non-template value.
        /// </summary>
        public string NonTemplateValue { get; set; }
    }
}
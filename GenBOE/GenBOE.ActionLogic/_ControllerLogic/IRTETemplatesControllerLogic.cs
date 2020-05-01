// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic
{
    using System.Collections.Generic;
    using GenBOE.ActionLogic.ControllerLogic;
    using GenBOE.Dtos;
    using GenBOE.Objects;

    public interface IRTETemplatesControllerLogic : IGenBOEControllerLogic
    {
        /// <summary>
        /// Retrieves Templates for the specific workspace
        /// </summary>
        /// <param name="wsId">Workspace Id</param>
        /// <returns>RTE Templates</returns>
        ICollection<RteCustomTemplateModelView> GetTemplates(int wsId);

        /// <summary>
        /// Gets all of the sources from lookup table.
        /// </summary>
        /// <returns>All of the sources from lookup table.</returns>
        ICollection<RteCustomTemplateSourceModelView> GetSources();

        /// <summary>
        /// Validates RTE templates
        /// </summary>
        /// <param name="templates">Templates to validate</param>
        /// <exception cref="GenValidationException">Throws GenValidationException with validation errors, if any</exception>
        void ValidateTemplates(ICollection<RteCustomTemplateModelView> templates);

        /// <summary>
        /// Save Templates
        /// </summary>
        /// <param name="templates">Templates to save</param>
        /// <param name="ws">WS to which the template belongs</param>
        void SaveTemplates(ICollection<RteCustomTemplateModelView> templates, FullWorkspace ws);

        /// <summary>
        /// Search templates
        /// </summary>
        /// <param name="search">Search Text</param>
        /// <returns>Matching results</returns>
        ICollection<RteCustomTemplateModelView> SearchTemplates(string search);

        /// <summary>
        /// Copies selected template into specified workspace
        /// </summary>
        /// <param name="templateId">Template Id</param>
        /// <param name="ws">Target WS</param>
        void CopyTemplate(int templateId, FullWorkspace ws);
    }
}

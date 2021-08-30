// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
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
        /// <param name="usingTemplateBOE">Is the WS using Template BOEs</param>
        /// <returns>All of the sources from lookup table.</returns>
        ICollection<RteCustomTemplateSourceModelView> GetSources(bool usingTemplateBOE);

        /// <summary>
        /// Validates RTE templates
        /// </summary>
        /// <param name="templates">Templates to validate</param>
        /// <param name="wsId">Workspace Id</param>
        /// <param name="moveDeletedPromptData">Whether to move the deleted prompt data, or delete it if false</param>
        /// <param name="moveToPrompt">ID of the Prompt to move the deleted prompt data to</param>
        /// <exception cref="GenValidationException">Throws GenValidationException with validation errors, if any</exception>
        void ValidateTemplates(ICollection<RteCustomTemplateModelView> templates, int wsId, bool moveDeletedPromptData, int? moveToPrompt);

        /// <summary>
        /// Save Templates
        /// </summary>
        /// <param name="templates">Templates to save</param>
        /// <param name="ws">WS to which the template belongs</param>
        /// <param name="moveDeletedPromptData">Whether to move the deleted prompt data, or delete it if false</param>
        /// <param name="moveToPrompt">ID of the Prompt to move the deleted prompt data to</param>
        void SaveTemplates(ICollection<RteCustomTemplateModelView> templates, FullWorkspace ws, bool moveDeletedPromptData, int? moveToPrompt);

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
        /// <param name="newTemplateName">New Template Name</param>
        void CopyTemplate(int templateId, FullWorkspace ws, string newTemplateName);
    }
}

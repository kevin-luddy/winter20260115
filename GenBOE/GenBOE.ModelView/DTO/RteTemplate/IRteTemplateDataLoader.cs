// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;

    /// <summary>
    /// Interface for IRteTemplateDataLoader
    /// </summary>
    public interface IRteTemplateDataLoader : IDataLoader<RteCustomTemplateModelView>
    {
        /// <summary>
        /// Gets all of the sources from lookup table.
        /// </summary>
        /// <returns>All of the sources from lookup table.</returns>
        ICollection<RteCustomTemplateSourceModelView> GetSources();

        /// <summary>
        /// Gets all templates for a workspace
        /// </summary>
        /// <param name="workspaceId">The Id of a workspace.</param>
        /// <returns>Collection of RTE Templates</returns>
        ICollection<RteCustomTemplateModelView> GetTemplates(int workspaceId);

        /// <summary>
        /// Searches Templates.
        /// </summary>
        /// <param name="searchTerm">Search term.</param>
        /// <returns>List of matching RTE Templates.</returns>
        ICollection<RteCustomTemplateModelView> Search(string searchTerm);

        /// <summary>
        /// Get the Questions and Answers by Boe Id.
        /// </summary>
        /// <param name="workspaceId">The Id of a workspace.</param>
        /// <param name="boeId">The id of the BOE to retrieve information for.</param>
        /// <returns>Combo of questions and answers for a BOE.</returns>
        ICollection<RTECustomTemplateQuestionAnswerModelView> GetByBoeId(int workspaceId, int boeId);

        /// <summary>
        /// Get the Questions and Answers by Boe Id.
        /// </summary>
        /// <param name="workspaceId">The Id of a workspace.</param>
        /// <param name="boeId">The id of the BOE to retrieve information for.</param>
        /// <param name="taskId">The id of the task to retrieve information for.</param>
        /// <returns>Combo of questions and answers for a BOE.</returns>
        ICollection<RTECustomTemplateQuestionAnswerModelView> GetByBoeIdAndTaskId(int workspaceId, int boeId, int? taskId);

        /// <summary>
        /// Gets the Questions and Answers by Workspace Id.
        /// </summary>
        /// <param name="workspaceId">The Id of a workspace.</param>
        /// <param name="boes">The full boes with tasks to retrieve questions and answers against.</param>
        /// <returns>Combo of questions and answers for a BOE.</returns>
        ICollection<RTECustomTemplateQuestionAnswerModelView> GetByWorkspaceId(int workspaceId, ICollection<FullBoe> boes);

        /// <summary>
        /// Save the answers for RTE questions.
        /// </summary>
        /// <param name="answers">The answers to save.</param>
        void SaveAnswers(ICollection<RTECustomTemplateQuestionAnswerModelView> answers);

        /// <summary>
        /// Saves the questions for an updated rte template
        /// </summary>
        /// <param name="questions">The questions to save.</param>
        /// <param name="templateId">The template ID for the questions.</param>
        /// <returns>Mapping of old and new IDs for the questions</returns>
        Dictionary<int, int> SaveQuestions(ICollection<RteCustomTemplateQuestionModelView> questions, int templateId);
    }
}

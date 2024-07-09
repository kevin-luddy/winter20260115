// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
    using System.Collections.Generic;
	using GenBOE.Dtos;
	using IES.Common.Exceptions;
	using GenBOE.ActionLogic.ModelView;

	/// <summary>
	/// Controller logic interface for the base controller logic class
	/// </summary>
	public interface IGenBOEControllerLogic
    {
        /// <summary>
        /// For any properties marked as rich-text, remove styling and/or markup that is incompatible with saving.
        /// </summary>
        /// <param name="obj">Object to be saved</param>
        /// <returns>List of validation messages, if any</returns>
        /// <seealso cref="IES.Common.RichTextAttribute"/>
        ICollection<ValidationMessage> ScrubRichTextPropertiesForSave(object obj);

        /// <summary>
        /// Scrub rich-text markup to remove unwanted items and convert image tag src-attribute route values to base-64.
        /// </summary>
        /// <param name="html">Rich-text markup</param>
        /// <param name="validationMessages">Repository for validation messages</param>
        /// <returns>Scrubbed rich-text</returns>
        string ScrubRichTextForPaste(string html, ICollection<ValidationMessage> validationMessages);

        /// <summary>
        /// Determines if either the Escalation Rates or Fees/Cost Rates are out of date.
        /// </summary>
        /// <param name="workspaceId">The id of the workspace to check.</param>
        /// <returns></returns>
        bool AreZoneTravelRatesOutOfDate(int workspaceId);

        /// <summary>
        /// Determines if the Offload Rates are out of date.
        /// </summary>
        /// <param name="workspaceId">The id of the workspace to check.</param>
        /// <returns></returns>
        bool AreOffloadRatesOutOfDate(int workspaceId);

        /// <summary>
        /// Validates the RTE Answers
        /// </summary>
        /// <param name="answers">The answers to validate.</param>
        /// <param name="sources">The sources for RTE Templates.</param>
        /// <param name="rteSizeLimit">The RTE Size limit for the workspace if overridden.</param>
        /// <returns>Validation warnings.</returns>
        ICollection<ValidationMessage> ValidateRteAnswers(ICollection<RTECustomTemplateQuestionAnswerModelView> answers, ICollection<RteCustomTemplateSourceModelView> sources, int? rteSizeLimit);

		/// <summary>
		/// Validate the Skill Mix Table for any errors
		/// </summary>
		/// <param name="skillMixTable">The Skill Mix table, as a model view</param>
		/// <returns>A collection of any validation errors/messages</returns>
		ICollection<ValidationMessage> ValidateSkillMixTable(ICollection<SkillMixModelView> skillMixTable);
	}
}

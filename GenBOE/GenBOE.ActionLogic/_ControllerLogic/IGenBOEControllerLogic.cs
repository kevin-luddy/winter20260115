// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
    using System.Collections.Generic;
    using IES.Common.Exceptions;

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
    }
}

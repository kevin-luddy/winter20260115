// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.ControllerLogic
{
    using System.Collections.Generic;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.DataBridge.ModelViews;

    /// <summary>
    /// Interface for the File Attachment Controller Logic.
    /// </summary>
    public interface IFileAttachmentControllerLogic : IRdmControllerLogic
    {
        /// <summary>
        /// Validate File Attachment Grid Data
        /// </summary>
        /// <param name="fileAttachments">Collection of File Attachment rows</param>
        /// <param name="sections">The sections for the current revision.</param>
        /// <returns>A list of validation errors (if any).</returns>
        ICollection<ValidationMessage> ValidateFileAttachments(ICollection<FileAttachmentRowModelView> fileAttachments, ICollection<OptionModelView> sections);
    }
}

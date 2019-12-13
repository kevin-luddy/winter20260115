// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;
    using Mediator;

    /// <summary>
    /// Logic for the File Attachment Controller.
    /// </summary>
    public class FileAttachmentControllerLogic : RdmControllerLogic, IFileAttachmentControllerLogic
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileAttachmentControllerLogic"/> class.
        /// </summary>
        /// <param name="revisionMediator">Revision Mediator</param>
        /// <param name="areaLockingLoader">Area Locking Loader</param>
        /// <param name="adUtils">AD Utilities</param>
        /// <param name="securityInfo">Security Information</param>
        public FileAttachmentControllerLogic(IAreaLockingLoader areaLockingLoader, IRevisionMediator revisionMediator, IActiveDirectoryUtilities adUtils, ISecurityInformation securityInfo)
            : base(areaLockingLoader, revisionMediator, adUtils, securityInfo)
        {
        }

        /// <summary>
        /// Validate File Attachment Grid Data
        /// </summary>
        /// <param name="fileAttachments">Collection of File Attachment rows</param>
        /// <param name="sections">The sections for this revision.</param>
        /// <returns>A list of validation errors (if any).</returns>
        public ICollection<ValidationMessage> ValidateFileAttachments(ICollection<FileAttachmentRowModelView> fileAttachments, ICollection<OptionModelView> sections)
        {
            if (fileAttachments == null)
            {
                throw new ArgumentNullException(nameof(fileAttachments));
            }

            if (sections == null)
            {
                throw new ArgumentNullException(nameof(sections));
            }
            
            Collection<ValidationMessage> validationErrors = new Collection<ValidationMessage>();

            if (fileAttachments.Any(c => !c.IsDeleted && string.IsNullOrWhiteSpace(c.Name)))
            {
                validationErrors.Add(new ValidationMessage("One or more rows are missing the required Name field."));
            }

            if (fileAttachments.Any(c => !c.IsDeleted && string.IsNullOrWhiteSpace(c.Link)))
            {
                validationErrors.Add(new ValidationMessage("One or more rows are missing the required Url field."));
            }

            if (fileAttachments.Any(c => !c.IsDeleted && !string.IsNullOrEmpty(c.Link) && c.Link.Length > 255))
            {
                validationErrors.Add(new ValidationMessage("One or more rows has a Url field over 255 characters."));
            }

            if (fileAttachments.Any(c => !c.IsDeleted && !string.IsNullOrEmpty(c.Name) && c.Name.Length > 100))
            {
                validationErrors.Add(new ValidationMessage("One or more rows has a Name field over 100 characters."));
            }

            foreach (FileAttachmentRowModelView row in fileAttachments.Where(f => f.IsDeleted))
            {
                row.Updateable = UpdateType.Deleted;
            }

            foreach (FileAttachmentRowModelView row in fileAttachments.Where(f => !f.IsDeleted))
            {
                row.Updateable = UpdateType.Upsert;

                if (row.SectionId.HasValue && row.SectionId.Value > 0)
                {
                    OptionModelView foundSection = sections.FirstOrDefault(s => s.Id == row.SectionId);
                    if (foundSection == null)
                    {
                        validationErrors.Add(new ValidationMessage($"Row with Name {row.Name ?? string.Empty} has a bad or old section selected."));
                    }
                }

                if (!string.IsNullOrWhiteSpace(row.Link))
                {
                    Uri result;
                    if (!Uri.TryCreate(row.Link, UriKind.Absolute, out result))
                    {
                        validationErrors.Add(new ValidationMessage($"Row has an invalid Url: {row.Link}"));
                    }
                }
            }

            return validationErrors;
        }
    }
}

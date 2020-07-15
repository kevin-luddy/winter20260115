// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.DataBridge.Loaders;
    using IES.DataBridge.ModelViews;
    using Mediator;

    /// <summary>
    /// PPRD Controller Logic
    /// </summary>
    public class PPRDControllerLogic : RdmControllerLogic, IPPRDControllerLogic
    {
        /// <summary>
        /// The file attachment loader
        /// </summary>
        private readonly IFileAttachmentLoader fileAttachmentLoader;

        /// <summary>
        /// Initializes a new instance of the <see cref="PPRDControllerLogic"/> class.
        /// </summary>
        /// <param name="fileAttachmentLoader">The file attachment loader.</param>
        /// <param name="revisionMediator">Revision Mediator</param>
        /// <param name="areaLockingLoader">Area Locking Loader</param>
        /// <param name="adUtils">AD Utilities</param>
        /// <param name="securityInfo">Security Information</param>
        public PPRDControllerLogic(IFileAttachmentLoader fileAttachmentLoader,
            IAreaLockingLoader areaLockingLoader, IRevisionMediator revisionMediator, IActiveDirectoryUtilities adUtils, ISecurityInformation securityInfo)
            : base(areaLockingLoader, revisionMediator, adUtils, securityInfo)
        {
            this.fileAttachmentLoader = fileAttachmentLoader;
        }

        /// <summary>
        /// Validate that the section Ids passed in can be deleted from the WIP revision.
        /// </summary>
        /// <param name="sectionIds">Section Ids to validate against.</param>
        /// <param name="validationErrors">List of errors found.</param>
        /// <param name="sections">Sections to validate.</param>
        /// <param name="wipRevision">The work in progress Revision.</param>
        public void ValidateDeletionSections(ICollection<int> sectionIds, ICollection<ValidationMessage> validationErrors, ICollection<OptionModelView> sections,
            RevisionModelView wipRevision)
        {
            if (sectionIds == null)
            {
                throw new ArgumentNullException(nameof(sectionIds));
            }

            if (validationErrors == null)
            {
                throw new ArgumentNullException(nameof(validationErrors));
            }

            if (sections == null)
            {
                throw new ArgumentNullException(nameof(sections));
            }

            if (wipRevision == null)
            {
                throw new ArgumentNullException(nameof(wipRevision));
            }

            ICollection<FileAttachmentRowModelView> fileAttachments = this.fileAttachmentLoader.GetByRevision(wipRevision.Id);

            foreach (int sectionId in sectionIds)
            {
                // only validate existing sections
                if (sectionId >= 0)
                {
                    OptionModelView section = sections.FirstOrDefault(s => s.Id == sectionId);
                    if (section == null)
                    {
                        validationErrors.Add(new ValidationMessage($"No section was found with this Id: {sectionId}, please refresh the page to get up to date data."));
                    }
                    else if (fileAttachments.Any(f => f.SectionId == sectionId))
                    {
                        validationErrors.Add(new ValidationMessage($"Cannot delete because a File Attachment was found for Section {section.Label}"));
                    }
                }
            }
        }

        /// <summary>
        /// Validate that the number of tables is correct for each section and the document. 
        /// Also check to see if any content nodes have child nodes. 
        /// Make sure all section content types are valid.
        /// </summary>
        /// <param name="sections">Sections to validate</param>
        /// <param name="validationErrors">List of errors found.</param>
        public void ValidateSections(ICollection<SectionModelView> sections, ICollection<ValidationMessage> validationErrors)
        {
            if (sections == null)
            {
                throw new ArgumentNullException(nameof(sections));
            }

            if (validationErrors == null)
            {
                throw new ArgumentNullException(nameof(validationErrors));
            }

            foreach (SectionModelView section in sections)
            {
                this.ValidateSection(section, validationErrors);
            }
        }

        /// <summary>
        /// Recursively validate the section and its children.
        /// </summary>
        /// <param name="section">The section node to check.</param>
        /// <param name="validationErrors">List of errors found.</param>
        private void ValidateSection(SectionModelView section, ICollection<ValidationMessage> validationErrors)
        {
            if (section.ContentType == SectionContentType.Section)
            {
                if (string.IsNullOrEmpty(section.Title))
                {
                    validationErrors.Add(new ValidationMessage($"Section {section.ReferenceNumber} - Is missing required Title."));
                }

                int numTablesInSection = 0;
                foreach (SectionModelView child in section.ChildNodes)
                {
                    if (child.ContentType == SectionContentType.RateTable)
                    {
                        numTablesInSection++;
                    }

                    this.ValidateSection(child, validationErrors);  // recursively validate children
                }

                if (numTablesInSection > 1)
                {
                    validationErrors.Add(new ValidationMessage($"Section {section.ReferenceNumber} - Only 1 table is allowed per section."));
                }
            }
            else if (section.IsSectionContent())
            {
                // validate text and table elements
                if (section.ChildNodes.Count > 0)
                {
                    validationErrors.Add(new ValidationMessage("Text or table nodes may not have children."));
                }
            }
            else
            {
                validationErrors.Add(new ValidationMessage("Invalid Content type detected."));
            }
        }
    }
}
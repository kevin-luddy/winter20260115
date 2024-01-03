// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.ControllerLogic
{
    using System.Collections.Generic;
    using IES.Core;
    using IES.Core.Exceptions;
    using IES.DataBridge.ModelViews;

    /// <summary>
    /// Interface for the PPRD Controller Logic.
    /// </summary>
    public interface IPPRDControllerLogic : IRdmControllerLogic
    {
        /// <summary>
        /// Validate that the number of tables is correct for each section and the document. 
        /// Also check to see if any content nodes have child nodes. 
        /// Make sure all section content types are valid.
        /// </summary>
        /// <param name="sections">Sections to validate</param>
        /// <param name="validationErrors">List of errors found.</param>
        void ValidateSections(ICollection<SectionModelView> sections, ICollection<ValidationMessage> validationErrors);

        /// <summary>
        /// Validate that the section Ids passed in can be deleted from the WIP revision.
        /// </summary>
        /// <param name="sectionIds">Section Ids to validate against.</param>
        /// <param name="validationErrors">List of errors found.</param>
        /// <param name="sections">Sections to validate.</param>
        /// <param name="wipRevision">The work in progress Revision.</param>
        void ValidateDeletionSections(ICollection<int> sectionIds, ICollection<ValidationMessage> validationErrors, ICollection<OptionModelView> sections,
            RevisionModelView wipRevision);
    }
}

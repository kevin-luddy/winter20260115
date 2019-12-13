// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.Loaders
{
    using System.Collections.Generic;
    using IES.Common;
    using IES.DataBridge.ModelViews;

    /// <summary>
    /// Interface for Section Loader
    /// </summary>
    public interface ISectionLoader : IDataLoader<SectionModelView>
    {
        /// <summary>
        /// Get all sections by revision.
        /// </summary>
        /// <param name="revision">Revision to retrieve the sections for.</param>
        /// <param name="sectionsOnly">bool noting if only Sections with Content Types of Section should be returned</param>
        /// <param name="sectionIds">A list of section ids to only return if set.</param>
        /// <param name="refNumberPrefix">A prefix for the Ref Numbers.</param>
        /// <returns>Return the specified top level sections for this revision including all child nodes.</returns>
        ICollection<SectionModelView> GetAll(RevisionModelView revision, bool sectionsOnly = false, ICollection<int> sectionIds = null, string refNumberPrefix = "");

        /// <summary>
        /// Retrieve all sections for the specified revision
        /// </summary>
        /// <param name="revision">WIP Revision</param>
        /// <param name="sectionsOnly">bool noting if only Sections with Content Types of Section should be returned</param>
        /// <returns>Returns all sections for the revision</returns>
        ICollection<SectionModelView> RetrieveAllSections(RevisionModelView revision, bool sectionsOnly = false);

        /// <summary>
        /// Retrieves the section names as OptionModelView objects.
        /// </summary>
        /// <param name="revision">WIP Revision</param>
        /// <returns>All of the sections.</returns>
        ICollection<OptionModelView> RetrieveSectionsAsOptions(RevisionModelView revision);

        /// <summary>
        /// Update the Sections within a document.
        /// </summary>
        /// <param name="revision">Revision</param>
        /// <param name="sectionSet">sections to update</param>
        void UpdateSectionsAndContent(RevisionModelView revision, ICollection<SectionModelView> sectionSet);
    }
}
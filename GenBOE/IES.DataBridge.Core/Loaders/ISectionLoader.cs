// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.Loaders
{
	using System.Collections.Generic;
	using IES.Common.Core.Loaders;
	using IES.Common.Core.Models;
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

		/// <summary>
		/// Gets data necessary for automation of a coversheet. Specifically sections that contain 1) CASB, 2) Non-Compliance data, and 3) Disclosure Statements
		/// </summary>
		/// <param name="proposalId">PTM Proposal ID</param>
		/// <returns>Data to support a Cover Sheet creation</returns>
		RDSBCoverSheetDataModelView GetCoverSheetData(int proposalId);

		/// <summary>
		/// Get all addresses, regardless if a section is a parent or not
		/// </summary>
		/// <param name="ptmTrackingId">The PTM Tracking #/Proposal ID</param>
		/// <returns>A collection of addresses, complete with a section title</returns>
		ICollection<SectionAddressModelView> GetAddresses(int ptmTrackingId);

		/// <summary>
		/// Get a flat dictionary of Section IDs and Names for the given Revision ID
		/// </summary>
		/// <param name="revisionId">Revision ID</param>
		/// <returns>flat dictionary of Section IDs and Names</returns>
		Dictionary<int, string> GetFlatSectionIdsAndNamesByRevisionId(int revisionId);
	}
}
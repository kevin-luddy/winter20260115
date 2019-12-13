// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.Loaders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using IES.Common;
    using IES.DataBridge.ModelViews;
    using IES.Models;

    /// <summary>
    /// Section Loader
    /// </summary>
    public class SectionLoader : DataLoader<SectionModelView>, ISectionLoader
    {
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SectionLoader"/> class.
        /// </summary>
        public SectionLoader()
        {
            this.Log = new Logger(typeof(SectionLoader));
        }

        #endregion

        #region Retrieves
        /// <summary>
        /// Gets all objects based on the IDs that were passed in
        /// </summary>
        /// <param name="ids">IDs</param>
        /// <returns>
        /// Corresponding Data
        /// </returns>
        public override ICollection<SectionModelView> GetByIds(ICollection<int> ids)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get all sections by revision.
        /// </summary>
        /// <param name="revision">Revision to retrieve the sections for.</param>
        /// <param name="sectionsOnly">bool noting if only Sections with Content Types of Section should be returned</param>
        /// <param name="sectionIds">A list of section ids to only return if set.</param>
        /// <param name="refNumberPrefix">A prefix for the Ref Numbers.</param>
        /// <returns>Return the specified top level sections for this revision including all child nodes.</returns>
        public ICollection<SectionModelView> GetAll(RevisionModelView revision, bool sectionsOnly = false, ICollection<int> sectionIds = null, string refNumberPrefix = "")
        {
            ICollection<SectionModelView> sectionDetailsByRevision = null;
            ICollection<SectionModelView> sectionsToReturn = new List<SectionModelView>();

            using (IESEntities context = new IESEntities())
            {
                ICollection<Section> sectionsForRevision = context.Sections.Where(x => (x.RevisionID == revision.Id)).ToCollection();
                if (sectionsOnly)
                {
                    // Filter out Sections that don't have a Content Type of Section
                    sectionsForRevision = sectionsForRevision
                        .Where(x => x.SectionContentTypeID == (int) SectionContentType.Section).ToCollection();
                }

                if (sectionIds != null)
                {
                    int sectionType = (int)SectionContentType.Section;
                    sectionsForRevision = sectionsForRevision.Where(x => x.SectionContentTypeID != sectionType || sectionIds.Contains(x.ID)).ToCollection();
                }

                sectionDetailsByRevision = sectionsForRevision.Select(r => new SectionModelView()
                {
                    Id = r.ID,
                    UpdateDate = r.UpdateDate,
                    RevisionId = r.RevisionID,
                    ParentId = r.ParentID,
                    DisplayOrder = r.DisplayOrder,
                    Title = r.Title,
                    TextContent = r.TextContent,
                    ContentType = (SectionContentType) r.SectionContentTypeID,
                    IsInternalSection = r.IsInternalSection,
                    DisplayRateCode = r.DisplayRateCode,
                    RevisionUniqueSectionId = r.RevisionUniqueSectionId,
                    IsRdsbRequired = r.IsRdsbRequired
                }).ToList();
            }

            foreach (SectionModelView topLevelSection in sectionDetailsByRevision.Where(x => (x.ParentId == null)))
            {
                sectionsToReturn.Add(this.GetBySectionId(topLevelSection.Id, sectionDetailsByRevision));
            }

            sectionsToReturn = this.SetReferenceNumbers(sectionsToReturn, null, refNumberPrefix);

            return sectionsToReturn;
        }

        /// <summary>
        /// Get by section id.
        /// </summary>
        /// <param name="id">Id to retrieve.</param>
        /// <param name="sectionDetailsForRevision">Contains list of sections for a revision.</param>
        /// <returns>All Sections for the id.</returns>
        private SectionModelView GetBySectionId(int id, ICollection<SectionModelView> sectionDetailsForRevision)
        {
            SectionModelView parentSection = null;

            // Parent node to return
            parentSection = sectionDetailsForRevision.First(s => s.Id == id);

            // Sub sections and content
            ICollection<SectionModelView> sectionDetails = sectionDetailsForRevision.Where(x => (x.ParentId == id))
                .OrderBy(y => y.DisplayOrder).ToList();

            if (sectionDetails.Any())
            {
                foreach (SectionModelView subSection in sectionDetails)
                {
                    switch (subSection.ContentType)
                    {
                        case SectionContentType.Section:
                            SectionModelView subNode = this.GetBySectionId(subSection.Id, sectionDetailsForRevision);
                            if (subNode != null)
                            {
                                parentSection.ChildNodes.Add(subNode);
                            }

                            break;
                        case SectionContentType.RateTable:
                        case SectionContentType.Text:
                            parentSection.ChildNodes.Add(subSection);
                            break;
                    }
                }
            }

            return parentSection;
        }
        #endregion

        #region SectionMethods

        /// <summary>
        /// Retrieve all sections for the specified revision
        /// </summary>
        /// <param name="revision">WIP Revision</param>
        /// <param name="sectionsOnly">bool noting if only Sections with Content Types of Section should be returned</param>
        /// <returns>Returns all sections for the revision</returns>
        public ICollection<SectionModelView> RetrieveAllSections(RevisionModelView revision, bool sectionsOnly = false)
        {
            ICollection<SectionModelView> allSections = this.GetAll(revision, sectionsOnly);
            allSections = this.SetReferenceNumbers(allSections, null);
            return allSections;
        }

        /// <summary>
        /// Retrieves the section names as OptionModelView objects.
        /// </summary>
        /// <param name="revision">WIP Revision</param>
        /// <returns>All of the sections.</returns>
        public ICollection<OptionModelView> RetrieveSectionsAsOptions(RevisionModelView revision)
        {
            ICollection<SectionModelView> allSections = this.RetrieveAllSections(revision);
            ICollection<OptionModelView> sections = new List<OptionModelView>();
            sections.Add(new OptionModelView { Id = 0, Label = string.Empty });
            this.RetrieveAllChildSections(allSections, sections);
            return sections;
        }

        /// <summary>
        /// Recursively get section names
        /// </summary>
        /// <param name="sectionNodes">Set of nodes to process</param>
        /// <param name="sections">Set of section names found so far</param>
        private void RetrieveAllChildSections(ICollection<SectionModelView> sectionNodes, ICollection<OptionModelView> sections)
        {
            foreach (SectionModelView section in sectionNodes)
            {
                if (section.ContentType == SectionContentType.Section)
                {
                    OptionModelView option = new OptionModelView()
                    {
                        Id = section.Id,
                        Label = string.Format("{0} - {1}", section.ReferenceNumber, section.Title)
                    };
                    sections.Add(option);
                    this.RetrieveAllChildSections(section.ChildNodes, sections);
                }
            }
        }

        /// <summary>
        /// Sets the reference numbers recursively.
        /// </summary>
        /// <param name="sections">The sections needing Reference numbers.</param>
        /// <param name="parent">The parent section.</param>
        /// <param name="refNumberPrefix">The prefix for the Reference Numbers.</param>
        /// <returns>renumbered section hierarchy in proper order</returns>
        private ICollection<SectionModelView> SetReferenceNumbers(ICollection<SectionModelView> sections, SectionModelView parent, string refNumberPrefix = "")
        {
            // order by DisplayOrder so that reference numbers can be created correctly
            sections = sections.OrderBy(x => x.DisplayOrder).ToCollection();

            int counter = 1;
            foreach (SectionModelView section in sections)
            {
                if (section.ContentType == SectionContentType.Section)
                {
                    section.ReferenceNumber = parent == null
                        ? string.Format("{1}{0}", counter++, refNumberPrefix)
                        : string.Format("{0}.{1}", parent.ReferenceNumber, counter++);
                }

                section.ChildNodes = this.SetReferenceNumbers(section.ChildNodes, section);
            }

            return sections;
        }
        #endregion

        #region Commits

        /// <summary>
        /// SaveAll will save everything in the section.
        /// </summary>
        /// <param name="revision">Revision to save.</param>
        /// <param name="section">Section to save.</param>
        /// <returns>Returns section id for top level node.</returns>
        private int SaveAll(RevisionModelView revision, SectionModelView section)
        {
            int? id = null;

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                id = this.SaveAll(revision, section, null);
            }

            return id.Value;
        }

        /// <summary>
        /// SaveAll will save everything in the section.
        /// </summary>
        /// <param name="revision">Revision to save.</param>
        /// <param name="section">Section to save.</param>
        /// <param name="parentId">The parent section Id.</param>
        /// <returns>Returns the top level section id.</returns>
        private int SaveAll(RevisionModelView revision, SectionModelView section, int? parentId)
        {
            int? newSectionId = 0;

            if (section != null)
            {
                // Parent node
                section.ParentId = parentId;
                section.Updateable = UpdateType.Upsert;
                section.RevisionId = revision.Id;
                newSectionId = this.Save(section);

                // check old Id value (set by UpdateSectionsAndContent method) to see if we need to re-map rate codes to the new section Id.
                if (section.OldId.HasValue && section.OldUpdateDate.HasValue && newSectionId.HasValue)
                {
                    // call Stored Procedure to re-map any section references (i.e. Rate Codes, File Attachments, etc.) from oldId to newId
                    this.RemapSectionReferences(section.OldId.Value, section.OldUpdateDate.Value, newSectionId.Value);
                }

                foreach (SectionModelView childNode in section.ChildNodes)
                {
                    this.SaveAll(revision, childNode, newSectionId);
                }
            }

            if (newSectionId != null)
            {
                return newSectionId.Value;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Re-map any section references (i.e. Rate Codes, File Attachments, etc.) for old section Id to point to the new section Id.
        /// </summary>
        /// <param name="oldId">Old section Id</param>
        /// <param name="oldUpdateDate">Old section UpdateDate</param>
        /// <param name="newId">New section Id</param>
        private void RemapSectionReferences(int oldId, DateTime oldUpdateDate, int newId)
        {
            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (IESEntities iesEntities = new IESEntities())
                {
                    iesEntities.remapSectionReferences(oldId, oldUpdateDate, newId);
                }
            }
        }

        /// <summary>
        /// Upsert
        /// </summary>
        /// <param name="dtoToUpsert">Dto that is upserted</param>
        /// <returns>Id of the dto after the modification</returns>
        protected override int? Upsert(SectionModelView dtoToUpsert)
        {
            if (dtoToUpsert == null)
            {
                throw new ArgumentNullException(nameof(dtoToUpsert));
            }

            int? result = null;

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (IESEntities iesEntities = new IESEntities())
                {
                    result = iesEntities.upsertSection(dtoToUpsert.Id, dtoToUpsert.UpdateDate, dtoToUpsert.RevisionId,
                        dtoToUpsert.ParentId, dtoToUpsert.DisplayOrder, dtoToUpsert.Title,
                        dtoToUpsert.TextContent, (int)dtoToUpsert.ContentType, dtoToUpsert.IsInternalSection,
                        dtoToUpsert.DisplayRateCode, dtoToUpsert.RevisionUniqueSectionId, dtoToUpsert.IsRdsbRequired).First();
                }
            }

            return result;
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="dtoToDelete">Dto that is deleted</param>
        /// <returns>Id of the deleted dto</returns>
        protected override int? Delete(SectionModelView dtoToDelete)
        {
            int? toReturn = null;

            if (dtoToDelete != null)
            {
                foreach (SectionModelView s in dtoToDelete.ChildNodes)
                {
                    if (dtoToDelete.ContentType == SectionContentType.Section)
                    {
                        toReturn = this.Delete(s);
                    }
                }

                using (IESEntities iesEntities = new IESEntities())
                {
                    toReturn = iesEntities.deleteSection(dtoToDelete.Id, dtoToDelete.UpdateDate);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Update the Sections within a document.
        /// </summary>
        /// <param name="revision">Revision</param>
        /// <param name="sectionSet">sections to update</param>
        public void UpdateSectionsAndContent(RevisionModelView revision, ICollection<SectionModelView> sectionSet)
        {
            if (revision == null)
            {
                throw new ArgumentNullException(nameof(revision));
            }

            if (sectionSet == null)
            {
                throw new ArgumentNullException(nameof(sectionSet));
            }

            // parent container for current top-level section data from DB
            SectionModelView parent = new SectionModelView
            {
                ChildNodes = this.GetAll(revision)
            };

            // Notes:
            // We need to make sure any section references (i.e. rate codes or file attachments) get mapped properly.
            // So if a section gets moved, we need to re-map references from the old sectionId to the new sectionId.
            ICollection<SectionModelView> sectionsToDelete = new List<SectionModelView>();  // keep a list of sections to be deleted after making updates.
            this.UpdateSectionsAndContent(parent, sectionSet, sectionsToDelete);
            this.SetReferenceNumbers(sectionSet, null);
            foreach (SectionModelView section in parent.ChildNodes)
            {
                this.SaveAll(revision, section);
            }

            foreach (SectionModelView sectionToDelete in sectionsToDelete)
            {
                this.Delete(sectionToDelete);  // delete the section and children
            }
        }

        /// <summary>
        /// Recursively update the Sections and content.
        /// </summary>
        /// <param name="parent">parent section</param>
        /// <param name="sectionSet">sections to update</param>
        /// <param name="sectionsToDelete">sections to delete</param>
        private void UpdateSectionsAndContent(SectionModelView parent, ICollection<SectionModelView> sectionSet, ICollection<SectionModelView> sectionsToDelete)
        {
            // Processing Existing Sections first to ensure sorted list of Sections doesn't prevent adding new Section to the Document
            // due to reorder of the sections.
            ICollection<SectionModelView> existSections = sectionSet.Where(x => x.Id > 0).ToCollection();
            ICollection<SectionModelView> newSections = sectionSet.Where(x => x.Id < 0).ToCollection();  // New Sections won't have Database Identifier yet
            ICollection<SectionModelView> dbSections = parent.ChildNodes.ToCollection();

            // Remove any Sections from Database if they are no longer in the Document/Section
            // (i.e. sections that were moved or deleted)
            foreach (SectionModelView dbSection in dbSections)
            {
                if (existSections.All(x => x.Id != dbSection.Id))
                {
                    sectionsToDelete.Add(dbSection);    // Add to list of sections to be deleted later (after re-mapping section references)
                    parent.ChildNodes.Remove(dbSection);
                }
            }

            // Process Existing Sections - i.e. sections that were moved (1) or section attribute changes (2) as follows:
            //  1) Section was moved (original section will be deleted, new section will be inserted,
            //     and rate code & file attachment references need to be re-mapped to new section). Or
            //  2) Section attribute changes, e.g. text, title, etc.
            foreach (SectionModelView section in existSections)
            {
                SectionModelView s = dbSections.FirstOrDefault(x => x.Id == section.Id);
                if (s == null)
                {
                    s = new SectionModelView
                    {
                        Id = -1,    // Force upsert to insert new row
                        // section was moved - keep old section Id and UpdateDate for eventual re-map in SaveAll method
                        OldId = section.Id,
                        OldUpdateDate = section.UpdateDate,
                        DisplayOrder = section.DisplayOrder
                    };

                    parent.ChildNodes.Add(s);
                }

                // update all properties in section
                s.ContentType = section.ContentType;
                s.DisplayOrder = section.DisplayOrder;
                s.DisplayRateCode = section.DisplayRateCode;
                s.IsInternalSection = section.IsInternalSection;
                s.RevisionId = section.RevisionId;
                s.TextContent = section.TextContent;
                s.Title = section.Title;
                s.RevisionUniqueSectionId = section.RevisionUniqueSectionId;
                s.IsRdsbRequired = section.IsRdsbRequired;
                this.UpdateSectionsAndContent(s, section.ChildNodes, sectionsToDelete);
            }

            // Process New Sections (i.e. new sections added by the user)
            foreach (SectionModelView section in newSections)
            {
                SectionModelView s = new SectionModelView(section.RevisionId, section.DisplayOrder, section.IsInternalSection,
                    section.Title, section.TextContent, section.DisplayRateCode, section.ContentType, section.ReferenceNumber,
                    section.RevisionUniqueSectionId);
                s.Id = -1; // Force upsert to insert new row
                parent.ChildNodes.Add(s);
                this.UpdateSectionsAndContent(s, section.ChildNodes, sectionsToDelete);
            }
        }
        #endregion
    }
}
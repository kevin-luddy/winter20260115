// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
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
    /// Loader for RDSB Documents.
    /// </summary>
    /// <seealso cref="IES.Common.DataLoader{IES.DataBridge.ModelViews.DocumentGridModelView}" />
    /// <seealso cref="IES.DataBridge.Loaders.IDocumentLoader" />
    public class DocumentLoader : DataLoader<DocumentGridModelView>, IDocumentLoader
    {
        /// <summary>
        /// The rev loader
        /// </summary>
        private IRevisionLoader revisionLoader;

        /// <summary>
        /// Gets all objects based on the IDs that were passed in
        /// </summary>
        /// <param name="ids">IDs</param>
        /// <returns>
        /// Corresponding Data
        /// </returns>
        public override ICollection<DocumentGridModelView> GetByIds(ICollection<int> ids)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentLoader"/> class.
        /// </summary>
        /// <param name="revisionLoader">The revision loader.</param>
        public DocumentLoader(IRevisionLoader revisionLoader)
        {
            this.revisionLoader = revisionLoader;
        }

        /// <summary>
        /// Gets the by proposal ids.
        /// </summary>
        /// <param name="proposalIds">The proposal ids.</param>
        /// <param name="latestRevisionId">The latest revision Id.</param>
        /// <returns>A collection of document model views.</returns>
        public ICollection<DocumentGridModelView> GetByProposalIds(ICollection<int> proposalIds, int latestRevisionId)
        {
            ICollection<DocumentGridModelView> documents = null;

            using (IESEntities context = new IESEntities())
            {
                // DatePublished should always be set since a Document cannot be linked to the WIP
                documents = context.RDSBDocumentInformations.Where(x => proposalIds.Contains(x.PTMProposalID))
                    .Select(r =>
                        new DocumentGridModelView()
                        {
                            Id = r.ID,
                            UpdateDate = r.LastUpdateDT,
                            RDMRevisionId = r.RDMRevisionID,
                            PPRDVersion = r.Revision.Revision1,
                            PPRDVersionDate = r.Revision.DatePublished.Value, // This will fail (on purpose) if linked to WIP
                            DocumentCreated = r.CreatedDate,
                            DocumentCreatedBy = r.CreatedBy,
                            ProposalId = r.PTMProposalID,
                            StartYear = r.StartYear ?? 0,
                            EndYear = r.EndYear ?? 0,
                            IsUsingLatest = latestRevisionId == r.RDMRevisionID

                            // proposal status/title/tracking number are added later when merged with ProposalDto
                        }).ToList();
            }

            return documents;
        }

        /// <summary>
        /// Upserts the specified dto to upsert.
        /// </summary>
        /// <param name="dtoToUpsert">The dto to upsert.</param>
        /// <returns>d of the dto after the modification</returns>
        protected override int? Upsert(DocumentGridModelView dtoToUpsert)
        {
            if (dtoToUpsert == null)
            {
                throw new ArgumentNullException(nameof(dtoToUpsert));
            }

            if (dtoToUpsert.ProposalId <= 0)
            {
                throw new ArgumentException("ProposalId is not set.");
            }

            if (dtoToUpsert.Id <= 0)
            {
                dtoToUpsert.RDMRevisionId = this.GetLatestPublishedRevisionId();
            }

            int? result;

            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (IESEntities iesEntities = new IESEntities())
                {
                    result = iesEntities.upsertRDSBDocumentInformation(dtoToUpsert.Id, dtoToUpsert.UpdateDate, dtoToUpsert.ProposalId, dtoToUpsert.DocumentCreatedBy, dtoToUpsert.RDMRevisionId, dtoToUpsert.StartYear, dtoToUpsert.EndYear, string.Empty).First();
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the latest published revision identifier.
        /// </summary>
        /// <returns>The id of the latest published Revision.</returns>
        private int GetLatestPublishedRevisionId()
        {
            RevisionModelView revision = this.revisionLoader.GetAll().Where(r => r.DatePublished.HasValue).OrderByDescending(r => r.DatePublished).First();

            return revision.Id;
        }

        /// <summary>
        /// Delete method to be overridden by the derived class.
        /// </summary>
        /// <param name="dtoToDelete">Dto that will be deleted.</param>
        /// <returns>
        /// Id of the deleted object
        /// </returns>
        protected override int? Delete(DocumentGridModelView dtoToDelete)
        {
            int? toReturn = null;

            if (dtoToDelete != null)
            {
                using (IESEntities iesEntities = new IESEntities())
                {
                    toReturn = iesEntities.deleteRDSBDocumentInformation(dtoToDelete.Id, dtoToDelete.UpdateDate);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Check if a record exists for the given Proposal ID
        /// </summary>
        /// <param name="proposalId">PTM Proposal ID</param>
        /// <returns>true if record exists, otherwise false</returns>
        public bool DoesRecordExist(int proposalId)
        {
            bool recordExists = false;

            using (IESEntities context = new IESEntities())
            {
                recordExists = context.RDSBDocumentInformations.Any(x => x.PTMProposalID == proposalId);
            }

            return recordExists;
        }
    }
}

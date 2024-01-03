// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.Loaders
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using IES.Core;
    using IES.DataBridge.ModelViews;
    using IES.Models;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// Loader for the Document Details
	/// </summary>
	public class DocumentDetailLoader : DataLoader<DocumentDetailModelView>, IDocumentDetailLoader
    {
        /// <summary>
        /// RDSB Rate Code Xref Loader
        /// </summary>
        private IRdsbRateCodeXrefLoader rdsbRateCodeXrefLoader;

        /// <summary>
        /// RDSB Section Xref Loader
        /// </summary>
        private IRdsbSectionXrefLoader rdsbSectionXrefLoader;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="rdsbRateCodeXrefLoader">RDSB Rate Code Xref Loader</param>
        /// <param name="rdsbSectionXrefLoader">RDSB Section Xref Loader</param>
        public DocumentDetailLoader(IRdsbRateCodeXrefLoader rdsbRateCodeXrefLoader, IRdsbSectionXrefLoader rdsbSectionXrefLoader,
            ILogger<DocumentDetailLoader> logger) : base(logger)
        {
            this.rdsbRateCodeXrefLoader = rdsbRateCodeXrefLoader;
            this.rdsbSectionXrefLoader = rdsbSectionXrefLoader;
        }

        /// <summary>
        /// Gets all objects based on the IDs that were passed in
        /// </summary>
        /// <param name="ids">IDs</param>
        /// <returns>
        /// Corresponding Data
        /// </returns>
        public override ICollection<DocumentDetailModelView> GetByIds(ICollection<int> ids)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets Document Detail MV by the proposal id
        /// </summary>
        /// <param name="proposalId">proposal id</param>
        /// <returns>Document Detail MV</returns>
        public DocumentDetailModelView GetByProposalId(int proposalId)
        {
            DocumentDetailModelView toReturn = null;

            using (IESEntities context = new IESEntities())
            {
                {
                    // ToCollection doesn't work in the linq statement, so the result needs to be stored in an anonymous type and then converted to the MV 
                    var result = (from d in context.RDSBDocumentInformations
                                  join s in context.RDSBSectionXrefs on d.ID equals s.RDSBDocumentInformationID into sectionGroup
                                  from ds in sectionGroup.DefaultIfEmpty()
                                  join r in context.RDSBRateCodeXrefs on d.ID equals r.RDSBDocumentInformationID into rateCodeGroup
                                  from dsr in rateCodeGroup.DefaultIfEmpty()
                                  where d.PTMProposalID == proposalId
                                  select new 
                                  {
                                      Id = d.ID,
                                      UpdateDate = d.LastUpdateDT,
                                      ProposalId = d.PTMProposalID,
                                      DocumentCreatedBy = d.CreatedBy,
                                      SelectedRevisionId = d.RDMRevisionID,
                                      StartYear = d.StartYear ?? 0,
                                      EndYear = d.EndYear ?? 0,
                                      SelectedRateCodeIds = d.RDSBRateCodeXrefs.Select(x => x.RateCodeID),
                                      SelectedSectionIds = d.RDSBSectionXrefs.Select(x => x.SectionID),
                                      ParentSection = d.ParentSection
                                  }).FirstOrDefault();

                    if (result != null)
                    {
                        toReturn = new DocumentDetailModelView()
                        {
                            Id = result.Id,
                            UpdateDate = result.UpdateDate,
                            ProposalId = result.ProposalId,
                            DocumentCreatedBy = result.DocumentCreatedBy,
                            SelectedRevisionId = result.SelectedRevisionId,
                            StartYear = result.StartYear,
                            EndYear = result.EndYear,
                            SelectedRateCodeIds = result.SelectedRateCodeIds.ToCollection(),
                            SelectedSectionIds = result.SelectedSectionIds.ToCollection(),
                            ParentSection = result.ParentSection
                        };
                    }
                }                
            }

            return toReturn;
        }

        /// <summary>
        /// Upsert Document Details and Xrefs
        /// </summary>
        /// <param name="dtoToUpsert">DTO to upsert</param>
        /// <returns>ID of upserted DTO</returns>
        protected override int? Upsert(DocumentDetailModelView dtoToUpsert)
        {
            if (dtoToUpsert == null)
            {
                throw new ArgumentNullException(nameof(dtoToUpsert));
            }

            int? result;

            // Upsert to RDSBDocumentInformation
            using (StopwatchTimer sw = new StopwatchTimer(this.Log))
            {
                using (IESEntities context = new IESEntities())
                {
                    result = context.upsertRDSBDocumentInformation(dtoToUpsert.Id, dtoToUpsert.UpdateDate, dtoToUpsert.ProposalId, dtoToUpsert.DocumentCreatedBy, dtoToUpsert.SelectedRevisionId, dtoToUpsert.StartYear, dtoToUpsert.EndYear, dtoToUpsert.ParentSection).First();
                }
            }

            // Bulk insert rate code and section xrefs
            if (result != null)
            {
                int id = -1;
                ICollection<RdsbRateCodeXrefModelView> rdsbRateCodeXrefs = new Collection<RdsbRateCodeXrefModelView>();
                foreach (int rateCodeId in dtoToUpsert.SelectedRateCodeIds)
                {
                    rdsbRateCodeXrefs.Add(new RdsbRateCodeXrefModelView()
                    {
                        Id = id--,
                        RateCodeId = rateCodeId,
                        RdsbDocumentInformationId = (int)result,
                        Updateable = UpdateType.Upsert
                    });
                }

                if(rdsbRateCodeXrefs.Any())
                {
                    this.rdsbRateCodeXrefLoader.BulkSave(rdsbRateCodeXrefs);
                }

                id = -1;
                ICollection<RdsbSectionXrefModelView> rdsbSectionXrefs = new Collection<RdsbSectionXrefModelView>();
                foreach(int sectionId in dtoToUpsert.SelectedSectionIds)
                {
                    rdsbSectionXrefs.Add(new RdsbSectionXrefModelView()
                    {
                        Id = id--,
                        SectionId = sectionId,
                        RdsbDocumentInformationId = (int)result,
                        Updateable = UpdateType.Upsert
                    });
                }

                if(rdsbSectionXrefs.Any())
                {
                    this.rdsbSectionXrefLoader.BulkSave(rdsbSectionXrefs);
                }
            }

            return result;
        }

        /// <summary>
        /// Delete method - Deletes are handled by the DocumentLoader, not used here
        /// </summary>
        /// <param name="dtoToDelete">Dto that will be deleted</param>
        /// <returns>Not Implemented Exception</returns>
        protected override int? Delete(DocumentDetailModelView dtoToDelete)
        {
            throw new NotImplementedException();
        }
    }
}

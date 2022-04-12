// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using System.Linq;
    using IES.Common;
    using GenBOE.Dtos;
    using GenBOE.Models;

    /// <summary>
    /// The PBOE INL Form Data Loader Class.
    /// </summary>
    /// <seealso cref="GenBOE.DataBridge.DataLoader{GenBOE.Dtos.BOEFormPBOEDTO, GenBOE.Models.BOEFormPBOE}" />
    /// <seealso cref="GenBOE.DataBridge.DTO.IBOEFormPBOEDTODataLoader" />
    public class BOEFormPBOEDTODataLoader : DataLoader<BOEFormPBOEDTO>, IBOEFormPBOEDTODataLoader
    {
        private Logger _log = new Logger(typeof(BOEFormPBOEDTODataLoader));

        /// <summary>
        /// Gets BOE Forms for a Workspace.
        /// </summary>
        /// <param name="wsId">Workspace Id</param>
        /// <returns>BOE Forms used by Workspace.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        [DbQuery]
        public ICollection<BOEFormPBOEDTO> GetByWorkspaceId(int wsId)
        {
            List<BOEFormPBOEDTO> toReturn = new List<BOEFormPBOEDTO>();
            List<BOEFormPBOEDTO> pboes;
            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    // get the basic BOE data from the sprocResults
                    pboes = (from b in gbe.BOEFormPBOEs
                             where b.WorkspaceID == wsId
                             orderby b.PBOEFormID
                             select new BOEFormPBOEDTO
                             {
                                 Approver = b.Approver,
                                 ApproverPhone = b.ApproverPhone,
                                 BasisAndRationale = b.BasisAndRationale,
                                 FormName = b.FormName,
                                 Id = b.PBOEFormID,
                                 Poc = b.Poc,
                                 PocPhone = b.PocPhone,
                                 ProposalDate = b.ProposalDate,
                                 ProposalTitle = b.ProposalTitle,
                                 Revision = b.Revision,
                                 Version = b.FormVersion,
                                 UpdateDate = b.UpdateDT,
                                 ResourceIdsIEnum = gbe.BOEFormPBOEResourcesXREFs.Where(r => r.PBOEFormID == b.PBOEFormID).Select(r => r.ResourceID),
                                 ClinContractTypesIEnum = gbe.BOEFormPBOECLINsXREFs.Where(c => c.PBOEFormID == b.PBOEFormID)
                                        .Select(c => new BoeFormClinContractTypeDTO
                                        {
                                            ClinId = c.ClinID,
                                            ContractType = c.ContractType
                                        }),
                                 WorkspaceId = b.WorkspaceID,
                                 CCoPDApplies = ((Ccopd)b.CCoPD & Ccopd.Applies) == Ccopd.Applies,
                                 CommercialItemExceptionApplies = ((Ccopd)b.CCoPD & Ccopd.Commercial) == Ccopd.Commercial,
                                 CompetitionExceptionApplies = ((Ccopd)b.CCoPD & Ccopd.Competition) == Ccopd.Competition,
                                 OtherExceptionApplies = ((Ccopd)b.CCoPD & Ccopd.Other) == Ccopd.Other,
                                 CID = (ScheduleEvent)b.CID,
                                 CIDDate = b.CIDDate,
                                 CommercialItemDocIncluded = (TripleBooleanState)b.CommercialItemDocIncluded,
                                 CostAnalysis = (ScheduleEvent)b.CostAnalysis,
                                 CostAnalysisDate = b.CostAnalysisDate,
                                 CostAnalysisIncluded = (TripleBooleanState)b.CostAnalysisIncluded,
                                 Description = b.Description,
                                 FactFinding = (ScheduleEvent)b.FactFinding,
                                 FactFindingDate = b.FactFindingDate,
                                 FirmSupplierReceipt = (ScheduleEvent)b.FirmSupplierReceipt,
                                 FirmSupplierReceiptDate = b.FirmSupplierReceiptDate,
                                 GovtPricing = (ScheduleEvent)b.GovtPricing,
                                 GovtPricingDate = b.GovtPricingDate,
                                 GovtReview = (ScheduleEvent)b.GovtReview,
                                 GovtReviewDate = b.GovtReviewDate,
                                 MOU = (ScheduleEvent)b.MOU,
                                 MOUDate = b.MOUDate,
                                 OtherText = b.CCoPDOtherText,
                                 PlannedDate_WrittenApproval = b.PlannedDate_WrittenApproval,
                                 PlannedDate_ApprovedSubmission = b.PlannedDate_ApprovedSubmission,
                                 PriceAnalysis = (ScheduleEvent)b.PriceAnalysis,
                                 PriceAnalysisDate = b.PriceAnalysisDate,
                                 PriceAnalysisIncluded = (TripleBooleanState)b.PriceAnalysisIncluded,
                                 Procurement = (ScheduleEvent)b.Procurement,
                                 ProcurementDate = b.ProcurementDate,
                                 ProposalNumber = b.ProposalNumber,
                                 RFP = b.RFP,
                                 RFPRelease = (ScheduleEvent)b.RFPRelease,
                                 RFPReleaseDate = b.RFPReleaseDate,
                                 ShouldCostEstimate = (ScheduleEvent)b.ShouldCostEstimate,
                                 ShouldCostEstimateDate = b.ShouldCostEstimateDate,
                                 SourceSelection = (ScheduleEvent)b.SourceSelection,
                                 SourceSelectionDate = b.SourceSelectionDate,
                                 SowWritten = (ScheduleEvent)b.SowWritten,
                                 SowWrittenDate = b.SowWrittenDate,
                                 SupplierName = b.SupplierName,
                                 SupplierNegotiations = (ScheduleEvent)b.SupplierNegotiations,
                                 SupplierNegotiationsDate = b.SupplierNegotiationsDate,
                                 SupplierProposalSupportingDataIncluded = (TripleBooleanState)b.SupplierProposalSupportingDataIncluded,
                                 TechnicalEvaluation = (ScheduleEvent)b.TechnicalEvaluation,
                                 TechnicalEvaluationDate = b.TechnicalEvaluationDate,
                                 ValidityDate = b.ValidityDate,
                                 CIDText = b.CIDText,
                                 CostAnalysisText = b.CostAnalysisText,
                                 FactFindingText = b.FactFindingText,
                                 GovtPricingText = b.GovtPricingText,
                                 GovtReviewText = b.GovtReviewText,
                                 MOUText = b.MOUText,
                                 PriceAnalysisText = b.PriceAnalysisText,
                                 ProcurementText = b.ProcurementText,
                                 SupplierNegotiationsText = b.SupplierNegotiationsText,
                                 TechnicalEvaluationText = b.TechnicalEvaluationText,
                                 ShouldCostEstimateText = b.ShouldCostEstimateText,
                                 SowWrittenText = b.SowWrittenText,
                                 RFPReleaseText = b.RFPReleaseText,
                                 FirmSupplierReceiptText = b.FirmSupplierReceiptText,
                                 SourceSelectionText = b.SourceSelectionText
                             }).ToList();
                }
                foreach (BOEFormPBOEDTO dto in pboes)
                {
                    dto.ResourceIds = dto.ResourceIdsIEnum.ToList();
                    dto.ResourceIdsIEnum = null;

                    dto.ClinContractTypes = dto.ClinContractTypesIEnum.ToList();
                    dto.ClinContractTypesIEnum = null;

                    toReturn.Add(dto);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Retrieves the latest version number of the form.
        /// </summary>
        /// <returns>Latest version number of the form.</returns>
        public int GetCurrentFormVersion()
        {
            return 1;
        }

        /// <summary>
        /// Returns a collection of PBOE BOE Form DTOs based on the Collection of Ids.
        /// </summary>
        /// <param name="ids">Capture Ids.</param>
        /// <returns>The matching DTOs.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        [DbQuery]
        public override ICollection<BOEFormPBOEDTO> GetByIds(ICollection<int> ids)
        {
            List<BOEFormPBOEDTO> toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    // get the basic BOE data from the sprocResults
                    toReturn = (from b in gbe.BOEFormPBOEs
                                where ids.Contains(b.PBOEFormID)
                                orderby b.PBOEFormID
                                select new BOEFormPBOEDTO
                                {
                                    Approver = b.Approver,
                                    ApproverPhone = b.ApproverPhone,
                                    BasisAndRationale = b.BasisAndRationale,
                                    FormName = b.FormName,
                                    Id = b.PBOEFormID,
                                    Poc = b.Poc,
                                    PocPhone = b.PocPhone,
                                    ProposalDate = b.ProposalDate,
                                    ProposalTitle = b.ProposalTitle,
                                    Revision = b.Revision,
                                    Version = b.FormVersion,
                                    UpdateDate = b.UpdateDT,
                                    ResourceIdsIEnum = gbe.BOEFormPBOEResourcesXREFs.Where(r => r.PBOEFormID == b.PBOEFormID).Select(r => r.ResourceID),
                                    ClinContractTypesIEnum = gbe.BOEFormPBOECLINsXREFs.Where(c => c.PBOEFormID == b.PBOEFormID)
                                        .Select(c => new BoeFormClinContractTypeDTO
                                        {
                                            ClinId = c.ClinID,
                                            ContractType = c.ContractType
                                        }),
                                    WorkspaceId = b.WorkspaceID,
                                    CCoPDApplies = ((Ccopd)b.CCoPD & Ccopd.Applies) == Ccopd.Applies,    
                                    CommercialItemExceptionApplies = ((Ccopd)b.CCoPD & Ccopd.Commercial) == Ccopd.Commercial,
                                    CompetitionExceptionApplies = ((Ccopd)b.CCoPD & Ccopd.Competition) == Ccopd.Competition,
                                    OtherExceptionApplies = ((Ccopd)b.CCoPD & Ccopd.Other) == Ccopd.Other,
                                    CID = (ScheduleEvent)b.CID,
                                    CIDDate = b.CIDDate,
                                    CommercialItemDocIncluded = (TripleBooleanState)b.CommercialItemDocIncluded,
                                    CostAnalysis = (ScheduleEvent)b.CostAnalysis,
                                    CostAnalysisDate = b.CostAnalysisDate,
                                    CostAnalysisIncluded = (TripleBooleanState)b.CostAnalysisIncluded,
                                    Description = b.Description,
                                    FactFinding = (ScheduleEvent)b.FactFinding,
                                    FactFindingDate = b.FactFindingDate,
                                    FirmSupplierReceipt = (ScheduleEvent)b.FirmSupplierReceipt,
                                    FirmSupplierReceiptDate = b.FirmSupplierReceiptDate,
                                    GovtPricing = (ScheduleEvent)b.GovtPricing,
                                    GovtPricingDate = b.GovtPricingDate,
                                    GovtReview = (ScheduleEvent)b.GovtReview,
                                    GovtReviewDate = b.GovtReviewDate,
                                    MOU = (ScheduleEvent)b.MOU,
                                    MOUDate = b.MOUDate,
                                    OtherText = b.CCoPDOtherText,
                                    PlannedDate_WrittenApproval = b.PlannedDate_WrittenApproval,
                                    PlannedDate_ApprovedSubmission = b.PlannedDate_ApprovedSubmission,
                                    PriceAnalysis = (ScheduleEvent)b.PriceAnalysis,
                                    PriceAnalysisDate = b.PriceAnalysisDate,
                                    PriceAnalysisIncluded = (TripleBooleanState)b.PriceAnalysisIncluded,
                                    Procurement = (ScheduleEvent)b.Procurement,
                                    ProcurementDate = b.ProcurementDate,
                                    ProposalNumber = b.ProposalNumber,
                                    RFP = b.RFP,
                                    RFPRelease = (ScheduleEvent)b.RFPRelease,
                                    RFPReleaseDate = b.RFPReleaseDate,
                                    ShouldCostEstimate = (ScheduleEvent)b.ShouldCostEstimate,
                                    ShouldCostEstimateDate = b.ShouldCostEstimateDate,
                                    SourceSelection = (ScheduleEvent)b.SourceSelection,
                                    SourceSelectionDate = b.SourceSelectionDate,
                                    SowWritten = (ScheduleEvent)b.SowWritten,
                                    SowWrittenDate = b.SowWrittenDate,
                                    SupplierName = b.SupplierName,
                                    SupplierNegotiations = (ScheduleEvent)b.SupplierNegotiations,
                                    SupplierNegotiationsDate = b.SupplierNegotiationsDate,
                                    SupplierProposalSupportingDataIncluded = (TripleBooleanState)b.SupplierProposalSupportingDataIncluded,
                                    TechnicalEvaluation = (ScheduleEvent)b.TechnicalEvaluation,
                                    TechnicalEvaluationDate = b.TechnicalEvaluationDate,
                                    ValidityDate = b.ValidityDate,
                                    CIDText = b.CIDText,
                                    CostAnalysisText = b.CostAnalysisText,
                                    FactFindingText = b.FactFindingText,
                                    GovtPricingText = b.GovtPricingText,
                                    GovtReviewText = b.GovtReviewText,
                                    MOUText = b.MOUText,
                                    PriceAnalysisText = b.PriceAnalysisText,
                                    ProcurementText = b.ProcurementText,
                                    SupplierNegotiationsText = b.SupplierNegotiationsText,
                                    TechnicalEvaluationText = b.TechnicalEvaluationText,
                                    ShouldCostEstimateText = b.ShouldCostEstimateText,
                                    SowWrittenText = b.SowWrittenText,
                                    RFPReleaseText = b.RFPReleaseText,
                                    FirmSupplierReceiptText = b.FirmSupplierReceiptText,
                                    SourceSelectionText = b.SourceSelectionText
                                }).ToList();
                }

                // post processing of resourceIDs
                foreach (BOEFormPBOEDTO pboe in toReturn)
                {
                    pboe.ResourceIds = pboe.ResourceIdsIEnum.ToList();
                    pboe.ResourceIdsIEnum = null;

                    pboe.ClinContractTypes = pboe.ClinContractTypesIEnum.ToList();
                    pboe.ClinContractTypesIEnum = null;
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Upserts a PBOE BOE Form.
        /// </summary>
        /// <param name="dtoToUpsert">Dto to upsert.</param>
        /// <returns>Id of the saved capture.</returns>
        protected override int? Upsert(BOEFormPBOEDTO dtoToUpsert)
        {
            using (StopwatchTimer sw = new StopwatchTimer(Log))
            {
                int? toReturn = null;

                if (dtoToUpsert != null)
                {

                    // assemble CSV lists of ids for resources
                    string resourceList = null;
                    if (dtoToUpsert.ResourceIds != null && dtoToUpsert.ResourceIds.Any())
                    {
                        resourceList = string.Join(",", dtoToUpsert.ResourceIds.Select(i => i.ToString()));
                    }

                    // assemble CSV lists of Clin Contract Types
                    string clinList = string.Empty;
                    if (dtoToUpsert.ClinContractTypes != null && dtoToUpsert.ClinContractTypes.Any())
                    {
                        clinList = string.Join(",", dtoToUpsert.ClinContractTypes.Where(c => c.ContractType > 0).Select(c => c.ClinId.ToString() + ":" + ((int)c.ContractType).ToString()));
                    }

                    Ccopd ccopdValue = CreateCCOPDEnum(dtoToUpsert);

                    using (GenBoeEntities gbm = new GenBoeEntities())
                    {
                        toReturn = gbm.upsertBOEFormPBOE(
                                    dtoToUpsert.Id,
                                    dtoToUpsert.UpdateDate,
                                    dtoToUpsert.WorkspaceId,
                                    dtoToUpsert.FormName,
                                    dtoToUpsert.Description,
                                    dtoToUpsert.BasisAndRationale,
                                    dtoToUpsert.ProposalTitle,
                                    dtoToUpsert.ProposalDate,
                                    dtoToUpsert.Poc,
                                    dtoToUpsert.PocPhone,
                                    dtoToUpsert.Approver,
                                    dtoToUpsert.ApproverPhone,
                                    dtoToUpsert.Version,
                                    (int)ccopdValue,
                                    dtoToUpsert.OtherText,
                                    dtoToUpsert.RFP,
                                    dtoToUpsert.ProposalNumber,
                                    dtoToUpsert.SupplierName,
                                    dtoToUpsert.ValidityDate,
                                    (int)dtoToUpsert.SupplierProposalSupportingDataIncluded,
                                    (int)dtoToUpsert.PriceAnalysisIncluded,
                                    (int)dtoToUpsert.CommercialItemDocIncluded,
                                    (int)dtoToUpsert.CostAnalysisIncluded,
                                    (int)dtoToUpsert.ShouldCostEstimate,
                                    dtoToUpsert.ShouldCostEstimateDate,
                                    (int)dtoToUpsert.SowWritten,
                                    dtoToUpsert.SowWrittenDate,
                                    (int)dtoToUpsert.RFPRelease,
                                    dtoToUpsert.RFPReleaseDate,
                                    (int)dtoToUpsert.FirmSupplierReceipt,
                                    dtoToUpsert.FirmSupplierReceiptDate,
                                    (int)dtoToUpsert.SourceSelection,
                                    dtoToUpsert.SourceSelectionDate,
                                    (int)dtoToUpsert.CID,
                                    dtoToUpsert.CIDDate,
                                    (int)dtoToUpsert.GovtReview,
                                    dtoToUpsert.GovtReviewDate,
                                    (int)dtoToUpsert.PriceAnalysis,
                                    dtoToUpsert.PriceAnalysisDate,
                                    (int)dtoToUpsert.TechnicalEvaluation,
                                    dtoToUpsert.TechnicalEvaluationDate,
                                    (int)dtoToUpsert.FactFinding,
                                    dtoToUpsert.FactFindingDate,
                                    (int)dtoToUpsert.CostAnalysis,
                                    dtoToUpsert.CostAnalysisDate,
                                    (int)dtoToUpsert.GovtPricing,
                                    dtoToUpsert.GovtPricingDate,
                                    (int)dtoToUpsert.SupplierNegotiations,
                                    dtoToUpsert.SupplierNegotiationsDate,
                                    (int)dtoToUpsert.MOU,
                                    dtoToUpsert.MOUDate,
                                    (int)dtoToUpsert.Procurement,
                                    dtoToUpsert.ProcurementDate,
                                    dtoToUpsert.PlannedDate_WrittenApproval,
                                    dtoToUpsert.PlannedDate_ApprovedSubmission,
                                    resourceList,
                                    clinList,
                                    dtoToUpsert.Revision,
                                    dtoToUpsert.CIDText,
                                    dtoToUpsert.GovtReviewText,
                                    dtoToUpsert.PriceAnalysisText,
                                    dtoToUpsert.TechnicalEvaluationText,
                                    dtoToUpsert.FactFindingText,
                                    dtoToUpsert.CostAnalysisText,
                                    dtoToUpsert.GovtPricingText,
                                    dtoToUpsert.SupplierNegotiationsText,
                                    dtoToUpsert.MOUText,
                                    dtoToUpsert.ProcurementText,
                                    dtoToUpsert.ShouldCostEstimateText,
                                    dtoToUpsert.SowWrittenText,
                                    dtoToUpsert.RFPReleaseText,
                                    dtoToUpsert.FirmSupplierReceiptText,
                                    dtoToUpsert.SourceSelectionText).FirstOrDefault().Value;

                        BOEFormPBOE pboeEntity;
                        if ((pboeEntity = gbm.BOEFormPBOEs.FirstOrDefault(b => b.PBOEFormID == dtoToUpsert.Id)) != null)
                        {
                            dtoToUpsert.UpdateDate = pboeEntity.UpdateDT;  // refresh the update-date to match the DB value
                        }
                    }
                }

                return toReturn;
            }
        }

        /// <summary>
        /// Creates the ccopd flags enum from the DTO.
        /// </summary>
        /// <param name="dtoToUpsert">The dto to upsert.</param>
        /// <returns>A Ccopd Enum.</returns>
        private static Ccopd CreateCCOPDEnum(BOEFormPBOEDTO dtoToUpsert)
        {
            Ccopd ccopdValue = new Ccopd(); // initializes it to none
            if (dtoToUpsert.CCoPDApplies)
            {
                ccopdValue = Ccopd.Applies;
            }

            if (dtoToUpsert.CommercialItemExceptionApplies)
            {
                ccopdValue = ccopdValue | Ccopd.Commercial;
            }

            if (dtoToUpsert.CompetitionExceptionApplies)
            {
                ccopdValue = ccopdValue | Ccopd.Competition;
            }

            if (dtoToUpsert.OtherExceptionApplies)
            {
                ccopdValue = ccopdValue | Ccopd.Other;
            }

            return ccopdValue;
        }

        /// <summary>
        /// Deletes a PBOE BOE Form.
        /// </summary>
        /// <param name="dtoToDelete">Capture to delete.</param>
        /// <returns>Id of the deleted item.</returns>
        protected override int? Delete(BOEFormPBOEDTO dtoToDelete)
        {
            int? toReturn = null;

            if (dtoToDelete != null)
            {
                using (StopwatchTimer sw = new StopwatchTimer(this.Log))
                {
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        gbe.deleteBOEFormPBOE(dtoToDelete.Id, dtoToDelete.UpdateDate);
                    }
                }

                toReturn = dtoToDelete.Id;
            }

            return toReturn;
        }

        /// <summary>
        /// Gets a list of currently used resources inside a workspace, excluding the boeForm passed in.
        /// </summary>
        /// <param name="wsId">Workspace Id.</param>
        /// <param name="boeFormId">The boe Form Id to not include when finding the in-use resources.</param>
        /// <returns>A list of resource ids used in a workspace.</returns>
        [DbQuery]
        public ICollection<int> CurrentlyUsedResources(int wsId, int boeFormId)
        {
            List<int> toReturn = new List<int>();
            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    // get the basic BOE data from the sprocResults
                    toReturn = (from x in gbe.BOEFormPBOEResourcesXREFs
                                join b in gbe.BOEFormPBOEs on x.PBOEFormID equals b.PBOEFormID
                                where b.WorkspaceID == wsId && b.PBOEFormID != boeFormId
                                select x.ResourceID).Distinct().ToList();
                }
            }
            return toReturn;
        }
    }
}

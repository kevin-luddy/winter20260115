// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using GenBOE.ActionLogic;
    using GenBOE.ActionLogic.Common.Calculations;
    using GenBOE.ActionLogic.IO.Export;
    using GenBOE.ActionLogic.ModelView.BOE;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.Common.PickList;

    /// <summary>
    /// Action Logic for the BOEFormController
    /// </summary>
    public class BOEFormControllerLogic : IBOEFormControllerLogic
    {
        private IBOEFormIBOEDTODataLoader iboeFormDataLoader;
        private IBOEFormPBOEDTODataLoader pboeFormDataLoader;
        private IResourceDTODataLoader resourceLoader;
        private ITMResourceRateDTODataLoader tmResourceRateLoader;

        private IBOEFormExporter iboeExporter;
        private PBOEFormExporter pboeExporter;
        private TMCalculator tmCalculator;

        public BOEFormControllerLogic(
            IBOEFormIBOEDTODataLoader iboeFormDataLoader,
            IBOEFormPBOEDTODataLoader pboeFormDataLoader,
            IResourceDTODataLoader resourceLoader,
            ITMResourceRateDTODataLoader tmResourceRateLoader,
            IBOEFormExporter iboeExporter,
            PBOEFormExporter pboeExporter,
            TMCalculator tmCalculator)
        {
            this.iboeFormDataLoader = iboeFormDataLoader;
            this.pboeFormDataLoader = pboeFormDataLoader;
            this.resourceLoader = resourceLoader;
            this.tmResourceRateLoader = tmResourceRateLoader;
            this.iboeExporter = iboeExporter;
            this.pboeExporter = pboeExporter;
            this.tmCalculator = tmCalculator;
        }

        /// <summary>
        /// Exports a BOE Form based on its Id
        /// </summary>
        /// <param name="workspace">The workspace for the BOE Form.</param>
        /// <param name="boeFormId">BOE Form Id</param>
        /// <param name="boeFormType">BOE Form Type that you wish to export</param>
        /// <param name="isPortionMarkingEnabled">True if portion marking is enabled; False otherwise.</param>
        /// <param name="contractTypes">The contract types.</param>
        /// <returns>Returns the path of the actual file, and the suggested filename.</returns>
        public string[] ExportBOEFormReport(FullWorkspace workspace, int boeFormId, BOEFormType boeFormType, bool isPortionMarkingEnabled, ICollection<PickListDto> contractTypes)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (boeFormId == 0)
            {
                throw new ArgumentNullException(nameof(boeFormId));
            }

            if (boeFormType == BOEFormType.NotSet)
            {
                throw new ArgumentNullException(nameof(boeFormType));
            }

            string exportedFileName = null;
            string fileName = null;
            string proposalTitleAndRfpNumber = this.GetProposalTitleAndRfpNumber(workspace);

            if (boeFormType == BOEFormType.IBOE)
            {
                BOEFormIBOEDTO dto = this.iboeFormDataLoader.GetById(boeFormId);
                fileName = "IBOE_" + dto.FormName + ".docx";
                exportedFileName = this.iboeExporter.ExportToWordFile(workspace, fileName, dto, isPortionMarkingEnabled, proposalTitleAndRfpNumber, contractTypes);
            }
            else if (boeFormType == BOEFormType.PBOE)
            {
                BOEFormPBOEDTO dto = this.pboeFormDataLoader.GetById(boeFormId);
                fileName = "PBOE_" + dto.FormName + ".docx";
                exportedFileName = this.pboeExporter.ExportToWordFile(workspace, fileName, dto, isPortionMarkingEnabled, proposalTitleAndRfpNumber, contractTypes);
            }

            return new string[] { exportedFileName, fileName };
        }

        /// <summary>
        /// Saves the IBOE Form.
        /// </summary>
        /// <param name="modelview">ModelView of the form.</param>
        /// <param name="workspaceId">The workspace Id.</param>
        public void SaveBOEFormIBOE(BOEFormIBOEModelView modelview, int workspaceId)
        {
            if (ReferenceEquals(modelview, null))
            {
                throw new ArgumentNullException(nameof(modelview));
            }

            BOEFormIBOEDTO dto = this.ConvertModelViewToDto(modelview, workspaceId);
            dto.Updateable = UpdateType.Upsert;
            this.iboeFormDataLoader.Save(dto);
        }

        /// <summary>
        /// Saves the PBOE Form.
        /// </summary>
        /// <param name="modelview">ModelView of the form.</param>
        /// <param name="workspaceId">The workspace Id.</param>
        public void SaveBOEFormPBOE(BOEFormPBOEModelView modelview, int workspaceId)
        {
            if (ReferenceEquals(modelview, null))
            {
                throw new ArgumentNullException(nameof(modelview));
            }

            BOEFormPBOEDTO dto = this.ConvertModelViewToDto(modelview, workspaceId); 
            dto.Updateable = UpdateType.Upsert;
            this.pboeFormDataLoader.Save(dto);
        }

        /// <summary>
        /// Deletes the IBOE form.
        /// </summary>
        /// <param name="boeFormId">The id of the form to delete.</param>
        public void DeleteBOEFormIBOE(int boeFormId)
        {
            BOEFormIBOEDTO dto = this.iboeFormDataLoader.GetById(boeFormId);
            dto.Updateable = UpdateType.Deleted;
            this.iboeFormDataLoader.Save(dto);
        }

        /// <summary>
        /// Deletes the PBOE form.
        /// </summary>
        /// <param name="boeFormId">The id of the form to delete.</param>
        public void DeleteBOEFormPBOE(int boeFormId)
        {
            BOEFormPBOEDTO dto = this.pboeFormDataLoader.GetById(boeFormId);
            dto.Updateable = UpdateType.Deleted;
            this.pboeFormDataLoader.Save(dto);
        }

        /// <summary>
        /// Retrieves a boe form.
        /// </summary>
        /// <param name="workspaceId">The id of the workspace</param>
        /// <param name="boeFormId">The id of the boe form.</param>
        /// <param name="proposalTitleAndRfpNumber">Proposal Title And RFP Number from the workspace. When present, no need to validate Proposal Title.</param>
        /// <returns>IBOE Model View for the BOEForm.</returns>
        /// <exception cref="GenValidationException">The INL Form is not linked to this workspace.</exception>
        public BOEFormIBOEModelView GetIBOEForm(int workspaceId, int boeFormId, string proposalTitleAndRfpNumber)
        {
            BOEFormIBOEDTO dto = this.iboeFormDataLoader.GetById(boeFormId);
            BOEFormIBOEModelView form = new BOEFormIBOEModelView(dto);
            
            // Validate that the form is for the workspace
            if (workspaceId != dto.WorkspaceId)
            {
                throw new GenValidationException("The INL Form is not linked to this workspace.");
            }

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            this.ValidateIBOE(dto, messages, proposalTitleAndRfpNumber);
            form.IsIncomplete = messages.Any();
            form.IncompleteMessages = messages.Select(e => e.ValidationIssue).ToList();

            return form;
        }

        /// <summary>
        /// Retrieves a boe form.
        /// </summary>
        /// <param name="workspaceId">The id of the workspace</param>
        /// <param name="boeFormId">The id of the boe form.</param>
        /// <returns>PBOE Model View for the BOE Form.</returns>
        /// <exception cref="GenValidationException">The INL Form is not linked to this workspace.</exception>
        /// <param name="proposalTitleAndRfpNumber">Proposal Title And RFP Number from the workspace. When present, no need to validate Proposal Title.</param>
        public BOEFormPBOEModelView GetPBOEForm(int workspaceId, int boeFormId, string proposalTitleAndRfpNumber)
        {
            BOEFormPBOEDTO dto = this.pboeFormDataLoader.GetById(boeFormId);
            BOEFormPBOEModelView form = new BOEFormPBOEModelView(dto);

            // Validate that the form is for the workspace
            if (workspaceId != dto.WorkspaceId)
            {
                throw new GenValidationException("The INL Form is not linked to this workspace.");
            }

            Collection<ValidationMessage> messages = new Collection<ValidationMessage>();
            this.ValidatePBOE(dto, messages, proposalTitleAndRfpNumber);
            form.IsIncomplete = messages.Any();
            form.IncompleteMessages = messages.Select(e => e.ValidationIssue).ToList();

            return form;
        }

        /// <summary>
        /// Retrieves summaries of forms by workspace
        /// </summary>
        /// <param name="workspace">A workspace to retrieve boe forms against.</param>
        /// <returns>A collection of boeforms for the workspace.</returns>
        public ICollection<BOEFormModelView> GetSummaryForms(FullWorkspace workspace)
        {
            if (ReferenceEquals(workspace, null))
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            ICollection<BOEFormIBOEDTO> iboeForms = this.iboeFormDataLoader.GetByWorkspaceId(workspace.Id);
            ICollection<BOEFormPBOEDTO> pboeForms = this.pboeFormDataLoader.GetByWorkspaceId(workspace.Id);
            Collection<BOEFormModelView> forms = new Collection<BOEFormModelView>();
            string proposalTitleAndRfpNumber = this.GetProposalTitleAndRfpNumber(workspace);

            // Validate the T&M rates for selected resources
            Collection<ValidationMessage> validationErrors = new Collection<ValidationMessage>();
            Collection<int> resourceIdsWithValidTMRates = new Collection<int>();
            this.ValidateBOEFormsTMResources(validationErrors, resourceIdsWithValidTMRates, workspace, 
                iboeForms.Select(x=>x.Id).ToCollection(), pboeForms.Select(x => x.Id).ToCollection());

            foreach (BOEFormIBOEDTO iboe in iboeForms)
            {
                Collection<ValidationMessage> iboeErrors = new Collection<ValidationMessage>();
                this.ValidateIBOE(iboe, iboeErrors, proposalTitleAndRfpNumber);
                BOEFormModelView modelView = this.ConvertSummaryDtoToModelView(iboe, workspace, resourceIdsWithValidTMRates);
                modelView.IsIncomplete = iboeErrors.Any();
                modelView.IncompleteMessages = iboeErrors.Select(e => e.ValidationIssue).ToList();
                forms.Add(modelView);
            }

            foreach (BOEFormPBOEDTO pboe in pboeForms)
            {
                Collection<ValidationMessage> pboeErrors = new Collection<ValidationMessage>();
                this.ValidatePBOE(pboe, pboeErrors, proposalTitleAndRfpNumber);
                BOEFormModelView modelView = this.ConvertSummaryDtoToModelView(pboe, workspace, resourceIdsWithValidTMRates);
                modelView.IsIncomplete = pboeErrors.Any();
                modelView.IncompleteMessages = pboeErrors.Select(e => e.ValidationIssue).ToList();
                forms.Add(modelView);
            }

            return forms;
        }

        /// <summary>
        /// Get latest/current version of form being used
        /// </summary>
        /// <param name="boeFormType"></param>
        /// <returns>Int value indicating latest/current form versio.n</returns>
        public int GetCurrentFormVersion(BOEFormType boeFormType)
        {
            if(boeFormType == BOEFormType.IBOE)
            {
                return this.iboeFormDataLoader.GetCurrentFormVersion();
            }
            if (boeFormType == BOEFormType.PBOE)
            {
                return this.pboeFormDataLoader.GetCurrentFormVersion();
            }
            throw new ArgumentException("boeFormType must be set");
        }

        /// <summary>
        /// Gets a list of in-use resources for a BOEFormType.
        /// </summary>
        /// <param name="boeFormType">The type of the boe form.</param>
        /// <param name="wsId">Workspace Id.</param>
        /// <param name="boeFormId">The boe Form Id to not include when finding the in-use resources.</param>
        /// <returns>A list of ids that are currently in-use.</returns>
        public ICollection<int> GetInUseResources(BOEFormType boeFormType, int wsId, int boeFormId)
        {
            if (boeFormType == BOEFormType.IBOE)
            {
                return this.iboeFormDataLoader.CurrentlyUsedResources(wsId, boeFormId);
            }
            if (boeFormType == BOEFormType.PBOE)
            {
                return this.pboeFormDataLoader.CurrentlyUsedResources(wsId, boeFormId);
            }
            throw new ArgumentException("boeFormType must be set");
        }


        /// <summary>
        /// Converts the summarized version of the DTO to a ModelView.
        /// </summary>
        /// <param name="dto">The DTO to convert.</param>
        /// <param name="workspace">The Workspace to use for retrieving TotalCost.</param>
        /// <param name="resourceIdsWithValidTMRates">list of Resource IDs that have valid T&amp;M rates.</param>
        /// <returns>Converted modelview from dto.</returns>
        private BOEFormModelView ConvertSummaryDtoToModelView(BOEFormDTO dto, FullWorkspace workspace, ICollection<int> resourceIdsWithValidTMRates)
        {
            decimal totalCost = 0m;
            decimal tmTotalCost = 0m;
            bool hasValidTMRates = true;

            foreach (BoeTaskElementDTO taskElement in workspace.TaskElements)
            {
                foreach (ResourceTypeDto laborTask in taskElement.taskElementLabors)
                {
                    if (laborTask.ValueSpread.HasValue && laborTask.ResourceID.HasValue && dto.ResourceIds.Contains(laborTask.ResourceID.Value))
                    {
                        if (laborTask.SpreadType == SpreadType.Cost)
                        {
                            totalCost += laborTask.ValueSpread.Value;
                        }
                        if (laborTask.SpreadType == SpreadType.Hours)
                        {
                            ResourceDTO resource = this.resourceLoader.GetById(laborTask.ResourceID.Value);
                            if (!resourceIdsWithValidTMRates.Contains(laborTask.ResourceID.Value))
                            {
                                hasValidTMRates = false;
                            }

                            if (workspace.IsUsingTM && hasValidTMRates &&
                                (resource.ElementOfCost == ElementOfCostType.IWTA ||
                                 resource.ElementOfCost == ElementOfCostType.Sub) &&
                                resource.RateType == RateType.Hours)
                            {
                                tmTotalCost += this.tmCalculator.TotalCostForTaskSpread(workspace, laborTask);
                            }
                        }
                    }
                }
            }
            return new BOEFormModelView
            {
                BOEFormId = dto.Id,
                BOEFormName = dto.FormName,
                BOEFormType = dto.BOEFormType,
                TotalCost = totalCost,
                TMCost = hasValidTMRates ? tmTotalCost : 0, // note: value will not be displayed if hasValidTMRates is false
                HasValidTMRates = hasValidTMRates
            };
        }

        /// <summary>
        /// Converts ModelView to DTO
        /// </summary>
        /// <param name="modelview">The modelview to convert.</param>
        /// <param name="workspaceId">The workspace Id.</param>
        public BOEFormIBOEDTO ConvertModelViewToDto(BOEFormIBOEModelView modelview, int workspaceId)
        {
            if (modelview == null)
            {
                throw new ArgumentNullException(nameof(modelview));
            }

            BOEFormIBOEDTO dto = modelview.BOEFormId > 0 ? this.iboeFormDataLoader.GetById(modelview.BOEFormId) : new BOEFormIBOEDTO();

            dto.Approver = modelview.Approver;
            dto.ApproverPhone = modelview.ApproverPhone;
            dto.BasisAndRationale = modelview.BasisAndRationale;
            dto.BusinessArea = modelview.BusinessArea;
            dto.ClinContractTypes = modelview.ClinContractTypes;
            dto.Description = modelview.Description;
            dto.FormName = modelview.BOEFormName;
            dto.Poc = modelview.Poc;
            dto.PocPhone = modelview.PocPhone;
            dto.ProposalDate = modelview.ProposalDate;
            dto.ProposalTitle = modelview.ProposalTitle;
            dto.ResourceIds = modelview.ResourceIds;
            dto.Revision = int.Parse(modelview.Revision);
            dto.Version = modelview.Version;
            dto.WorkspaceId = workspaceId;

            return dto;
        }

        /// <summary>
        /// Converts ModelView to DTO
        /// </summary>
        /// <param name="modelview">The modelview to convert.</param>
        /// <param name="workspaceId">The workspace Id.</param>
        public BOEFormPBOEDTO ConvertModelViewToDto(BOEFormPBOEModelView modelview, int workspaceId)
        {
            if (modelview == null)
            {
                throw new ArgumentNullException(nameof(modelview));
            }

            BOEFormPBOEDTO dto = modelview.BOEFormId > 0 ? this.pboeFormDataLoader.GetById(modelview.BOEFormId) : new BOEFormPBOEDTO();
            dto.Approver = modelview.Approver;
            dto.ApproverPhone = modelview.ApproverPhone;
            dto.BasisAndRationale = modelview.BasisAndRationale;
            dto.CCoPDApplies = modelview.CCoPDApplies;
            dto.CommercialItemExceptionApplies = modelview.CommercialItemExceptionApplies;
            dto.CompetitionExceptionApplies = modelview.CompetitionExceptionApplies;
            dto.OtherExceptionApplies = modelview.OtherExceptionApplies;
            dto.CID = modelview.CID;
            dto.CIDDate = modelview.CIDDate;
            dto.ClinContractTypes = modelview.ClinContractTypes;
            dto.CostAnalysis = modelview.CostAnalysis;
            dto.CostAnalysisDate = modelview.CostAnalysisDate;
            dto.Description = modelview.Description;
            dto.FactFinding = modelview.FactFinding;
            dto.FactFindingDate = modelview.FactFindingDate;
            dto.FirmSupplierReceipt = modelview.FirmSupplierReceipt;
            dto.FirmSupplierReceiptDate = modelview.FirmSupplierReceiptDate;
            dto.FormName = modelview.BOEFormName;
            dto.GovtPricing = modelview.GovtPricing;
            dto.GovtPricingDate = modelview.GovtPricingDate;
            dto.GovtReview = modelview.GovtReview;
            dto.GovtReviewDate = modelview.GovtReviewDate;
            dto.MOU = modelview.MOU;
            dto.MOUDate = modelview.MOUDate;
            dto.OtherText = modelview.OtherText;
            if (modelview.IsPlannedDatesRequired)
            {
                dto.PlannedDate_WrittenApproval = modelview.PlannedDate_WrittenApproval;
                dto.PlannedDate_ApprovedSubmission = modelview.PlannedDate_ApprovedSubmission;
            }
            else
            {
                dto.PlannedDate_WrittenApproval = null;
                dto.PlannedDate_ApprovedSubmission = null;
            }
            dto.Poc = modelview.Poc;
            dto.PocPhone = modelview.PocPhone;
            dto.PriceAnalysis = modelview.PriceAnalysis;
            dto.PriceAnalysisDate = modelview.PriceAnalysisDate;
            dto.Procurement = modelview.Procurement;
            dto.ProcurementDate = modelview.ProcurementDate;
            dto.ProposalDate = modelview.ProposalDate;
            dto.ProposalNumber = modelview.ProposalNumber;
            dto.ProposalTitle = modelview.ProposalTitle;
            dto.ResourceIds = modelview.ResourceIds;
            dto.Revision = int.Parse(modelview.Revision);
            dto.RFP = modelview.RFP;
            dto.RFPRelease = modelview.RFPRelease;
            dto.RFPReleaseDate = modelview.RFPReleaseDate;
            dto.ShouldCostEstimate = modelview.ShouldCostEstimate;
            dto.ShouldCostEstimateDate = modelview.ShouldCostEstimateDate;
            dto.SourceSelection = modelview.SourceSelection;
            dto.SourceSelectionDate = modelview.SourceSelectionDate;
            dto.SowWritten = modelview.SowWritten;
            dto.SowWrittenDate = modelview.SowWrittenDate;
            dto.SupplierProposalSupportingDataIncluded = modelview.SupplierProposalSupportingDataIncluded;
            dto.PriceAnalysisIncluded = modelview.PriceAnalysisIncluded;
            dto.CommercialItemDocIncluded = modelview.CommercialItemDocIncluded;
            dto.CostAnalysisIncluded = modelview.CostAnalysisIncluded;
            dto.SupplierName = modelview.SupplierName;
            dto.SupplierNegotiations = modelview.SupplierNegotiations;
            dto.SupplierNegotiationsDate = modelview.SupplierNegotiationsDate;
            dto.TechnicalEvaluation = modelview.TechnicalEvaluation;
            dto.TechnicalEvaluationDate = modelview.TechnicalEvaluationDate;
            dto.ValidityDate = modelview.ValidityDate;
            dto.Version = modelview.Version;
            dto.WorkspaceId = workspaceId;
            dto.CIDText = modelview.CIDText;
            dto.GovtReviewText = modelview.GovtReviewText;
            dto.PriceAnalysisText = modelview.PriceAnalysisText;
            dto.TechnicalEvaluationText = modelview.TechnicalEvaluationText;
            dto.FactFindingText = modelview.FactFindingText;
            dto.CostAnalysisText = modelview.CostAnalysisText;
            dto.GovtPricingText = modelview.GovtPricingText;
            dto.SupplierNegotiationsText = modelview.SupplierNegotiationsText;
            dto.MOUText = modelview.MOUText;
            dto.ProcurementText = modelview.ProcurementText;
            dto.ShouldCostEstimateText = modelview.ShouldCostEstimateText;
            dto.SowWrittenText = modelview.SowWrittenText;
            dto.RFPReleaseText = modelview.RFPReleaseText;
            dto.FirmSupplierReceiptText = modelview.FirmSupplierReceiptText;
            dto.SourceSelectionText = modelview.SourceSelectionText;

            return dto;
        }

        /// <summary>
        /// Retrieves the Proposal Title and RFP Number from the workspace.
        /// </summary>
        /// <param name="workspace">Workspace</param>
        /// <returns>A string in the form { Proposal Number } / { RFP }</returns>
        public string GetProposalTitleAndRfpNumber(FullWorkspace workspace)
        {
            if (workspace != null)
            {
                return string.IsNullOrWhiteSpace(workspace.RFPNumber) ? $"{workspace.ProposalTitle}" : $"{workspace.ProposalTitle} / {workspace.RFPNumber}";
            }

            return string.Empty;
        }

        /// <summary>
        /// Validates the specified IBOE and PBOE Form T&amp;M resources.
        /// </summary>
        /// <param name="validationErrors">output parameter - collection of error messages</param>
        /// <param name="resourceIdsWithValidTMRates">output parameter - list of Resource IDs that have valid T&amp;M rates.</param>
        /// <param name="workspace">A workspace to validate boe forms against.</param>
        /// <param name="iboeFormIds">IBOE form IDs to validate</param>
        /// <param name="pboeFormIds">PBOE form IDs to validate</param>
        public void ValidateBOEFormsTMResources(ICollection<ValidationMessage> validationErrors,
            ICollection<int> resourceIdsWithValidTMRates,  FullWorkspace workspace, ICollection<int> iboeFormIds, 
            ICollection<int> pboeFormIds)
        {
            if (ReferenceEquals(validationErrors, null))
            {
                throw new ArgumentNullException(nameof(validationErrors));
            }

            if (ReferenceEquals(resourceIdsWithValidTMRates, null))
            {
                throw new ArgumentNullException(nameof(resourceIdsWithValidTMRates));
            }

            if (ReferenceEquals(workspace, null))
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            // Validate each of the selected IBOE and PBOE forms
            if (iboeFormIds != null)
            {
                foreach (int boeFormId in iboeFormIds)
                {
                    BOEFormIBOEDTO iboe = this.iboeFormDataLoader.GetById(boeFormId);
                    this.ValidateBOEFormTMResources(validationErrors, resourceIdsWithValidTMRates, workspace, iboe.ResourceIds);
                }
            }

            if (pboeFormIds != null)
            {
                foreach (int boeFormId in pboeFormIds)
                {
                    BOEFormPBOEDTO pboe = this.pboeFormDataLoader.GetById(boeFormId);
                    this.ValidateBOEFormTMResources(validationErrors, resourceIdsWithValidTMRates, workspace, pboe.ResourceIds);
                }
            }
        }

        /// <summary>
        /// Validates the IBOE or PBOE Form T&amp;M Resources
        /// </summary>
        /// <param name="validationErrors">output parameter - collection of error messages</param>
        /// <param name="resourceIdsWithValidTMRates">output parameter - list of Resource IDs that have valid T&amp;M rates.</param>
        /// <param name="workspace">A workspace to validate boe forms against.</param>
        /// <param name="resourceIds">IBOE or PBOE Resource Ids</param>
        internal void ValidateBOEFormTMResources(ICollection<ValidationMessage> validationErrors, ICollection<int> resourceIdsWithValidTMRates, FullWorkspace workspace, ICollection<int> resourceIds)
        {
            if (ReferenceEquals(validationErrors, null))
            {
                throw new ArgumentNullException(nameof(validationErrors));
            }

            if (ReferenceEquals(resourceIdsWithValidTMRates, null))
            {
                throw new ArgumentNullException(nameof(resourceIdsWithValidTMRates));
            }

            if (ReferenceEquals(workspace, null))
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (ReferenceEquals(resourceIds, null))
            {
                throw new ArgumentNullException(nameof(resourceIds));
            }

            // Get list of T&M resources for this workspace 
            ICollection<TMResourceRateDTO> workspaceTMResourceRates = this.tmResourceRateLoader.GetByWorkspaceId(workspace.Id);

            foreach (int resourceId in resourceIds)
            {
                int startingErrorCount = validationErrors.Count;

                // Only validate Sub and IWTA resources with RateType = Hours
                ResourceDTO resource = this.resourceLoader.GetById(resourceId);
                if ((resource.ElementOfCost == ElementOfCostType.Sub || resource.ElementOfCost == ElementOfCostType.IWTA) && resource.RateType == RateType.Hours)
                {
                    // Perform the following validations:
                    // - Make sure we have rates for selected resources   
                    // - Make sure the rates all have start dates, end dates, and rate values        
                    // - Make sure the rate dates cover the period when they are used (BOEJ-2862)
                    // - Make sure the rate dates are sequential, i.e. don't overlap, or contain gaps.
                    IList<TMResourceRateDTO> tmResourceRates = workspaceTMResourceRates.Where(x => x.ResourceID == resourceId).OrderBy(x => x.StartDate).ToList();
                    if (!tmResourceRates.Any())
                    {
                        validationErrors.Add(new ValidationMessage(string.Format("There are no T&M rates for resource {0}", resource.ResourceName)));
                    }
                    else if (tmResourceRates.Any(x => !x.StartDate.HasValue || !x.EndDate.HasValue))
                    {
                        validationErrors.Add(new ValidationMessage(string.Format("One or more of the T&M rates for resource {0} are missing a start or end date.", resource.ResourceName)));
                    }
                    else
                    {
                        if (tmResourceRates.Any(x => !x.ResourceRate.HasValue))
                        {
                            validationErrors.Add(new ValidationMessage(string.Format("One or more of the T&M rates for resource {0} are not populated.", resource.ResourceName)));
                        }

                        if (this.StartAndEndDatesAreValid(workspace, tmResourceRates, resourceId))
                        {
                            bool isSequential = true;
                            DateTime? previousEndDate = null;
                            foreach (TMResourceRateDTO tmResourceRate in tmResourceRates)
                            {
                                if (previousEndDate.HasValue && isSequential && tmResourceRate.StartDate.HasValue)
                                {
                                    isSequential = this.CheckConsecutiveMonths(previousEndDate.Value, tmResourceRate.StartDate.Value);
                                }
                                previousEndDate = tmResourceRate.EndDate;
                            }

                            if (!isSequential)
                            {
                                validationErrors.Add(new ValidationMessage(string.Format("The T&M rates for resource {0} are not sequential, i.e. there are gaps or overlaps in the date ranges.", 
                                    resource.ResourceName)));
                            }
                        }
                        else
                        {
                            validationErrors.Add(new ValidationMessage(string.Format("The T&M rates for resource {0} do not cover the entire period of performance {1:MM/yyyy} - {2:MM/yyyy}.", 
                                resource.ResourceName, workspace.Boes.SelectMany(x => x.LaborTypes.Where(z => z.ResourceID == resourceId)).Min(x => x.StartDate), 
                                workspace.Boes.SelectMany(x => x.LaborTypes.Where(z => z.ResourceID == resourceId)).Max(x => x.EndDate))));
                        }
                    }
                }

                int endingErrorCount = validationErrors.Count;
                if (startingErrorCount == endingErrorCount)
                {
                    // no errors logged for this resource, so add it to the validated list
                    resourceIdsWithValidTMRates.Add(resourceId);
                }
            }
        }

        /// <summary>
        /// Validates whether start & end dates are valid
        /// </summary>
        /// <param name="workspace">Full WS</param>
        /// <param name="tmResourceRates">T&amp;M Resource Rates</param>
        /// <param name="resourceId">Resource Id (which we are validating)</param>
        /// <returns>Whether the rates are valid or not</returns>
        private bool StartAndEndDatesAreValid(FullWorkspace workspace, IList<TMResourceRateDTO> tmResourceRates, int resourceId)
        {
            DateTime? rateStartDate = tmResourceRates.First().StartDate;
            DateTime? rateEndDate = tmResourceRates.Last().EndDate;

            bool startAndEndDatesValid = rateStartDate.HasValue && rateEndDate.HasValue;

            if (startAndEndDatesValid)
            {
                // get start/end dates based on when the resource is actually used
                Collection<ResourceTypeDto> resourceUsages = workspace.Boes.SelectMany(x => x.LaborTypes.Where(z => z.ResourceID == resourceId)).ToCollection();
                if (resourceUsages.Any())
                {
                    DateTime resourceUsageStartDate = resourceUsages.Min(x => x.StartDateValue);
                    DateTime resourceUsageEndDate = resourceUsages.Max(x => x.EndDateValue);

                    startAndEndDatesValid = (rateStartDate.Value.Date <= resourceUsageStartDate.Date) && (rateEndDate.Value.Date >= resourceUsageEndDate.Date);
                }
            }

            return startAndEndDatesValid;
        }

        /// <summary>
        /// Compare two dates and return true if they represent consecutive months. 
        /// Note: This code assume the start and end dates are normalized to the 15th of the month at 12pm.
        /// </summary>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <returns>True if date2 is 1 month later than date1; False otherwise.</returns>
        private bool CheckConsecutiveMonths(DateTime date1, DateTime date2)
        {
            return DateTime.Compare(date1.AddMonths(1), date2) == 0;
        }

        /// <summary>
        /// Validates the pboe.
        /// </summary>
        /// <param name="boeForm">The BOE form.</param>
        /// <param name="validationMessages">The validation messages.</param>
        /// <param name="proposalTitleAndRfpNumber">Proposal Title And RFP Number from the workspace. When present, no need to validate Proposal Title.</param>
        public void ValidatePBOE(BOEFormPBOEDTO boeForm, ICollection<ValidationMessage> validationMessages, string proposalTitleAndRfpNumber = null)
        {
            if (boeForm == null)
            {
                throw new ArgumentNullException(nameof(boeForm));
            }

            if (validationMessages == null)
            {
                throw new ArgumentNullException(nameof(validationMessages));
            }

            if (string.IsNullOrWhiteSpace(boeForm.SupplierName))
            {
                validationMessages.Add(new ValidationMessage("Supplier Name is required."));
            }

            if (string.IsNullOrWhiteSpace(boeForm.RFP))
            {
                validationMessages.Add(new ValidationMessage("Supplier RFP Number is required."));
            }

            if (string.IsNullOrWhiteSpace(boeForm.ProposalNumber))
            {
                validationMessages.Add(new ValidationMessage("Proposal Number is required."));
            }

            if (string.IsNullOrWhiteSpace(boeForm.ProposalDate))
            {
                validationMessages.Add(new ValidationMessage("Proposal Date is required."));
            }

            if (string.IsNullOrWhiteSpace(boeForm.ValidityDate))
            {
                validationMessages.Add(new ValidationMessage("Validity Date is required."));
            }

            if (string.IsNullOrWhiteSpace(boeForm.ProposalTitle) && string.IsNullOrWhiteSpace(proposalTitleAndRfpNumber))
            {
                validationMessages.Add(new ValidationMessage("Proposal Title is required."));
            }

            if (string.IsNullOrWhiteSpace(boeForm.Description))
            {
                validationMessages.Add(new ValidationMessage("Description is required."));
            }

            if (!boeForm.CCoPDApplies && !boeForm.CommercialItemExceptionApplies &&
                !boeForm.CompetitionExceptionApplies && !boeForm.OtherExceptionApplies)
            {
                validationMessages.Add(new ValidationMessage("Certified Cost or Pricing Data (CCoPD) Applicability is required."));
            }

            if (string.IsNullOrWhiteSpace(boeForm.BasisAndRationale))
            {
                validationMessages.Add(new ValidationMessage("Basis of and Rationale is required."));
            }

            if (string.IsNullOrWhiteSpace(boeForm.Poc) || string.IsNullOrWhiteSpace(boeForm.Approver))
            {
                validationMessages.Add(new ValidationMessage("Points of Contact are required."));
            }
        }

        /// <summary>
        /// Validates the iboe.
        /// </summary>
        /// <param name="boeForm">The BOE form.</param>
        /// <param name="validationMessages">The validation messages.</param>
        /// <param name="proposalTitleAndRfpNumber">Proposal Title And RFP Number from the workspace. When present, no need to validate Proposal Title.</param>
        public void ValidateIBOE(BOEFormIBOEDTO boeForm, ICollection<ValidationMessage> validationMessages, string proposalTitleAndRfpNumber = null)
        {
            if (boeForm == null)
            {
                throw new ArgumentNullException(nameof(boeForm));
            }

            if (validationMessages == null)
            {
                throw new ArgumentNullException(nameof(validationMessages));
            }

            if (string.IsNullOrWhiteSpace(boeForm.BusinessArea))
            {
                validationMessages.Add(new ValidationMessage("Business Area is required."));
            }

            if (string.IsNullOrWhiteSpace(boeForm.ProposalDate))
            {
                validationMessages.Add(new ValidationMessage("Proposal Date is required."));
            }

            if (string.IsNullOrWhiteSpace(boeForm.ProposalTitle) && string.IsNullOrWhiteSpace(proposalTitleAndRfpNumber))
            {
                validationMessages.Add(new ValidationMessage("Proposal Title is required."));
            }

            if (string.IsNullOrWhiteSpace(boeForm.Description))
            {
                validationMessages.Add(new ValidationMessage("Description is required."));
            }

            if (string.IsNullOrWhiteSpace(boeForm.BasisAndRationale))
            {
                validationMessages.Add(new ValidationMessage("Basis of and Rationale is required."));
            }

            if (string.IsNullOrWhiteSpace(boeForm.Poc) || string.IsNullOrWhiteSpace(boeForm.Approver))
            {
                validationMessages.Add(new ValidationMessage("Points of Contact are required."));
            }
        }
    }
}
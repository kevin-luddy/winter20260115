// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Transactions;
    using System.Web.Configuration;
    using GenTRAC.ActionLogic.Mediator;
    using GenTRAC.ActionLogic.ModelView.Checklist;
    using GenTRAC.DataBridge.Common.Security;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Objects;
    using GenTRAC.Objects.FullObject;
    using IES.Common;

    /// <summary>
    /// Checklist Controller Logic
    /// </summary>
    public class ChecklistControllerLogic : GenTRACControllerLogic
    {
        /// <summary>
        /// The logger
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private IES.Common.Logger log = new IES.Common.Logger(typeof(ChecklistControllerLogic));

        /// <summary>
        /// The name of the checklist general information data form, needed for validation
        /// </summary>
        public const string CHECKLIST_GENERAL_INFO_FORM = "checklistGeneralInfoForm";

        /// <summary>
        /// the name of the checklist propsoal pricing data form, needed for validation
        /// </summary>
        public const string CHECKLIST_PROPOSAL_PRICING_DATA_FORM = "checklistProposalPricingDataForm";

        /// <summary>
        /// the name of the checklist PPR document form, needed for validation
        /// </summary>
        public const string CHECKLIST_PPR_DOCUMENT_FORM = "checklistPPRDocumentForm";

        /// <summary>
        /// the name of the checklist PAR document form, needed for validation
        /// </summary>
        public const string CHECKLIST_PAR_DOCUMENT_FORM = "checklistPARDocumentForm";

        /// <summary>
        /// The Proposal Checklist Loader.
        /// </summary>
        private IProposalChecklistLoader checklistLoader;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inSecurityAccess">Security Access</param>
        /// <param name="inProposalLoader">Proposal Loader</param>
        /// <param name="inUserMapper">User Mapper</param>
        /// <param name="objectFactory">Object Factory</param>
        /// <param name="inChecklistMediator">checklist mediator</param>
        /// <param name="inProposalMediator">proposal mediator</param>
        /// <param name="approvalsLoader">Approvals Loader</param>
        /// <param name="proposalChecklistLoader">Proposal Checklist Loader</param>
        public ChecklistControllerLogic(
            ISecurityAccess inSecurityAccess,
            IProposalLoader inProposalLoader,
            IUserMapper inUserMapper,
            IFullObjectFactory objectFactory,
            IChecklistMediator inChecklistMediator,
            IProposalMediator inProposalMediator,
            IApprovalsLoader approvalsLoader,
            IProposalChecklistLoader proposalChecklistLoader)
            : base(inSecurityAccess, inProposalLoader, inUserMapper, objectFactory, approvalsLoader, proposalChecklistLoader, inChecklistMediator, inProposalMediator)
        {
            this.checklistLoader = proposalChecklistLoader;
        }

        /// <summary>
        /// Get checklist index view for ppr or par checklist version
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>Checklist Index Model View</returns>
        public ChecklistIndexModelView GetChecklistVersion(int proposalId)
        {
            ChecklistIndexModelView model = new ChecklistIndexModelView();
            FullProposal fullProposalDto = this.GetFullProposalDto(proposalId);

            // while PPR and PAR each have a checklist version, they should be in sync
            var pprData = fullProposalDto.ProposalChecklistPPRData;
            model.Version = pprData.Version;
            model.IsReadOnly = this.IsProposalChecklistReadOnly(fullProposalDto);

            model.ShouldDisplayChecklist = !(model.Version == 0);
            model.IsPricer = this.SecurityAccess.CurrentUserHasRole(PtmRole.Pricer, proposalId);

            return model;
        }

        /// <summary>
        /// Get the full proposal DTO
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>full Proposal DTO</returns>
        private FullProposal GetFullProposalDto(int proposalId)
        {
            FullProposal fullProposal = null;

            ProposalDto proposal = this.ProposalLoader.GetById(proposalId);
            fullProposal = this.ObjectFactory.CreateFullProposal(proposal);

            return fullProposal;
        }

        /// <summary>
        /// Populate the parameters for exporting the current PAR Checklist
        /// </summary>
        /// <param name="reportParameters">PAR Checklist Document Model</param>
        /// <returns>Uri parameters to open the SSRS report in</returns>
        public Uri PopulatePARChecklistSSRSParameters(PARChecklistModelView reportParameters)
        {
            if (reportParameters == null)
            {
                throw new ArgumentNullException(nameof(reportParameters));
            }

            StringBuilder sb = new StringBuilder();

            // do not show report parameters
            sb.Append(Constants.Report.NO_REPORT_PARAMETERS);

            if (reportParameters.ProposalID > 0)
            {
                sb.Append(string.Format("&{0}={1}", Constants.Report.PROPOSAL_ID, reportParameters.ProposalID));
            }

            if (reportParameters.ChecklistVersion > 0)
            {
                sb.Append(string.Format("&{0}={1}", Constants.Report.PROPOSAL_ADEQUACY_REVIEW_ID, reportParameters.ChecklistVersion));
            }
            
            Uri toReturn = new Uri(string.Format("{0}/{1}/{2}{3}", WebConfigurationManager.AppSettings["ReportServerLocation"], WebConfigurationManager.AppSettings["ReportServerFolderName"], "PAR Checklist Report", sb));
            return toReturn;
        }

        /// <summary>
        /// Deletes all checklist data that exists for the corresponding Proposal.  Should only be used when changing the ProposalClass to Forecasted.
        /// </summary>
        /// <param name="checklist">Checklist to delete.</param>
        public void DeleteChecklist(ProposalChecklistDto checklist)
        {
            if (checklist == null)
            {
                throw new ArgumentNullException(nameof(checklist));
            }

            using (TransactionScope scope = new TransactionScope())
            {
                checklist.Updateable = IES.Common.UpdateType.Deleted;
                this.checklistLoader.Save(checklist);
                scope.Complete();
            }
        }

        /// <summary>
        /// Deletes all checklist data for a given ProposalId.  Should only be used when changing the ProposalClass to Forecasted.
        /// </summary>
        /// <param name="proposalId">The ProposalId.</param>
        public void DeleteChecklist(int? proposalId)
        {
            if (proposalId != null)
            {
                FullProposal fullProposal = this.GetFullProposalDto(proposalId.Value);

                var checklists = fullProposal.ProposalChecklistData;

                if (checklists != null && checklists.Any())
                {
                    ProposalChecklistDto checklist = fullProposal.ProposalChecklistData.FirstOrDefault();

                    if (checklist != null)
                    {
                        this.DeleteChecklist(checklist);
                    }
                }
            }
        }
    }
}

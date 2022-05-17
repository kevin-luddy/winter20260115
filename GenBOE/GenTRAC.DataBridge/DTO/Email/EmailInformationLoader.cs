// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using IES.Common;

    /// <summary>
    /// Email information dto data loader
    /// </summary>
    public class EmailInformationLoader : IEmailInformationLoader
    {
        /// <summary>
        /// Logger
        /// </summary>
        private IES.Common.Logger Log { get; set; }

        /// <summary>
        /// Gets or sets the proposal loader.
        /// </summary>
        private IProposalLoader ProposalLoader { get; set; }

        /// <summary>
        /// Gets or sets the Permission Loader.
        /// </summary>
        private IProposalPermissionLoader PermissionLoader { get; set; }

        /// <summary>
        /// Gets or sets the user loader.
        /// </summary>
        private IUserLoader UserLoader { get; set; }

        /// <summary>
        /// Gets or sets the attachment loader
        /// </summary>
        private IAttachmentLoader AttachmentLoader { get; set; }

        private static DateTime DOCUMENT_REMINDER_CUTOFF_DATE = new DateTime(2020, 1, 1);

        /// <summary>
        /// Default Constructor
        /// </summary>
        public EmailInformationLoader()
        {
            this.Log = new IES.Common.Logger(typeof(EmailInformationLoader));
            // This Loader is called from the Emailer console app, so it does not have the Unity Container loaded for resolutions, have to new up any Loaders/mappers
            this.PermissionLoader = new ProposalPermissionLoader();
            this.ProposalLoader = new ProposalLoader(this.PermissionLoader);
            this.UserLoader = new UserLoader();
            this.AttachmentLoader = new AttachmentLoader();
        }

        /// <summary>
        /// Polls the database to return all Proposals that require an email to be sent.
        /// </summary>
        /// <param name="proposalId">If set, this should get emails that are only during this approval</param>
        /// <returns>A list of Proposal information that require an email.</returns>
        [IES.Common.DbQuery]
        public ICollection<EmailInformationDto> GetAllEmailsToBeSent(int? proposalId)
        {
            ICollection<EmailInformationDto> emailList = new List<EmailInformationDto>();

            using (IES.Common.StopwatchTimer sw = new IES.Common.StopwatchTimer("EmailInformationLoader.GetAllEmailsToBeSent", this.Log))
            {
                // Get the cutoff Date
                DateTime cutoffDate = IES.Common.Utilities.GetWorkflowCutoffDate();

                if (proposalId.HasValue)
                {
                    // only retrieve locked proposals for THIS proposal
                    ProposalDto proposal = this.ProposalLoader.GetById(proposalId.Value);

                    switch (proposal.WorkflowStatus)
                    {
                        case (WorkflowStatus.Started):
                            this.ProcessApprovers(emailList, new List<ProposalDto>() { proposal }, EmailType.InitialApprovalEmail);
                            break;
                        case (WorkflowStatus.AllApproved):
                            this.ProcessLOBApprover(emailList, new List<ProposalDto>() { proposal }, EmailType.InitialLOBApprovalEmail);
                            break;
                        case (WorkflowStatus.ProposalLocked):
                            this.ProcessLOBApproved(emailList, proposal);
                            break;
                    }
                }
                else
                {
                    this.ProcessApprovers(emailList, this.ProposalLoader.GetProposalsByWorkflowStatus(WorkflowStatus.Started), EmailType.InitialApprovalEmail);
                    this.ProcessLOBApprover(emailList, this.ProposalLoader.GetProposalsByWorkflowStatus(WorkflowStatus.AllApproved), EmailType.InitialLOBApprovalEmail);

                    // only get cutoff emails if this is not for a specific approval
                    this.ProcessApprovers(emailList, this.ProposalLoader.GetProposalsByWorkflowStatusAndCutoffDate(WorkflowStatus.InitialApproverEmail, cutoffDate), EmailType.SecondApprovalEmail);
                    this.ProcessApprovers(emailList, this.ProposalLoader.GetProposalsByWorkflowStatusAndCutoffDate(WorkflowStatus.SecondApproverEmail, cutoffDate), EmailType.FinalApprovalEmail);
                    this.ProcessLeadAlertForApprover(emailList, this.ProposalLoader.GetProposalsByWorkflowStatusAndCutoffDate(WorkflowStatus.ThirdApproverEmail, cutoffDate));
                    this.ProcessLOBApprover(emailList, this.ProposalLoader.GetProposalsByWorkflowStatusAndCutoffDate(WorkflowStatus.InitialLOBLeadEmail, cutoffDate), EmailType.SecondLOBApprovalEmail);
                    this.ProcessLOBApprover(emailList, this.ProposalLoader.GetProposalsByWorkflowStatusAndCutoffDate(WorkflowStatus.SecondLOBLeadEmail, cutoffDate), EmailType.FinalLOBApprovalEmail);
                    this.ProcessLeadAlertForLOBApprover(emailList, this.ProposalLoader.GetProposalsByWorkflowStatusAndCutoffDate(WorkflowStatus.ThirdLOBLeadEmail, cutoffDate));

                    // Process Forecast Proposals that are within X number of days to today
                    DateTime forecastCutoff = DateTime.Now.AddDays(IES.Common.ConfigurationUtilities.GetAppSetting<int>("ForecastedDaysOut"));
                    this.ProcessForecastProposals(emailList, this.ProposalLoader.GetForecastProposalsPastAllowedDate(forecastCutoff));

                    // Process the Certification Timeline emails
                    this.ProcessCertificationTimeline(emailList, this.ProposalLoader.GetProposalsCertificationTimelinePastDue());

                    // Process the Mod Execution Date emails
                    this.ProcessModExecutionDateMissing(emailList, this.ProposalLoader.GetModExecutedDateMissingNotifications());
                }
            }

            return emailList;
        }

        /// <summary>
        /// Updates the DB table tracking if an email has been sent.
        /// </summary>
        /// <param name="proposalId">The Proposal Id.</param>
        /// <param name="emailType">The email type.</param>
        public void UpdateEmailSent(int proposalId, EmailType emailType)
        {
            using (IES.Common.StopwatchTimer sw = new IES.Common.StopwatchTimer("EmailInformationLoader.UpdateEmailSent", this.Log))
            {
                // get the latest version from DB
                ProposalDto proposal = this.ProposalLoader.GetById(proposalId);
                proposal.Updateable = IES.Common.UpdateType.Upsert;

                if (emailType == EmailType.CertificationTimelineEmail)
                {
                    proposal.CertificationLastEmailed = DateTime.Now;

                    this.ProposalLoader.Save(proposal);
                }
                else if (emailType == EmailType.ForecastAlertEmail)
                {
                    // calling the update directly since the Upsert doesn't update the ForecastEmailSent property
                    this.ProposalLoader.UpdateProposalForecastEmailSent(proposalId, proposal.UpdateDate);
                }
                else if (emailType == EmailType.ModExecutionDateRequired)
                {
                    proposal.ModExecutedLastEmailed = DateTime.Now;

                    this.ProposalLoader.Save(proposal);
                }
                else
                {
                    proposal.WorkflowStatusLastUpdated = DateTime.Now;

                    switch (emailType)
                    {
                        case EmailType.InitialApprovalEmail:
                            proposal.WorkflowStatus = WorkflowStatus.InitialApproverEmail;
                            break;
                        case EmailType.SecondApprovalEmail:
                            proposal.WorkflowStatus = WorkflowStatus.SecondApproverEmail;
                            break;
                        case EmailType.FinalApprovalEmail:
                            proposal.WorkflowStatus = WorkflowStatus.ThirdApproverEmail;
                            break;
                        case EmailType.LeadAlertForApprovers:
                            proposal.WorkflowStatus = WorkflowStatus.LeadEstimatorAlert;
                            break;
                        case EmailType.InitialLOBApprovalEmail:
                            proposal.WorkflowStatus = WorkflowStatus.InitialLOBLeadEmail;
                            break;
                        case EmailType.SecondLOBApprovalEmail:
                            proposal.WorkflowStatus = WorkflowStatus.SecondLOBLeadEmail;
                            break;
                        case EmailType.FinalLOBApprovalEmail:
                            proposal.WorkflowStatus = WorkflowStatus.ThirdLOBLeadEmail;
                            break;
                        case EmailType.LeadAlertForLOBApprover:
                            proposal.WorkflowStatus = WorkflowStatus.LeadEstimatorAlertLOB;
                            break;
                        case EmailType.LOBApprovedEmail:
                            proposal.WorkflowStatus = WorkflowStatus.ProposalLocked;
                            break;
                        default:
                            // We still want to update the workflowstatus last updated, for things like post approval
                            break;
                    }

                    this.ProposalLoader.Save(proposal);
                }
            }
        }

        /// <summary>
        /// Gets the optional document missing reminder emails to be sent
        /// </summary>
        /// <returns>A collection of the email info dtos for the emails to be sent</returns>
        public ICollection<EmailInformationDto> GetDocumentReminderEmailsToBeSent()
        {
            ICollection<EmailInformationDto> emailList = new List<EmailInformationDto>();

            using (IES.Common.StopwatchTimer sw = new IES.Common.StopwatchTimer("EmailInformationLoader.GetDocumentReminderEmailsToBeSent", this.Log))
            {
                // Get all approved Proposals approved in 2020 or later, when this feature was first implemented
                // so we don't send for proposals missing the document because PSAs were not implemented when they were created
                ICollection<ProposalDto> approvedProposals = this.ProposalLoader.GetAllCompletedProposalsAfterSubmitDate(DOCUMENT_REMINDER_CUTOFF_DATE);

                foreach (ProposalDto proposal in approvedProposals)
                {
                    // Only send for approved proposals missing the optional document that have been approved for a week or more
                    DateTime? approvalDate = this.ProposalLoader.GetProposalCompletedDate(proposal.Id);

                    if (approvalDate != null && approvalDate <= DateTime.Today.AddDays(-7))
                    {
                        if (!this.AttachmentLoader.OptionalAttachmentHasBeenUploaded(proposal.Id))
                        {
                            // retrieve permissions
                            ICollection<ProposalPermissionDto> permissions = this.RetrievePermissions(proposal.Id);

                            string leadEstEmail = this.RetrieveEmailForRole(proposal, PtmRole.Pricer, permissions);
                            string backupEstEmail = this.RetrieveEmailForRole(proposal, PtmRole.BackupPricer, permissions);

                            string emailTo = leadEstEmail;
                            if (!string.IsNullOrEmpty(backupEstEmail))
                            {
                                emailTo += ";" + backupEstEmail;
                            }

                            UserDTO estManagerDelegate = this.RetrieveUserForRole(proposal, PtmRole.LOBEstLead, permissions);

                            emailList.Add(new EmailInformationDto()
                            {
                                EmailAddress = emailTo,
                                ccUsers = new Collection<UserDTO>() { estManagerDelegate },
                                ProposalEmailType = EmailType.OptionalDocumentReminderEmail,
                                ProposalId = proposal.Id,
                                ProposalTitle = proposal.ProposalTitle,
                                TrackingNumber = proposal.TrackingNumber,
                                AdditionalText = string.Empty
                            });
                        }
                    }
                }
            }

            return emailList;
        }

        /// <summary>
        /// Creates the email content for the Mod Executed Date missing reminders
        /// </summary>
        /// <param name="emailList">List of added email content</param>
        /// <param name="proposals">Proposals that need to have the reminder sent</param>
        private void ProcessModExecutionDateMissing(ICollection<EmailInformationDto> emailList, ICollection<ProposalDto> proposals)
        {
            foreach (ProposalDto proposal in proposals)
            {
                string contractsEmail = this.RetrieveEmailForRole(proposal, PtmRole.ContractsPOC);
                string backupContractsEmail = this.RetrieveEmailForRole(proposal, PtmRole.BackupContractsPOC);
                if (!string.IsNullOrEmpty(backupContractsEmail))
                {
                    contractsEmail = $"{contractsEmail};{backupContractsEmail}";
                }

                if (!string.IsNullOrEmpty(contractsEmail))
                {
                    EmailInformationDto emailInfo = new EmailInformationDto()
                    {
                        EmailAddress = contractsEmail,
                        ProposalEmailType = EmailType.ModExecutionDateRequired,
                        ProposalId = proposal.Id,
                        ProposalTitle = proposal.ProposalTitle,
                        TrackingNumber = proposal.ForecastedTrackingNumber,
                        AdditionalText = string.Empty
                    };

                    emailList.Add(emailInfo);
                }
            }            
        }

        /// <summary>
        /// Processes emails after the LOB approved.
        /// </summary>
        /// <param name="emailList">A list of emails needed sending.</param>
        /// <param name="proposal">The proposal to process.</param>
        private void ProcessLOBApproved(ICollection<EmailInformationDto> emailList, ProposalDto proposal)
        {
            string email = this.RetrieveEmailForRole(proposal, PtmRole.Pricer);

            if (!string.IsNullOrEmpty(email))
            {
                EmailInformationDto emailInfo = new EmailInformationDto()
                {
                    EmailAddress = email,
                    ProposalEmailType = EmailType.LOBApprovedEmail,
                    ProposalId = proposal.Id,
                    ProposalTitle = proposal.ProposalTitle,
                    TrackingNumber = proposal.TrackingNumber,
                    AdditionalText = proposal.ApprovalEmailText
                };

                emailList.Add(emailInfo);
            }
        }

        /// <summary>
        /// Processes Lead Alert emails if LOB never approved.
        /// </summary>
        /// <param name="emailList">A list of emails needed sending.</param>
        /// <param name="collection">The collection of proposals to process.</param>
        private void ProcessLeadAlertForLOBApprover(ICollection<EmailInformationDto> emailList, ICollection<ProposalDto> collection)
        {
            foreach (ProposalDto proposal in collection)
            {
                string email = this.RetrieveEmailForRole(proposal, PtmRole.Pricer);
                if (!string.IsNullOrEmpty(email))
                {
                    EmailInformationDto emailInfo = new EmailInformationDto()
                    {
                        EmailAddress = email,
                        ProposalEmailType = EmailType.LeadAlertForLOBApprover,
                        ProposalId = proposal.Id,
                        ProposalTitle = proposal.ProposalTitle,
                        TrackingNumber = proposal.TrackingNumber,
                        AdditionalText = proposal.ApprovalEmailText
                    };
                    emailList.Add(emailInfo);
                }
            }
        }

        /// <summary>
        /// Processes the forecast proposals that need email alerts sent.
        /// </summary>
        /// <param name="emailList">The email list.</param>
        /// <param name="collection">The collection.</param>
        private void ProcessForecastProposals(ICollection<EmailInformationDto> emailList, ICollection<ProposalDto> collection)
        {
            foreach (ProposalDto proposal in collection)
            {
                string email = this.RetrieveEmailForRole(proposal, PtmRole.LOBEstLead);
                if (!string.IsNullOrEmpty(email))
                {
                    EmailInformationDto emailInfo = new EmailInformationDto()
                    {
                        EmailAddress = email,
                        ProposalEmailType = EmailType.ForecastAlertEmail,
                        ProposalId = proposal.Id,
                        ProposalTitle = proposal.ProposalTitle,
                        TrackingNumber = proposal.ForecastedTrackingNumber,
                        AdditionalText = string.Empty
                    };
                    emailList.Add(emailInfo);
                }
            }
        }

        /// <summary>
        /// Processes the certification timeline proposals that need email alerts sent.
        /// </summary>
        /// <param name="emailList">The email list.</param>
        /// <param name="collection">The collection.</param>
        private void ProcessCertificationTimeline(ICollection<EmailInformationDto> emailList, ICollection<ProposalDto> collection)
        {
            foreach (ProposalDto proposal in collection)
            {
                string email = this.RetrieveEmailForRole(proposal, PtmRole.Pricer);

                string contractsEmail = this.RetrieveEmailForRole(proposal, PtmRole.ContractsPOC);
                if (!email.Contains(contractsEmail))
                {
                    email += ";" + contractsEmail;
                }

                string backupContractsEmail = this.RetrieveEmailForRole(proposal, PtmRole.BackupContractsPOC);
                if (!email.Contains(backupContractsEmail))
                {
                    email += ";" + backupContractsEmail;
                }

                ICollection<ProposalPermissionDto> permissions = this.RetrievePermissions(proposal.Id);

                if (permissions.Any(x => x.Role == PtmRole.SupplyChainPOCMatl))
                {
                    string matEmail = this.RetrieveEmailForRole(proposal, PtmRole.SupplyChainPOCMatl);
                    if (!email.Contains(matEmail))
                    {
                        email += ";" + matEmail;
                    }
                }

                if (permissions.Any(x => x.Role == PtmRole.SupplyChainPOCSubs))
                {
                    string subEmail = this.RetrieveEmailForRole(proposal, PtmRole.SupplyChainPOCSubs);
                    if (!email.Contains(subEmail))
                    {
                        email += ";" + subEmail;
                    }
                }

                if (!string.IsNullOrEmpty(email))
                {
                    EmailInformationDto emailInfo = new EmailInformationDto()
                    {
                        EmailAddress = email,
                        ProposalEmailType = EmailType.CertificationTimelineEmail,
                        ProposalId = proposal.Id,
                        ProposalTitle = proposal.ProposalTitle,
                        TrackingNumber = proposal.TrackingNumber,
                        AdditionalText = string.Empty
                    };
                    emailList.Add(emailInfo);
                }
            }
        }
        
        /// <summary>
        /// Processes email alerts for LOB approver.
        /// </summary>
        /// <param name="emailList">A list of emails needed sending.</param>
        /// <param name="collection">The collection of proposals to process.</param>
        /// <param name="emailType">Type of the email.</param>
        private void ProcessLOBApprover(ICollection<EmailInformationDto> emailList, ICollection<ProposalDto> collection, EmailType emailType)
        {
            foreach (ProposalDto proposal in collection)
            {
                string email = this.RetrieveEmailForRole(proposal, PtmRole.LOBEstLead);
                if (!string.IsNullOrEmpty(email))
                {
                    EmailInformationDto emailInfo = new EmailInformationDto()
                    {
                        EmailAddress = email,
                        ProposalEmailType = emailType,
                        ProposalId = proposal.Id,
                        ProposalTitle = proposal.ProposalTitle,
                        TrackingNumber = proposal.TrackingNumber,
                        AdditionalText = proposal.ApprovalEmailText
                    };
                    emailList.Add(emailInfo);
                }
            }
        }

        /// <summary>
        /// Processes Lead Alert emails if the approvers never approved.
        /// </summary>
        /// <param name="emailList">A list of emails needed sending.</param>
        /// <param name="collection">The collection of proposals to process.</param>
        private void ProcessLeadAlertForApprover(ICollection<EmailInformationDto> emailList, ICollection<ProposalDto> collection)
        {
            foreach (ProposalDto proposal in collection)
            {
                string email = this.RetrieveEmailForRole(proposal, PtmRole.Pricer);
                if (!string.IsNullOrEmpty(email))
                {
                    EmailInformationDto emailInfo = new EmailInformationDto()
                    {
                        EmailAddress = email,
                        ProposalEmailType = EmailType.LeadAlertForApprovers,
                        ProposalId = proposal.Id,
                        ProposalTitle = proposal.ProposalTitle,
                        TrackingNumber = proposal.TrackingNumber,
                        AdditionalText = proposal.ApprovalEmailText
                    };
                    emailList.Add(emailInfo);
                }
            }
        }

        /// <summary>
        /// Processes email alerts for proposal approvers.
        /// </summary>
        /// <param name="emailList">A list of emails needed sending.</param>
        /// <param name="collection">The collection of proposals to process.</param>
        /// <param name="emailType">The current emailType inside the workflow.</param>
        private void ProcessApprovers(ICollection<EmailInformationDto> emailList, ICollection<ProposalDto> collection, EmailType emailType)
        {
            foreach (ProposalDto proposal in collection)
            {
                // retrieve permissions
                ICollection<ProposalPermissionDto> permissions = this.RetrievePermissions(proposal.Id);

                string email = string.Empty;

                if (!proposal.CoverSheetApproverSignedDate.HasValue)
                {
                    string coverSheetEmail = this.RetrieveEmailForRole(proposal, PtmRole.CoverSheetApprover, permissions);
                    if (!string.IsNullOrEmpty(coverSheetEmail))
                    {
                        email += coverSheetEmail + ";";
                    }
                }

                if (!proposal.PricingVerifierSignedDate.HasValue)
                {
                    string pricingVerifierEmail = this.RetrieveEmailForRole(proposal, PtmRole.PricingVerification, permissions);
                    if (!string.IsNullOrEmpty(pricingVerifierEmail))
                    {
                        email += pricingVerifierEmail + ";";
                    }
                }

                if (!proposal.IndependentReviewerSignedDate.HasValue)
                {
                    string independentReviewerEmail = this.RetrieveEmailForRole(proposal, PtmRole.PeerReviewer, permissions);
                    if (!string.IsNullOrEmpty(independentReviewerEmail))
                    {
                        email += independentReviewerEmail + ";";
                    }
                }

                EmailInformationDto emailInfo = new EmailInformationDto()
                {
                    EmailAddress = email.TrimEnd(';'),
                    ProposalEmailType = emailType,
                    ProposalId = proposal.Id,
                    ProposalTitle = proposal.ProposalTitle,
                    TrackingNumber = proposal.TrackingNumber,
                    AdditionalText = proposal.ApprovalEmailText
                };

                emailList.Add(emailInfo);
            }
        }

        /// <summary>
        /// Retrieves the permissions for a proposal.
        /// </summary>
        /// <param name="proposalId">The proposal identifier.</param>
        /// <returns>The permissions for a proposal.</returns>
        private ICollection<ProposalPermissionDto> RetrievePermissions(int proposalId)
        {
            ICollection<int> permissionIds = this.PermissionLoader.GetIdsByProposalId(proposalId);
            ICollection<ProposalPermissionDto> permissions = this.PermissionLoader.GetByIds(permissionIds);
            return permissions;
        }

        /// <summary>
        /// Retrieves the email for a particular role.
        /// </summary>
        /// <param name="proposal">The proposal.</param>
        /// <param name="role">The role.</param>
        /// <param name="permissions">The permissions.</param>
        /// <returns>An email address for a role found in the permissions passed in.</returns>
        private string RetrieveEmailForRole(ProposalDto proposal, PtmRole role, ICollection<ProposalPermissionDto> permissions)
        {
            string email = string.Empty;
            ProposalPermissionDto permission = permissions.FirstOrDefault(p => p.Role == role);

            if (permission == null)
            {
                bool coverSheetApproverRequired = proposal.IsCCPDRequired.HasValue && proposal.IsCCPDRequired.Value;

                // Since the Independent Reviewer is optional and Cover Sheet Approver sometimes, don't log if not found
                if (role != PtmRole.PeerReviewer)
                {
                    if (coverSheetApproverRequired || role != PtmRole.CoverSheetApprover)
                    {
                        this.Log.Error(string.Format("There is no Permission associated with Proposal {0} and role {1} ", proposal.Id, role));
                    }
                }
            }
            else
            {
                UserDTO user = this.UserLoader.GetById(permission.UserId);

                if (user != null)
                {
                    email = user.EmailAddress;
                }
            }

            return email;
        }

        /// <summary>
        /// Retrieves the email for a specific role in a proposal's permissions.
        /// </summary>
        /// <param name="proposal">The proposal.</param>
        /// <param name="role">The role.</param>
        /// <returns>The email or an empty string if not found for the specific role in the permissions for this proposal.
        /// </returns>
        private string RetrieveEmailForRole(ProposalDto proposal, PtmRole role)
        {
            ICollection<ProposalPermissionDto> permissions = this.RetrievePermissions(proposal.Id);
            return this.RetrieveEmailForRole(proposal, role, permissions);
        }

        /// <summary>
        /// Retrieves the user DTO for a particular role.
        /// </summary>
        /// <param name="proposal">The proposal.</param>
        /// <param name="role">The role.</param>
        /// <param name="permissions">The permissions.</param>
        /// <returns>A user dto for a role found in the permissions passed in.</returns>
        private UserDTO RetrieveUserForRole(ProposalDto proposal, PtmRole role, ICollection<ProposalPermissionDto> permissions)
        {
            UserDTO user = new UserDTO();
            ProposalPermissionDto permission = permissions.FirstOrDefault(p => p.Role == role);

            if (permission == null)
            {
                bool coverSheetApproverRequired = proposal.IsCCPDRequired.HasValue && proposal.IsCCPDRequired.Value;

                // Since the Independent Reviewer is optional and Cover Sheet Approver sometimes, don't log if not found
                if (role != PtmRole.PeerReviewer)
                {
                    if (coverSheetApproverRequired || role != PtmRole.CoverSheetApprover)
                    {
                        this.Log.Error(string.Format("There is no Permission associated with Proposal {0} and role {1} ", proposal.Id, role));
                    }
                }
            }
            else
            {
                user = this.UserLoader.GetById(permission.UserId);
            }

            return user;
        }
    }
}

// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web.Configuration;
    using GenTRAC.ActionLogic.Mediator;
    using GenTRAC.ActionLogic.ModelView;
    using GenTRAC.ActionLogic.ModelView.Checklist;
    using GenTRAC.ActionLogic.Validation;
    using GenTRAC.DataBridge.Common.Security;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Objects;
    using GenTRAC.Objects.FullObject;
    using IES.Common;
    using IES.Common.Exceptions;

    /// <summary>
    /// Gen TRAC Controller logic
    /// </summary>
    public class GenTRACControllerLogic
    {
        /// <summary>
        /// Base Url to
        /// </summary>
        private const string BASE_URL_TOKEN = "__BASE_URL__";

        /// <summary>
        /// The logger
        /// </summary>
        private IES.Common.Logger logger = new IES.Common.Logger(typeof(GenTRACControllerLogic));

        /// <summary>
        /// Create static Regex object for ChecklistRowInvalid.
        /// </summary>
        private static Regex regexChecklistRowInvalid = new Regex(Constants.CHECKLIST_ROW_INVALID_REGEX, RegexOptions.None);

        /// <summary>
        /// Security Access class
        /// </summary>
        protected ISecurityAccess SecurityAccess { get; set; }

        /// <summary>
        /// The proposal loader
        /// </summary>
        protected IProposalLoader ProposalLoader { get; set; }

        /// <summary>
        /// The user mapper
        /// </summary>
        protected IUserMapper UserMapper { get; set; }

        /// <summary>
        /// Approvals Loader
        /// </summary>
        protected IApprovalsLoader ApprovalsLoader { get; set; }

        /// <summary>
        /// Object Factory
        /// </summary>
        protected IFullObjectFactory ObjectFactory { get; set; }

        /// <summary>
        /// checklist loader
        /// </summary>
        private IProposalChecklistLoader proposalChecklistLoader = null;

        /// <summary>
        /// checklist Mediator
        /// </summary>
        protected IChecklistMediator ChecklistMediator { get; private set; }

        /// <summary>
        /// proposal mediator 
        /// </summary>
        protected IProposalMediator ProposalMediator { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inSecurityAccess">Security Access</param>
        /// <param name="inProposalLoader">Proposal Loader</param>
        /// <param name="inUserMapper">User Mapper</param>
        /// <param name="objectFactory">Object Factory</param>
        /// <param name="approvalsLoader">Approvals Loader</param>
        /// <param name="proposalChecklistLoader">Proposal Checklist Loader</param>
        /// <param name="inChecklistMediator">Checklist Mediator</param>
        /// <param name="inProposalMediator">Proposal Mediator</param>
        public GenTRACControllerLogic(
            ISecurityAccess inSecurityAccess,
            IProposalLoader inProposalLoader,
            IUserMapper inUserMapper,
            IFullObjectFactory objectFactory,
            IApprovalsLoader approvalsLoader,
            IProposalChecklistLoader proposalChecklistLoader,
            IChecklistMediator inChecklistMediator,
            IProposalMediator inProposalMediator)
        {
            this.SecurityAccess = inSecurityAccess;
            this.ProposalLoader = inProposalLoader;
            this.UserMapper = inUserMapper;
            this.ObjectFactory = objectFactory;
            this.ApprovalsLoader = approvalsLoader;
            this.proposalChecklistLoader = proposalChecklistLoader;
            this.ChecklistMediator = inChecklistMediator;
            this.ProposalMediator = inProposalMediator;
        }

        /// <summary>
        /// Check permissions and return the authorization of the user
        /// </summary>
        /// <param name="inPage">The page to check</param>
        /// <param name="inProposalId">The proposal id (optional, null if not required)</param>
        /// <returns>The view to redirect to if security error, NULL otherwise (i.e. NULL indicates the user is allowed to proceed</returns>
        public SecurityAuthorizationAndRole CheckPermissions(PtmSecurityPage inPage, int? inProposalId)
        {
            Dictionary<PtmSecurityPage, SecurityAuthorizationAndRole> securityPermission = null;

            using (IES.Common.StopwatchTimer sw = new IES.Common.StopwatchTimer("GenTRACControllerLogic.CheckPermissions", this.logger))
            {
                securityPermission = this.CheckPermissions(new List<PtmSecurityPage>() { inPage }, inProposalId);

                //// NOTE: If you want to turn security "off" uncomment the next line.  everyone accessing the system will get CRUD access...
                //// authorizationForUser = SecurityAuthorization.CreateReadUpdateDelete;
            }

            return securityPermission.Values.First();
        }

        /// <summary>
        /// Check permissions and return the authorization of the user
        /// </summary>
        /// <param name="inPages">The pages to check</param>
        /// <param name="inProposalId">The proposal id (optional, null if not required)</param>
        /// <returns>The view to redirect to if security error, NULL otherwise (i.e. NULL indicates the user is allowed to proceed)</returns>
        public Dictionary<PtmSecurityPage, SecurityAuthorizationAndRole> CheckPermissions(ICollection<PtmSecurityPage> inPages, int? inProposalId)
        {
            if (inPages == null)
            {
                throw new ArgumentNullException(nameof(inPages));
            }

            Dictionary<PtmSecurityPage, SecurityAuthorizationAndRole> securityDictionary = new Dictionary<PtmSecurityPage, SecurityAuthorizationAndRole>();

            using (IES.Common.StopwatchTimer sw = new IES.Common.StopwatchTimer("GenTRACControllerLogic.CheckPermission", this.logger))
            {
                foreach (PtmSecurityPage page in inPages)
                {
                    PtmRole highestRole;
                    SecurityAuthorization authorizationForUser = this.SecurityAccess.IsAuthorized(
                        new SecurityPermissionsRequested
                        {
                            PageToCheck = page,
                            ProposalId = inProposalId
                        }, out highestRole);

                    securityDictionary.Add(page, new SecurityAuthorizationAndRole { Authorization = authorizationForUser, Role = highestRole });
                }
            }

            return securityDictionary;
        }

        #region Retrieves

        /// <summary>
        /// Returns a view model 
        /// </summary>
        /// <returns>The master view model</returns>
        public GenTRACMasterModelView GetMasterView()
        {
            GenTRACMasterModelView model = new GenTRACMasterModelView();

            model.HeaderFooter =
                   "Lockheed Martin Proprietary Information";

            return model;
        }
        #endregion

        /// <summary>
        /// Get proposal dto by proposal id
        /// </summary>
        /// <param name="proposalId">proposal id</param>
        /// <returns>proposal dto</returns>
        public ProposalDto GetByProposalId(int proposalId)
        {
            return this.ProposalLoader.GetById(proposalId);
        }

        /// <summary>
        /// Get the Users Online from the commonDataMapper
        /// </summary>
        /// <returns>UsersOnlineDTO</returns>
        public UsersOnlineDTO GetUsersOnline()
        {
            UsersOnlineDTO toReturn = this.UserMapper.GetUsersOnline();

            return toReturn;
        }

        /// <summary>
        /// Uses the common data mapper to update the online user status
        /// </summary>
        /// <param name="user">user</param>
        /// <param name="inProposalId">proposal Id</param>
        [ExcludeFromCodeCoverage]
        public void UpdateUsersStatus(IES.Common.UserData user, int? inProposalId)
        {
            string trackingNumber = string.Empty;
            if (inProposalId.HasValue && inProposalId != -1)
            {
                ProposalDto proposal = this.GetByProposalId(inProposalId.Value);
                trackingNumber = proposal.IsForecastProposal ? proposal.ForecastedTrackingNumber : proposal.TrackingNumber;
            }

            this.UserMapper.UpdateUsersStatus(user, trackingNumber);
        }

        /// <summary>
        /// Gets the Data for the Online Users
        /// </summary>
        /// <returns>The Model view.</returns>
        [ExcludeFromCodeCoverage]
        public WhosOnlineModelView GetWhosOnlineData()
        {
            WhosOnlineModelView model = new WhosOnlineModelView();
            // user details to populate the "Who's Online" popup dialog.
            UsersOnlineDetailDescriptor usersOnline = this.GetUsersOnline().GetAllUsers();

            model.LastCacheRefresh = usersOnline.LastCacheRefresh.ToString(Constants.USERS_ONLINE_LONG_DATE_FORMAT);
            model.MinutesToExpireUsers = usersOnline.MinutesToExpireUsers;
            model.UserDetails = usersOnline.UserOnlineDetailsCollection;

            return model;
        }

        /// <summary>
        /// Gets number of pending approvals for the current user
        /// </summary>
        /// <returns>Number of approvals pending for the user</returns>
        public int GetPendingApprovalsCountForUser()
        {
            return this.ApprovalsLoader.GetPendingApprovalsCountForUser(this.UserMapper.GetActiveUser().Id);
        }

        /// <summary>
        /// Determines if Independent Reviewer approval is needed
        /// An independent reviewer is needed if the Lead Estimator and Cover Sheet Approver are the same person or if CCOPD is set to false
        /// </summary>
        /// <param name="proposal">The proposal to check.</param>
        /// <returns>True if the independent reviewer is needed, false if not</returns>
        public bool IndependentReviewerIsNeeded(FullProposal proposal)
        {
            if (proposal == null)
            {
                throw new ArgumentNullException(nameof(proposal));
            }

            bool toReturn = false;

            if (proposal.IsCCPDRequired.HasValue && proposal.IsCCPDRequired.Value)
            {
                ProposalPermissionDto leadEstimatorID = proposal.Permissions.FirstOrDefault(x => x.Role == PtmRole.Pricer);
                ProposalPermissionDto coverSheetApproverID = proposal.Permissions.FirstOrDefault(x => x.Role == PtmRole.CoverSheetApprover);

                if (leadEstimatorID != null && coverSheetApproverID != null && leadEstimatorID.UserId == coverSheetApproverID.UserId)
                {
                    toReturn = true;
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get the full proposal DTO
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>full Proposal DTO</returns>
        public FullProposal GetFullProposalDto(int? proposalId)
        {
            FullProposal fullProposal = null;
            if (proposalId.HasValue && proposalId >= 0)
            {
                ProposalDto proposal = this.ProposalLoader.GetById(proposalId.Value);
                fullProposal = this.ObjectFactory.CreateFullProposal(proposal);
            }

            return fullProposal;
        }

        /// <summary>
        /// Validate if the answer to question One of the PPR checklist is still no for Peer reviewer.
        /// </summary>
        /// <param name="checklistGeneralInfo">checklist general information</param>
        /// <param name="inValidationErrors">validation errors</param>
        /// <returns>True if there are validation errors, false otherwise</returns>
        public bool ValidatePARVisibility(ChecklistGeneralInformationModelView checklistGeneralInfo, ICollection<ValidationMessage> inValidationErrors)
        {
            if (checklistGeneralInfo == null)
            {
                throw new ArgumentNullException(nameof(checklistGeneralInfo));
            }

            if (inValidationErrors == null)
            {
                throw new ArgumentNullException(nameof(inValidationErrors));
            }

            bool validationErrors = false;
            // Check here if the answer to question 1 is still no, if its not, add a validation error and return false
            if (checklistGeneralInfo.IsPTMChecklistUIEnabled == false && this.IsPeerChecklistAndQuestionOneFalse(checklistGeneralInfo))
            {
                inValidationErrors.Add(new ValidationMessage(ValidationConstants.ChecklistValidationConstants.PRICER_UPDATED_QUESTION_ONE_OF_PPR_WHILE_PEER_EDITING));
                validationErrors = true;
            }

            return validationErrors;
        }

        /// <summary>
        /// Checks if the PAR section was displayed for this particular proposal ID.
        /// </summary>
        /// <param name="proposalId">The proposal id.</param>
        /// <returns>True if PAR should be displaeyd, false otherwise.</returns>
        public bool IsParCurrentlyDisplayed(int proposalId)
        {
            FullProposal fullProposal = this.GetFullProposalDto(proposalId);

            // check if the PAR section was displayed for this particular proposal ID 
            // if it wasn't, then we can go ahead and skip these validation messages
            int? contentID = this.GetPPRQuestion1ContentId(proposalId, fullProposal);
            bool parIsCurrentlyDisplayed = true;
            if (contentID.HasValue)
            {
                ChecklistResponseOption currentResponse = this.GetPPRQuestion1Response(proposalId, fullProposal);
                if (currentResponse == ChecklistResponseOption.Yes)
                {
                    parIsCurrentlyDisplayed = false;
                }
            }

            return parIsCurrentlyDisplayed;
        }

        /// <summary>
        /// Checks if this is a peer reviewer and the answer to question one has been updated to false.
        /// </summary>
        /// <param name="checklistGeneralInfo">The checklist general info.</param>
        /// <returns>True if a peer reviewer is saving checklist and answer to question one is true, false otherwise.</returns>
        public bool IsPeerChecklistAndQuestionOneFalse(ChecklistGeneralInformationModelView checklistGeneralInfo)
        {
            if (checklistGeneralInfo == null)
            {
                throw new ArgumentNullException(nameof(checklistGeneralInfo));
            }

            if (checklistGeneralInfo.ShowChecklistResponse == ShowChecklistResponse.Peer)
            {
                // Check here if the answer to question 1 is still yes, if its not, return the json result with something flagged to alert the user they no longer have to fill out the PAR
                if (!this.IsParCurrentlyDisplayed(checklistGeneralInfo.ProposalID))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get the content ID for PPR's Question 1
        /// </summary>
        /// <param name="inProposalId">proposal id</param>
        /// <param name="fullProposalDto">Full proposal</param>
        /// <returns>content ID</returns>
        public int? GetPPRQuestion1ContentId(int inProposalId, FullProposal fullProposalDto)
        {
            int? toReturn = null;

            if (fullProposalDto == null)
            {
                fullProposalDto = this.GetFullProposalDto(inProposalId);
            }

            if (fullProposalDto != null)
            {
                var pprData = fullProposalDto.ProposalChecklistPPRData;

                if (pprData != null)
                {
                    var checklistContent = pprData;

                    ICollection<ChecklistContentItem> sortedChecklistContent = (from c in checklistContent.Content
                                                                                where c.TextType == ChecklistTextType.Question
                                                                                select c).ToList();
                    var contentID = sortedChecklistContent.OrderBy(x => x.SortOrder).Select(x => x.Id).FirstOrDefault();
                    return contentID;
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get the response for PPR's data section Question 1. 
        /// </summary>
        /// <param name="inProposalId">proposal id</param>
        /// <param name="fullProposalDto">Full proposal</param>
        /// <returns>checklist response option</returns>
        public ChecklistResponseOption GetPPRQuestion1Response(int inProposalId, FullProposal fullProposalDto)
        {
            ChecklistResponseOption toReturn = ChecklistResponseOption.No;

            // get PPR's Question 1 content ID
            int? contentID = this.GetPPRQuestion1ContentId(inProposalId, fullProposalDto);
            if (contentID.HasValue)
            {
                ICollection<ChecklistResponseItem> propCheck = new Collection<ChecklistResponseItem>();

                propCheck = this.proposalChecklistLoader.GetPPRResponses(inProposalId);

                // get the response for this content ID
                ChecklistResponseOption question1Response = (from p in propCheck
                                                             where p.ChecklistContentId == contentID.Value
                                                             select p.Response).FirstOrDefault();

                return question1Response;
            }

            return toReturn;
        }

        /// <summary>
        /// Validate checklist
        /// </summary>
        /// <param name="checklistGeneralInfo">checklist general information</param>
        /// <param name="checklistProposalPricingData">checklist proposal pricing data</param>
        /// <param name="checklistPPRDocumentData">checklist PPR document section</param>
        /// <param name="checklistPARDocumentData">checklist PAR document section</param>
        /// <param name="isSubmit">true if submittal, false if save</param>
        /// <param name="inValidationErrors">validation errors</param>
        /// <returns>Full proposal</returns>
        public FullProposal ValidateChecklist(ChecklistGeneralInformationModelView checklistGeneralInfo, ChecklistProposalPricingDataModelView checklistProposalPricingData, ChecklistProposalPricingReviewDocumentModelView checklistPPRDocumentData, ChecklistProposalAdequacyReviewDocumentModelView checklistPARDocumentData, bool isSubmit, ICollection<ValidationMessage> inValidationErrors)
        {
            FullProposal fullProposal = null;

            if (checklistGeneralInfo == null)
            {
                throw new ArgumentNullException(nameof(checklistGeneralInfo));
            }

            if (checklistProposalPricingData == null)
            {
                throw new ArgumentNullException(nameof(checklistProposalPricingData));
            }

            if (checklistPPRDocumentData == null)
            {
                throw new ArgumentNullException(nameof(checklistPPRDocumentData));
            }

            if (checklistPARDocumentData == null)
            {
                throw new ArgumentNullException(nameof(checklistPARDocumentData));
            }

            if (inValidationErrors == null)
            {
                throw new ArgumentNullException(nameof(inValidationErrors));
            }

            fullProposal = this.GetFullProposalDto(checklistPARDocumentData.ProposalID);
            int? contentID = this.GetPPRQuestion1ContentId(checklistPARDocumentData.ProposalID, fullProposal);

            // check if the PAR section was displayed for this particular proposal ID 
            bool parIsCurrentlyDisplayed = this.ShouldChecklistPARSectionBeDisplayed(checklistPARDocumentData.ProposalID, fullProposal);

            #region Validate Pricer Page number and Pricer/Peer Row Comment
            // Do this validation on the save or submit for both Pricer and Peer on the PAR checklist.  Validates for invalid characters.

            if (parIsCurrentlyDisplayed && this.ShowComment(checklistPARDocumentData.ChecklistVersion))
            {
                this.ValidatePARPageNumbersAndRowComments(checklistGeneralInfo, checklistPARDocumentData, inValidationErrors);
            }
            #endregion

            // only do certain save validation if its a pricer doing the save
            if (checklistGeneralInfo.ShowChecklistResponse == ShowChecklistResponse.Pricer)
            {
                // the following validation checks are always done regardless if this is a save or submit
                if (!string.IsNullOrEmpty(checklistGeneralInfo.EstimatingSubmitsToContractsDate))
                {
                    try
                    {
                        checklistGeneralInfo.EstimatingSubmitsToContractsDate.ToDateTime("MM/dd/yyyy");
                    }
                    catch (FormatException)
                    {
                        inValidationErrors.Add(new ValidationMessage(ValidationConstants.ChecklistValidationConstants.SUBMITTAL_DATE_FORMAT));
                    }
                }
            }

            #region Submit Validation
            // if this is a submit then do more validation checks including checking for required fields
            if (isSubmit)
            {
                // only do the general info validation if its the pricer saving
                if (checklistGeneralInfo.ShowChecklistResponse == ShowChecklistResponse.Pricer)
                {
                    if (string.IsNullOrEmpty(checklistGeneralInfo.EstimatingSubmitsToContractsDate))
                    {
                        inValidationErrors.Add(new ValidationMessage(ValidationConstants.ChecklistValidationConstants.ESTIMATING_SUBMITS_TO_CONTRACTS_DATE_REQUIRED));
                    }

                    if (string.IsNullOrEmpty(checklistGeneralInfo.SubmittedValue))
                    {
                        inValidationErrors.Add(new ValidationMessage(ValidationConstants.ChecklistValidationConstants.SUBMITTED_VALUE_REQUIRED));
                    }

                    if (checklistProposalPricingData.UseCostThroughCom)
                    {
                        if (string.IsNullOrEmpty(checklistProposalPricingData.CostThroughCom))
                        {
                            inValidationErrors.Add(new ValidationMessage(ValidationConstants.ChecklistValidationConstants.COST_THROUGH_COM_NEEDED));
                        }

                        if (string.IsNullOrEmpty(checklistProposalPricingData.ProfitFeeTotal))
                        {
                            inValidationErrors.Add(new ValidationMessage(ValidationConstants.ChecklistValidationConstants.PROFIT_FEE_NEEDED));
                        }
                    }
                }

                // only do PPR validation on a submit if its the Pricer saving and 
                //  (Pre-PTM checklist or (Post-PTM checklist and CCPDRequired is set to Yes))
                if (checklistPPRDocumentData.ShowChecklistResponse == ShowChecklistResponse.Pricer &&
                    (checklistGeneralInfo.IsPTMChecklistUIEnabled == false ||
                     (checklistGeneralInfo.IsPTMChecklistUIEnabled == true && fullProposal.IsCCPDRequired.HasValue && fullProposal.IsCCPDRequired.Value == true)))
                {
                    var questions = from q in checklistPPRDocumentData.ChecklistRows
                                    where q.RowType == ChecklistTextType.Question && q.PricerResponse == ChecklistResponseOption.NotSet
                                    select q;
                    if (questions.Any())
                    {
                        inValidationErrors.Add(new ValidationMessage(ValidationConstants.ChecklistValidationConstants.ALL_PPR_MUST_BE_ANSWERED));
                    }

                    // do not include the response of Q1 being a No as part of this validation 
                    int question1ContentID = checklistGeneralInfo.IsPTMChecklistUIEnabled == false && contentID.HasValue ? contentID.Value : 0;
                    questions = from q in checklistPPRDocumentData.ChecklistRows
                                where q.RowType == ChecklistTextType.Question && q.PricerResponse == ChecklistResponseOption.No &&
                                q.ChecklistContentId != question1ContentID
                                select q;
                    string pricerComment = checklistPPRDocumentData.PPRPricerComment;

                    if (questions.Any() && string.IsNullOrEmpty(pricerComment))
                    {
                        inValidationErrors.Add(new ValidationMessage(ValidationConstants.ChecklistValidationConstants.PRICER_COMMENT_NEEDED));
                    }
                }

                if (DateTime.Now >= checklistProposalPricingData.CostThroughComStartingDate)
                {
                    if (string.IsNullOrEmpty(checklistProposalPricingData.ProfitFeeTotal))
                    {
                        inValidationErrors.Add(new ValidationMessage(ValidationConstants.ChecklistValidationConstants.PROFIT_FEE_NEEDED));
                    }

                    if (string.IsNullOrEmpty(checklistProposalPricingData.SubmittedValue) && checklistGeneralInfo.ShowChecklistResponse != ShowChecklistResponse.Pricer)
                    {
                        inValidationErrors.Add(new ValidationMessage(ValidationConstants.ChecklistValidationConstants.SUBMITTED_VALUE_REQUIRED));
                    }

                    if (string.IsNullOrEmpty(checklistProposalPricingData.CostThroughCom))
                    {
                        inValidationErrors.Add(new ValidationMessage(ValidationConstants.ChecklistValidationConstants.COST_THROUGH_COM_NEEDED));
                    }
                }

                #region PAR Validation
                // PAR checks
                // Skip these validation messages if PAR was not displayed
                if (parIsCurrentlyDisplayed)
                {
                    this.ValidatePARForSubmit(checklistGeneralInfo, checklistPARDocumentData, inValidationErrors);
                }
                #endregion PAR Validation
            }
            #endregion Submit Validation

            return fullProposal;
        }

        /// <summary>
        /// Validate PAR checklist
        /// </summary>
        /// <param name="checklistGeneralInfo">checklist general information</param>
        /// <param name="checklistProposalPricingData">checklist proposal pricing data</param>
        /// <param name="checklistPPRDocumentData">checklist PPR document section</param>
        /// <param name="checklistPARDocumentData">checklist PAR document section</param>
        /// <param name="inValidationErrors">validation errors</param>
        public void ValidatePARChecklist(ChecklistGeneralInformationModelView checklistGeneralInfo, ChecklistProposalPricingDataModelView checklistProposalPricingData, ChecklistProposalPricingReviewDocumentModelView checklistPPRDocumentData, ChecklistProposalAdequacyReviewDocumentModelView checklistPARDocumentData, ICollection<ValidationMessage> inValidationErrors)
        {
            FullProposal fullProposal = null;

            if (checklistGeneralInfo == null)
            {
                throw new ArgumentNullException(nameof(checklistGeneralInfo));
            }

            if (checklistProposalPricingData == null)
            {
                throw new ArgumentNullException(nameof(checklistProposalPricingData));
            }

            if (checklistPPRDocumentData == null)
            {
                throw new ArgumentNullException(nameof(checklistPPRDocumentData));
            }

            if (checklistPARDocumentData == null)
            {
                throw new ArgumentNullException(nameof(checklistPARDocumentData));
            }

            if (inValidationErrors == null)
            {
                throw new ArgumentNullException(nameof(inValidationErrors));
            }

            fullProposal = this.GetFullProposalDto(checklistPARDocumentData.ProposalID);

            // check if the PAR section was displayed for this particular proposal ID 
            bool parIsCurrentlyDisplayed = this.ShouldChecklistPARSectionBeDisplayed(checklistPARDocumentData.ProposalID, fullProposal);

            // Validates PAR checklist responses for invalid characters.
            if (parIsCurrentlyDisplayed && this.ShowComment(checklistPARDocumentData.ChecklistVersion))
            {
                this.ValidatePARPageNumbersAndRowComments(checklistGeneralInfo, checklistPARDocumentData, inValidationErrors);
            }

            // PAR checks
            // Skip these validation messages if PAR was not displayed
            if (parIsCurrentlyDisplayed)
            {
                this.ValidatePARForSubmit(checklistGeneralInfo, checklistPARDocumentData, inValidationErrors);
            }
        }

        /// <summary>
        /// Validate Pricer Page numbers and Pricer/Peer Row Comments
        /// </summary>
        /// <param name="checklistGeneralInfo">checklist general information</param>
        /// <param name="checklistPARDocumentData">checklist PAR document section</param>
        /// <param name="inValidationErrors">validation errors</param>        
        private void ValidatePARPageNumbersAndRowComments(ChecklistGeneralInformationModelView checklistGeneralInfo, ChecklistProposalAdequacyReviewDocumentModelView checklistPARDocumentData, ICollection<ValidationMessage> inValidationErrors)
        {
            int numInvalidCharacterResponses = 0;

            foreach (ChecklistRowModelView row in checklistPARDocumentData.ChecklistRows)
            {
                if (checklistGeneralInfo.ShowChecklistResponse == ShowChecklistResponse.Pricer)
                {
                    if (row.PricerPageNumber != null && regexChecklistRowInvalid.Matches(row.PricerPageNumber).Count > 0)
                    {
                        ++numInvalidCharacterResponses;
                    }

                    if (row.PricerRowComment != null && regexChecklistRowInvalid.Matches(row.PricerRowComment).Count > 0)
                    {
                        ++numInvalidCharacterResponses;
                    }

                    if (numInvalidCharacterResponses > 0)
                    {
                        inValidationErrors.Add(new ValidationMessage(ValidationConstants.ChecklistValidationConstants.PAR_ROW_INVALID_CHARACTER_PRICER));
                    }
                }
                else
                {
                    if (row.PeerRowComment != null && regexChecklistRowInvalid.Matches(row.PeerRowComment).Count > 0)
                    {
                        ++numInvalidCharacterResponses;
                    }

                    if (numInvalidCharacterResponses > 0)
                    {
                        inValidationErrors.Add(new ValidationMessage(ValidationConstants.ChecklistValidationConstants.PAR_ROW_INVALID_CHARACTER_PEER));
                    }
                }
            }
        }

        /// <summary>
        /// Validate that all PAR questions have been answered, and that all page number and row comments have been populated as required.
        /// </summary>
        /// <param name="checklistGeneralInfo">checklist general information</param>
        /// <param name="checklistPARDocumentData">checklist PAR document section</param>
        /// <param name="inValidationErrors">validation errors</param>        
        private void ValidatePARForSubmit(ChecklistGeneralInformationModelView checklistGeneralInfo, ChecklistProposalAdequacyReviewDocumentModelView checklistPARDocumentData, ICollection<ValidationMessage> inValidationErrors)
        {
            if (checklistPARDocumentData.ShowChecklistResponse == ShowChecklistResponse.Pricer)
            {
                // verify that all PAR questions have been answered
                var pricerQuestions = from q in checklistPARDocumentData.ChecklistRows
                                      where q.RowType == ChecklistTextType.Question && !string.IsNullOrEmpty(q.SubmissionItem) && q.PricerResponse == ChecklistResponseOption.NotSet
                                      select q;
                if (pricerQuestions.Any())
                {
                    inValidationErrors.Add(new ValidationMessage(ValidationConstants.ChecklistValidationConstants.ALL_PAR_MUST_BE_ANSWERED));
                }

                // Verify that all row comments and page number have been populated (checklist format Jan 2015)
                // Else, perform the legacy validation which requires an Estimator comment at the bottom of the PAR checklist
                if (this.ShowComment(checklistPARDocumentData.ChecklistVersion))
                {
                    int numMissedPageNumberResponses = 0;
                    int numMissedCommentResponses = 0;
                    int numMissedCannedResponses = 0;
                    int numMissedYesResponses = 0;
                    int numMissedOtherComments = 0;

                    foreach (ChecklistRowModelView row in checklistPARDocumentData.ChecklistRows)
                    {
                        // Note: Skip response validation when SubmissionItem is null
                        if (row.RowType == ChecklistTextType.Question && !string.IsNullOrEmpty(row.SubmissionItem))
                        {
                            if (row.PricerResponse == ChecklistResponseOption.Yes && checklistGeneralInfo.DeliverChecklistDFARS)
                            {
                                if (string.IsNullOrEmpty(row.PricerPageNumber))
                                {
                                    ++numMissedPageNumberResponses;
                                }
                            }

                            if (row.PricerResponse == ChecklistResponseOption.No && string.IsNullOrEmpty(row.PricerRowComment) && checklistPARDocumentData.ChecklistVersion < ValidationConstants.ChecklistValidationConstants.CANNED_RESPONSE_CHECKLIST_VERSION)
                            {
                                ++numMissedCommentResponses;
                            }
                            
                            if (checklistPARDocumentData.ChecklistVersion >= ValidationConstants.ChecklistValidationConstants.CANNED_RESPONSE_CHECKLIST_VERSION)
                            {
                                if (row.PricerResponse == ChecklistResponseOption.No && row.SelectedCannedResponse == null && !row.YesOnly)
                                {
                                    ++numMissedCannedResponses;
                                }

                                if (row.YesOnly && row.PricerResponse != ChecklistResponseOption.Yes)
                                {
                                    ++numMissedYesResponses;
                                }
                                
                                if (row.SelectedCannedResponse == ValidationConstants.ChecklistValidationConstants.OTHER_CANNED_RESPONSE_ID && string.IsNullOrEmpty(row.PricerRowComment))
                                {
                                    ++numMissedOtherComments;
                                }
                            }
                        }
                    }

                    if (numMissedPageNumberResponses > 0)
                    {
                        inValidationErrors.Add(new ValidationMessage(ValidationConstants.ChecklistValidationConstants.PAR_ROW_PAGE_NUMBER_NEEDED));
                    }

                    if (numMissedCommentResponses > 0)
                    {
                        inValidationErrors.Add(new ValidationMessage(ValidationConstants.ChecklistValidationConstants.PAR_ROW_COMMENT_NEEDED));
                    }

                    if (numMissedCannedResponses > 0)
                    {
                        inValidationErrors.Add(new ValidationMessage(ValidationConstants.ChecklistValidationConstants.PAR_ROW_CANNED_RESPONSE_NEEDED));
                    }

                    if (numMissedYesResponses > 0)
                    {
                        inValidationErrors.Add(new ValidationMessage(ValidationConstants.ChecklistValidationConstants.PAR_ROW_YES_NEEDED));
                    }

                    if (numMissedOtherComments > 0)
                    {
                        inValidationErrors.Add(new ValidationMessage(ValidationConstants.ChecklistValidationConstants.PAR_ROW_OTHER_COMMENT_NEEDED));
                    }
                }
                else
                {
                    pricerQuestions = from q in checklistPARDocumentData.ChecklistRows
                                      where q.RowType == ChecklistTextType.Question && q.PricerResponse == ChecklistResponseOption.No
                                      select q;

                    string pricerComment = checklistPARDocumentData.PARPricerComment;

                    if (pricerQuestions.Any() && string.IsNullOrEmpty(pricerComment))
                    {
                        inValidationErrors.Add(new ValidationMessage(ValidationConstants.ChecklistValidationConstants.PAR_COMMENT_NEEDED));
                    }
                }
            }
            else if (checklistPARDocumentData.ShowChecklistResponse == ShowChecklistResponse.Peer)
            {
                var peerQuestions = from q in checklistPARDocumentData.ChecklistRows
                                    where q.RowType == ChecklistTextType.Question && q.PeerResponse == ChecklistResponseOption.NotSet
                                    select q;
                if (peerQuestions.Any())
                {
                    inValidationErrors.Add(new ValidationMessage(ValidationConstants.ChecklistValidationConstants.ALL_PAR_MUST_BE_ANSWERED));
                }

                // Verify that all row comments have been populated (checklist format Jan 2015)
                // Else, perform the legacy validation which requires a Peer comment at the bottom of the PAR checklist
                if (this.ShowComment(checklistPARDocumentData.ChecklistVersion))
                {
                    int numMissedCommentResponses = 0;
                    foreach (ChecklistRowModelView row in checklistPARDocumentData.ChecklistRows)
                    {
                        if (row.RowType == ChecklistTextType.Question)
                        {
                            if (row.PeerResponse == ChecklistResponseOption.No)
                            {
                                if (string.IsNullOrEmpty(row.PeerRowComment))
                                {
                                    ++numMissedCommentResponses;
                                }
                            }
                        }
                    }

                    if (numMissedCommentResponses > 0)
                    {
                        inValidationErrors.Add(new ValidationMessage(ValidationConstants.ChecklistValidationConstants.PAR_ROW_COMMENT_NEEDED));
                    }
                }
                else
                {
                    peerQuestions = from q in checklistPARDocumentData.ChecklistRows
                                    where q.RowType == ChecklistTextType.Question && q.PeerResponse == ChecklistResponseOption.No
                                    select q;

                    string peerComment = checklistPARDocumentData.PARPeerComment;
                    if (peerQuestions.Any() && string.IsNullOrEmpty(peerComment))
                    {
                        inValidationErrors.Add(new ValidationMessage(ValidationConstants.ChecklistValidationConstants.PAR_COMMENT_NEEDED));
                    }
                }
            }
        }

        /// <summary>
        /// Retrieve the ChecklistRowCommentVersion from the Web.config file.  If it is greater than or equal to the
        /// current checklist version for this proposal return true, else return false.
        /// </summary>
        /// <param name="checklistVersion">checklist version for the checklist</param>
        /// <returns>is ChecklistRowComment Version >= current checklist version</returns>
        protected bool ShowComment(int? checklistVersion)
        {
            bool showPricerCommentColumn = false;
            int minCommentColumnVersion = Convert.ToInt32(WebConfigurationManager.AppSettings[Constants.CHECKLIST_ROW_COMMENT_VERSION]);
            if ((checklistVersion != null) && (checklistVersion >= minCommentColumnVersion))
            {
                showPricerCommentColumn = true;
            }

            return showPricerCommentColumn;
        }

        /// <summary>
        /// Checks if the Estimator has saved since the Peer began editing the checklist.
        /// </summary>
        /// <param name="checklistGeneralInfo">checklist general information</param>
        /// <param name="checklistPARDocumentData">The PAR data.</param>
        /// <returns>True if the Estimator has saved, false otherwise.</returns>
        public bool HasPricerSavedSincePeerReviewOpenedChecklist(ChecklistGeneralInformationModelView checklistGeneralInfo, ChecklistProposalAdequacyReviewDocumentModelView checklistPARDocumentData)
        {
            if (checklistGeneralInfo == null)
            {
                throw new ArgumentNullException(nameof(checklistGeneralInfo));
            }

            if (checklistPARDocumentData == null)
            {
                throw new ArgumentNullException(nameof(checklistPARDocumentData));
            }

            bool pricerSaved = false;

            // Only do the check if this checklist is for the Peer.
            if (checklistGeneralInfo.ShowChecklistResponse == ShowChecklistResponse.Peer)
            {
                // Get the Estimator last saved date coming back from the UI of the Peer Reviewer.
                DateTime pricerLastSavedDate = string.IsNullOrEmpty(checklistPARDocumentData.PricerLastSavedDate) ? new DateTime() : Convert.ToDateTime(checklistPARDocumentData.PricerLastSavedDate);

                // Get the Estimator last saved date that is currently stored in the DB.
                ChecklistProposalPricingReviewDocumentModelView savedPPRChecklistData = this.GetDataForChecklistPPRDocument(checklistPARDocumentData.ProposalID);
                DateTime savedPricerLastSavedDate = string.IsNullOrEmpty(savedPPRChecklistData.LastSavedDate) ? new DateTime() : Convert.ToDateTime(savedPPRChecklistData.LastSavedDate);

                // If the last saved date on the UI is older than the last saved date for the Pricer in the DB, then we know that the 
                // Estimator has saved since the Peer reviewer opened the checklist.
                if (pricerLastSavedDate < savedPricerLastSavedDate)
                {
                    pricerSaved = true;
                }
            }

            return pricerSaved;
        }

        /// <summary>
        /// get data for PPR checklist
        /// </summary>
        /// <param name="proposalId">proposal id</param>
        /// <returns>PPR document model view</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "checklistContent")]
        public ChecklistProposalPricingReviewDocumentModelView GetDataForChecklistPPRDocument(int proposalId)
        {
            ChecklistProposalPricingReviewDocumentModelView model = new ChecklistProposalPricingReviewDocumentModelView();

            // set which role is trying to view general info
            if (this.SecurityAccess.CurrentUserHasRole(PtmRole.Pricer, proposalId) || this.SecurityAccess.CurrentUserHasRole(PtmRole.BackupPricer, proposalId))
            {
                model.ShowChecklistResponse = ShowChecklistResponse.Pricer;
            }
            else if (this.SecurityAccess.CurrentUserHasRole(PtmRole.PeerReviewer, proposalId))
            {
                model.ShowChecklistResponse = ShowChecklistResponse.Peer;
            }
            else
            {
                // all other roles get show both
                model.ShowChecklistResponse = ShowChecklistResponse.ShowBoth;
            }

            FullProposal fullProposalDto = this.GetFullProposalDto(proposalId);
            model.IsPTMChecklistUIEnabled = (fullProposalDto.ProposalChecklistPPRData == null) ? true : this.IsPTMChecklistUIEnabled(fullProposalDto.ProposalChecklistPPRData.Version);
            // if this is a PTM checklist, only show Estimator (Pricer) response column.
            if (model.IsPTMChecklistUIEnabled.HasValue && model.IsPTMChecklistUIEnabled.Value)
            {
                model.ShowChecklistResponse = ShowChecklistResponse.Pricer;
            }

            model.ShouldDisplayPPRSection = this.ShouldChecklistPPRSectionBeDisplayed(fullProposalDto).ToString().ToLower();
            model.IsReadOnly = this.IsProposalChecklistReadOnly(fullProposalDto);

            model.ProposalID = fullProposalDto.Id;

            // even though a collection is returned, we know that one proposal can only contain one of these dtos
            var pprData = fullProposalDto.ProposalChecklistPPRData;
            var proposalChecklist = fullProposalDto.ProposalChecklistData;
            ProposalChecklistSaveInfo pricerSaveInfo = null;
            if (pprData != null)
            {
                if (proposalChecklist != null && proposalChecklist.Any())
                {
                    var generalChecklistData = proposalChecklist.First();
                    model.ProposalChecklistID = generalChecklistData.Id;
                    model.ProposalID = generalChecklistData.ProposalID;
                    model.UpdateDate = generalChecklistData.UpdateDate;

                    if (generalChecklistData.UserSaveInfo[ChecklistResponseType.Pricer].TryGetValue(ChecklistType.ProposalPricingReview, out pricerSaveInfo))
                    {
                        model.PPRPricerComment = pricerSaveInfo.Comment == null ? string.Empty : pricerSaveInfo.Comment;
                    }
                }

                ICollection<ChecklistRowModelView> checklistRows = new Collection<ChecklistRowModelView>();

                ChecklistContentDto checklistContent = pprData;
                ICollection<ChecklistContentItem> sortedChecklistContent = checklistContent.Content.OrderBy(x => x.SortOrder).ToList();

                ICollection<ChecklistResponseItem> propCheck = new Collection<ChecklistResponseItem>();

                // if proposal checklist exists, get PPR responses from the DTO
                if (proposalChecklist != null && proposalChecklist.Any())
                {
                    propCheck = proposalChecklist.First().PPRResponses;
                }
                else
                {
                    propCheck = this.proposalChecklistLoader.GetPPRResponses(proposalId);
                }

                // get content and associate responses with each content
                foreach (var content in sortedChecklistContent)
                {
                    ChecklistRowModelView row = new ChecklistRowModelView();
                    row.RowType = content.TextType;
                    row.DisplayText = content.Text;
                    row.ChecklistContentId = content.Id;
                    row.SortOrder = content.SortOrder;
                    row.ColumnOrder = content.ColumnOrder;
                    row.SubmissionItem = content.SubmissionItem;

                    var responses = (from p in propCheck
                                     where p.ChecklistContentId == content.Id
                                     select p).ToList();

                    row.PricerResponse = (from r in responses
                                          select r.Response).FirstOrDefault();
                    checklistRows.Add(row);
                }

                model.ChecklistRows = checklistRows;
                model.Question1SortOrder = model.ChecklistRows.Where(x => x.RowType == ChecklistTextType.Question).Select(x => x.SortOrder).Min();
            }

            return model;
        }

        /// <summary>
        /// Determines whether checklist is read only based on proposal state and current user
        /// </summary>
        /// <param name="fullProposalDto">Full proposal</param>
        /// <returns>"true" if readonly, "false" if editable</returns>
        internal string IsProposalChecklistReadOnly(FullProposal fullProposalDto)
        {
            bool readOnly = true;

            // if this proposal is not in progress, or it is in progress and the Lead Estimator has approved the proposal,
            // the entire checklist is read only so just return that value now
            if (fullProposalDto.ProposalStatus != ProposalStatus.InProgress || fullProposalDto.LeadEstimatorSignedDate != null)
            {
                return "true";
            }

            // The checklist can only be edited by a Lead Estimator or Backup Estimator.
            if (this.IsCurrentUserPricerOrBackupEstimator(fullProposalDto))
            {
                // Form is only editable if it has not been submitted by the Pricer.
                readOnly = fullProposalDto.ProposalChecklistSaveInfo.Where(x => x.ResponseType == ChecklistResponseType.Pricer && x.SubmitDate != null).Any();
            }

            return readOnly.ToString().ToLower();
        }

        /// <summary>
        /// Determines whether or not the current user has at least one of the Pricer or Backup Estimator roles.
        /// </summary>
        /// <param name="proposalId">Proposal ID</param>
        /// <returns>True if the user has at least one of the roles, false otherwise.</returns>
        public bool IsCurrentUserPricerOrBackupEstimator(int proposalId)
        {
            FullProposal fullProposalDto = this.GetFullProposalDto(proposalId);
            return this.IsCurrentUserPricerOrBackupEstimator(fullProposalDto);
        }

        /// <summary>
        /// Determines whether or not the current user has at least one of the Pricer or Backup Estimator roles.
        /// </summary>
        /// <param name="fullProposalDto">Full Proposal</param>
        /// <returns>True if the user has at least one of the roles, false otherwise.</returns>
        internal bool IsCurrentUserPricerOrBackupEstimator(FullProposal fullProposalDto)
        {
            if (fullProposalDto != null)
            {
                return this.SecurityAccess.CurrentUserHasRole(PtmRole.Pricer, fullProposalDto.Id) || this.SecurityAccess.CurrentUserHasRole(PtmRole.BackupPricer, fullProposalDto.Id);
            }

            return false;
        }

        /// <summary>
        /// Save checklist
        /// </summary>
        /// <param name="checklistGeneralInfo">checklist general info</param>
        /// <param name="checklistProposalPricingData">checklist proposal pricing data</param>
        /// <param name="checklistPPRDocumentData">checklist PPR document data</param>
        /// <param name="checklistPARDocumentData">checklist PAR document data</param>
        /// <param name="isSubmit">true if submit, false if save</param>
        /// <returns>checklist ID</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "checklistProposalPricingData"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "isSubmit"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "checklistPPRDocumentData"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "checklistPARDocumentData"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "checklistGeneralInfo")]
        public int? SaveChecklist(ChecklistGeneralInformationModelView checklistGeneralInfo, ChecklistProposalPricingDataModelView checklistProposalPricingData, ChecklistProposalPricingReviewDocumentModelView checklistPPRDocumentData, ChecklistProposalAdequacyReviewDocumentModelView checklistPARDocumentData, bool isSubmit)
        {
            if (checklistGeneralInfo == null)
            {
                throw new ArgumentNullException(nameof(checklistGeneralInfo));
            }

            if (checklistProposalPricingData == null)
            {
                throw new ArgumentNullException(nameof(checklistProposalPricingData));
            }

            if (checklistPPRDocumentData == null)
            {
                throw new ArgumentNullException(nameof(checklistPPRDocumentData));
            }

            if (checklistPARDocumentData == null)
            {
                throw new ArgumentNullException(nameof(checklistPARDocumentData));
            }

            using (IES.Common.StopwatchTimer sw = new IES.Common.StopwatchTimer("GenTRACControllerLogic.SaveChecklist", this.logger))
            {
                ProposalChecklistDto checklist = new ProposalChecklistDto()
                {
                    Id = checklistGeneralInfo.ProposalChecklistID,
                    Updateable = IES.Common.UpdateType.Upsert,
                    UpdateDate = checklistGeneralInfo.UpdateDate,
                    ProposalID = checklistGeneralInfo.ProposalID,
                    EstimatingSubmitsToContractsDate = string.IsNullOrEmpty(checklistGeneralInfo.EstimatingSubmitsToContractsDate) ? (DateTime?)null : checklistGeneralInfo.EstimatingSubmitsToContractsDate.ToDateTime("MM/dd/yyyy"),
                    SubmittedValue = !string.IsNullOrEmpty(checklistGeneralInfo.SubmittedValue) ? long.Parse(checklistGeneralInfo.SubmittedValue.Replace(",", string.Empty)) : (long?)null,
                    AbsoluteValue = !string.IsNullOrEmpty(checklistGeneralInfo.AbsoluteValue) ? long.Parse(checklistGeneralInfo.AbsoluteValue.Replace(",", string.Empty)) : (long?)null,
                    CostThroughCom = !string.IsNullOrEmpty(checklistProposalPricingData.CostThroughCom) ? long.Parse(checklistProposalPricingData.CostThroughCom.Replace(",", string.Empty)) : (long?)null,
                    ProfitFeeWithCom = !string.IsNullOrEmpty(checklistProposalPricingData.ProfitFeeComTotal) ? long.Parse(checklistProposalPricingData.ProfitFeeComTotal.Replace(",", string.Empty)) : (long?)null,
                    Profit = !string.IsNullOrEmpty(checklistProposalPricingData.ProfitFeeTotal) ? long.Parse(checklistProposalPricingData.ProfitFeeTotal.Replace(",", string.Empty)) : (long?)null,
                    Com = !string.IsNullOrEmpty(checklistProposalPricingData.ComTotal) ? long.Parse(checklistProposalPricingData.ComTotal.Replace(",", string.Empty)) : (long?)null,
                    ROSPercentage = string.IsNullOrEmpty(checklistProposalPricingData.ROSPercent) ? (decimal?)null : Convert.ToDecimal(checklistProposalPricingData.ROSPercent),
                    LMLaborHrs = string.IsNullOrEmpty(checklistProposalPricingData.LMLaborHrs) ? (decimal?)null : Convert.ToDecimal(checklistProposalPricingData.LMLaborHrs),
                    LMLaborCost = string.IsNullOrEmpty(checklistProposalPricingData.LMLaborCost) ? (long?)null : long.Parse(checklistProposalPricingData.LMLaborCost.Replace(",", string.Empty)),
                    SubcontractorCost = string.IsNullOrEmpty(checklistProposalPricingData.SubcontractorCost) ? (long?)null : long.Parse(checklistProposalPricingData.SubcontractorCost.Replace(",", string.Empty)),
                    MaterialCost = string.IsNullOrEmpty(checklistProposalPricingData.MaterialCost) ? (long?)null : long.Parse(checklistProposalPricingData.MaterialCost.Replace(",", string.Empty)),
                    IWTACost = string.IsNullOrEmpty(checklistProposalPricingData.IWTACost) ? (long?)null : long.Parse(checklistProposalPricingData.IWTACost.Replace(",", string.Empty)),
                    TravelCost = string.IsNullOrEmpty(checklistProposalPricingData.TravelCost) ? (long?)null : long.Parse(checklistProposalPricingData.TravelCost.Replace(",", string.Empty)),
                    OtherDirectCosts = string.IsNullOrEmpty(checklistProposalPricingData.OtherDirectCosts) ? (long?)null : long.Parse(checklistProposalPricingData.OtherDirectCosts.Replace(",", string.Empty)),
                    IsSubmit = isSubmit,
                    ResponseType = checklistPARDocumentData.ShowChecklistResponse == ShowChecklistResponse.Pricer ? ChecklistResponseType.Pricer : ChecklistResponseType.Peer,
                    DeliverChecklistDFARS = checklistGeneralInfo.DeliverChecklistDFARS
                };

                // Save PPR responses
                Collection<ChecklistResponseItem> pprResponses = new Collection<ChecklistResponseItem>();
                foreach (ChecklistRowModelView row in checklistPPRDocumentData.ChecklistRows)
                {
                    if (row.RowType == ChecklistTextType.Question)
                    {
                        ChecklistResponseItem response = new ChecklistResponseItem();
                        response.ChecklistContentId = row.ChecklistContentId;
                        response.ChecklistType = ChecklistType.ProposalPricingReview;
                        response.ResponseType = ChecklistResponseType.Pricer;
                        response.Response = row.PricerResponse;
                        pprResponses.Add(response);
                    }
                }

                checklist.PPRResponses = pprResponses;

                // Save PAR and PPR Pricer comments
                if (checklistGeneralInfo.PricerId > 0)
                {
                    ProposalChecklistSaveInfo pricerSaveInfo = new ProposalChecklistSaveInfo { Comment = checklistPPRDocumentData.PPRPricerComment, UserID = checklistGeneralInfo.PricerId };
                    checklist.UserSaveInfo[ChecklistResponseType.Pricer].Add(ChecklistType.ProposalPricingReview, pricerSaveInfo);

                    pricerSaveInfo = new ProposalChecklistSaveInfo { Comment = checklistPARDocumentData.PARPricerComment, UserID = checklistGeneralInfo.PricerId };
                    checklist.UserSaveInfo[ChecklistResponseType.Pricer].Add(ChecklistType.ProposalAdequacyReview, pricerSaveInfo);
                }

                // gather PAR row responses
                Collection<ChecklistResponseItem> parResponses = new Collection<ChecklistResponseItem>();
                foreach (ChecklistRowModelView row in checklistPARDocumentData.ChecklistRows)
                {
                    if (row.RowType == ChecklistTextType.Question)
                    {
                        ChecklistResponseItem pricerResponse = new ChecklistResponseItem();
                        ChecklistResponseItem peerResponse = new ChecklistResponseItem();

                        // get pricer response first and add it to the collection
                        pricerResponse.ChecklistContentId = row.ChecklistContentId;
                        pricerResponse.ChecklistType = ChecklistType.ProposalAdequacyReview;
                        pricerResponse.ResponseType = ChecklistResponseType.Pricer;
                        pricerResponse.Response = row.PricerResponse;
                        pricerResponse.PricerPageNumber = row.PricerPageNumber;
                        pricerResponse.RowComment = row.PricerRowComment;
                        if(checklistPARDocumentData.ChecklistVersion >= ValidationConstants.ChecklistValidationConstants.CANNED_RESPONSE_CHECKLIST_VERSION)
                        {
                            // If 'Other' Selected, set to null since id is not in the db and should not be saved
                            pricerResponse.CannedResponseId = row.SelectedCannedResponse == ValidationConstants.ChecklistValidationConstants.OTHER_CANNED_RESPONSE_ID 
                                ? null 
                                : row.SelectedCannedResponse;
                        }

                        parResponses.Add(pricerResponse);

                        // now get peer response and add it to the collection                     
                        peerResponse.ChecklistContentId = pricerResponse.ChecklistContentId;
                        peerResponse.ChecklistType = pricerResponse.ChecklistType;
                        peerResponse.ResponseType = ChecklistResponseType.Peer;
                        peerResponse.Response = row.PeerResponse;
                        peerResponse.RowComment = row.PeerRowComment;
                        parResponses.Add(peerResponse);
                    }
                }

                checklist.PARResponses = parResponses;

                // If this is a submission and the user is the LOB Est Lead, manually set the ResponseType to Pricer to prevent issues with saving.
                // Issues arise when the Checklist is already locked and we attempt to save it - a null is returned and the IDs do not match.
                checklist.ResponseType = checklist.IsSubmit && this.SecurityAccess.CurrentUserHasRole(PtmRole.LOBEstLead, checklist.ProposalID) 
                    ? ChecklistResponseType.Pricer : checklist.ResponseType;

                // save the checklist
                int? proposalchecklistID = this.ChecklistMediator.SaveChecklistProposal(checklist);

                // determine if proposal status needs to be changed to completed
                if (checklist.IsSubmit)
                {
                    // get the dto again to get latest update date
                    FullProposal fullProposalDto = this.GetFullProposalDto(checklist.ProposalID);

                    ProposalStatus newStatus = ProposalStatus.PendingAward;
                    if (fullProposalDto.IsCCPDRequired.HasValue && fullProposalDto.IsCCPDRequired.Value)
                    {
                        newStatus = ProposalStatus.PendingCertification;
                    }

                    this.ProposalMediator.SaveProposalStatus(checklist.ProposalID, fullProposalDto.UpdateDate, newStatus);
                }

                return proposalchecklistID;
            }
        }

        /// <summary>
        /// Retrieve the ChecklistChangeToPTMVersion from the Web.config file.
        /// Return true, if current checklist version for this proposal >= Web.config value; false otherwise.
        /// 
        /// Notes: BOEJ-1768 and BOEJ-1773 introduced a new version of PPR and PAR checklist questions. 
        /// More importantly, it marked the beginning of a change to the UI. 
        /// Going forward, the PPR and PAR sections are shown/hidden depending 
        /// on how the "Is Certified Cost or Pricing Data Required?" question is answered.
        /// (In previous versions, question 1 of the PPR determined whether or not to show/hide the PAR section.)
        /// This method is used to determine when to use the old versus new UI.
        /// </summary>
        /// <param name="checklistVersion">checklist version for the checklist</param>
        /// <returns>is checklist version >= ChecklistChangeToPTMVersion version</returns>
        public bool IsPTMChecklistUIEnabled(int? checklistVersion)
        {
            bool toReturn = true;   // Default to use new (PTM) Checklist User Interface
            int minChecklistChangeToPTMVersion = Convert.ToInt32(WebConfigurationManager.AppSettings[Constants.CHECKLIST_CHANGE_TO_PTM_VERSION]);
            if ((checklistVersion != null) && (checklistVersion < minChecklistChangeToPTMVersion))
            {
                toReturn = false;    // Use old (GenTRAC) Checklist User Interface
            }

            return toReturn;
        }

        /// <summary>
        /// determines if the PAR checklist section should be displayed based on PPR's Question 1 response (legacy checklist versions) or 
        /// "Is Certified Cost or Pricing Data Required?" response (PTM checklist versions)
        /// </summary>
        /// <param name="inProposalId">proposal id</param>
        /// <param name="fullProposal">Full proposal</param>
        /// <returns>true if should be displayed, false is should be hidden</returns>
        public bool ShouldChecklistPARSectionBeDisplayed(int inProposalId, FullProposal fullProposal)
        {
            if (fullProposal == null)
            {
                throw new ArgumentNullException(nameof(fullProposal));
            }

            bool toReturn = true;

            if (fullProposal.ProposalChecklistPPRData != null)
            {
                if (this.IsPTMChecklistUIEnabled(fullProposal.ProposalChecklistPPRData.Version))
                {
                    // show/hide PAR checklist based on "Is Certified Cost or Pricing Data Required?" response
                    if (fullProposal.IsCCPDRequired.HasValue)
                    {
                        toReturn = fullProposal.IsCCPDRequired.Value;
                    }
                }
                else
                {
                    // show/hide PAR checklist based on PPR's Question 1 response
                    ChecklistResponseOption response = this.GetPPRQuestion1Response(inProposalId, fullProposal);

                    if (response == ChecklistResponseOption.Yes)
                    {
                        toReturn = false;
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Get view for checklist general information
        /// </summary>
        /// <param name="proposalId">Proposal Id.</param>
        /// <returns>Checklist General Information Model View</returns>
        public ChecklistGeneralInformationModelView GetDataForChecklistGeneralInformation(int proposalId)
        {
            ChecklistGeneralInformationModelView model = new ChecklistGeneralInformationModelView();

            // set which role is trying to view general info
            if (this.SecurityAccess.CurrentUserHasRole(PtmRole.Pricer, proposalId) || this.SecurityAccess.CurrentUserHasRole(PtmRole.BackupPricer, proposalId))
            {
                model.ShowChecklistResponse = ShowChecklistResponse.Pricer;
            }
            else if (this.SecurityAccess.CurrentUserHasRole(PtmRole.PeerReviewer, proposalId))
            {
                model.ShowChecklistResponse = ShowChecklistResponse.Peer;
            }
            else
            {
                // all other roles get show both
                model.ShowChecklistResponse = ShowChecklistResponse.ShowBoth;
            }

            FullProposal fullProposalDto = this.GetFullProposalDto(proposalId);

            model.IsReadOnly = this.IsProposalChecklistReadOnly(fullProposalDto);
            model.ProposalID = fullProposalDto.Id;
            // get pricer info
            int pricerUserID = fullProposalDto.Permissions.Where(x => x.Role.Equals(PtmRole.Pricer)).Select(y => y.UserId).FirstOrDefault();
            model.PricerDisplayName = this.UserMapper.GetById(pricerUserID).DisplayName;
            model.PricerId = pricerUserID;
            model.ShowDFARQuestion = this.ShowComment(fullProposalDto.ProposalChecklistPPRData.Version) && this.ShouldChecklistPARSectionBeDisplayed(proposalId, fullProposalDto) && fullProposalDto.ProposalChecklistPARData.ChecklistType != ProposalChecklistType.InternationalCommercial;
            model.IsPTMChecklistUIEnabled = (fullProposalDto.ProposalChecklistPPRData == null) ? true : this.IsPTMChecklistUIEnabled(fullProposalDto.ProposalChecklistPPRData.Version);
            // if this is a PTM checklist, hide DFAR Question, and only show Estimator (Pricer) response column.
            if (model.IsPTMChecklistUIEnabled.HasValue && model.IsPTMChecklistUIEnabled.Value)
            {
                model.DeliverChecklistDFARS = true;
                model.ShowDFARQuestion = false;
                model.ShowChecklistResponse = ShowChecklistResponse.Pricer;
            }

            // even though a collection is returned, we know that one proposal can only contain one of these dtos
            var checklists = fullProposalDto.ProposalChecklistData;

            if (checklists != null && checklists.Any())
            {
                // there is really only one checklist per proposal so just grab it
                ProposalChecklistDto checklist = checklists.First();
                model.ProposalChecklistID = checklist.Id;

                if (checklist.EstimatingSubmitsToContractsDate.HasValue)
                {
                    model.EstimatingSubmitsToContractsDate = checklist.EstimatingSubmitsToContractsDate.Value.ToString("MM/dd/yyyy");
                }

                // submitted value is SSC total price
                model.SubmittedValue = checklist.SubmittedValue.ToString();
                model.AbsoluteValue = checklist.AbsoluteValue.ToString();
                
                model.UpdateDate = checklist.UpdateDate;
                model.DeliverChecklistDFARS = checklist.DeliverChecklistDFARS;
            }

            return model;
        }

        /// <summary>
        /// Get view for checklist proposal pricing data
        /// </summary>
        /// <param name="proposalId">Proposal Id.</param>
        /// <returns>Checklist Proposal Pricing Data Model View</returns>
        public ChecklistProposalPricingDataModelView GetDataForChecklistProposalPricingData(int proposalId)
        {
            FullProposal proposal = this.GetFullProposalDto(proposalId);
            ChecklistProposalPricingDataModelView model = new ChecklistProposalPricingDataModelView()
            {
                ProposalID = proposal.Id,
                SplitProfitFeeCOM = (proposal.DateCreated ?? DateTime.Now) >= ConfigurationUtilities.GetAppSetting<DateTime>("ProfitFeeComSplitStartDate"),
                UseCostThroughCom = (proposal.DateCreated ?? DateTime.Now) >= ConfigurationUtilities.GetAppSetting<DateTime>("CostThroughComStartDate"),
                IsReadOnly = this.IsProposalChecklistReadOnly(proposal),
                ShowChecklistResponse = this.GetShowChecklistResponse(proposal)
            };

            if (proposal.ProposalChecklistData != null && proposal.ProposalChecklistData.Any())
            {
                // there is really only one checklist per proposal so just grab it
                ProposalChecklistDto checklist = proposal.ProposalChecklistData.First();
                model.ProposalChecklistID = checklist.Id;
                model.LMLaborHrs = string.Format("{0:#,###0.##}", checklist.LMLaborHrs);
                model.LMLaborCost = string.Format("{0:#,###0}", checklist.LMLaborCost);
                model.SubcontractorCost = string.Format("{0:#,###0}", checklist.SubcontractorCost);
                model.MaterialCost = string.Format("{0:#,###0}", checklist.MaterialCost);
                model.IWTACost = string.Format("{0:#,###0}", checklist.IWTACost);
                model.TravelCost = string.Format("{0:#,###0}", checklist.TravelCost);
                model.OtherDirectCosts = string.Format("{0:#,###0}", checklist.OtherDirectCosts);
                model.CostThroughCom = string.Format("{0:#,###0}", checklist.CostThroughCom);
                model.ProfitFeeComTotal = string.Format("{0:#,###0}", checklist.ProfitFeeWithCom);
                model.ProfitFeeTotal = string.Format("{0:#,###0}", checklist.Profit);
                model.ComTotal = string.Format("{0:#,###0}", checklist.Com);
                model.SubmittedValue = string.Format("{0:#,###0}", checklist.SubmittedValue);
                model.AbsoluteValue = string.Format("{0:#,###0}", checklist.AbsoluteValue);
                model.ROSPercent = checklist.ROSPercentage.ToString();
                model.UpdateDate = checklist.UpdateDate;
            }

            return model;
        }

        /// <summary>
        /// Gets the value for the ShowChecklistResponse field, for ChecklistProposalPricingDataModelView
        /// </summary>
        /// <param name="proposal">Proposal</param>
        /// <returns>ChecklistProposalPricingDataModelView.ShowChecklistResponse value</returns>
        private ShowChecklistResponse GetShowChecklistResponse(FullProposal proposal)
        {
            ShowChecklistResponse result = ShowChecklistResponse.ShowBoth;

            // set which role is trying to view general info
            if (this.SecurityAccess.CurrentUserHasRole(PtmRole.Pricer, proposal.Id) 
                || this.SecurityAccess.CurrentUserHasRole(PtmRole.BackupPricer, proposal.Id) 
                || (proposal.ProposalChecklistPPRData == null) ? true : this.IsPTMChecklistUIEnabled(proposal.ProposalChecklistPPRData.Version))
            {
                result = ShowChecklistResponse.Pricer;
            }
            else if (this.SecurityAccess.CurrentUserHasRole(PtmRole.PeerReviewer, proposal.Id))
            {
                result = ShowChecklistResponse.Peer;
            }

            return result;
        }

        /// <summary>
        /// get data for PAR checklist
        /// </summary>
        /// <param name="proposalId">proposal ID</param>
        /// <param name="baseUrl">Base url for additional information</param>
        /// <returns>PAR document model view</returns>
        public ChecklistProposalAdequacyReviewDocumentModelView GetDataForChecklistPARDocument(int proposalId, string baseUrl)
        {
            ChecklistProposalAdequacyReviewDocumentModelView model = new ChecklistProposalAdequacyReviewDocumentModelView();

            if (this.SecurityAccess.CurrentUserHasRole(PtmRole.Pricer, proposalId) || this.SecurityAccess.CurrentUserHasRole(PtmRole.BackupPricer, proposalId))
            {
                model.ShowChecklistResponse = ShowChecklistResponse.Pricer;
            }
            else if (this.SecurityAccess.CurrentUserHasRole(PtmRole.PeerReviewer, proposalId))
            {
                model.ShowChecklistResponse = ShowChecklistResponse.Peer;
            }
            else
            {
                // all other roles get show both
                model.ShowChecklistResponse = ShowChecklistResponse.ShowBoth;
            }

            model.ProposalID = proposalId;

            FullProposal fullProposalDto = this.GetFullProposalDto(proposalId);
            model.ShouldDisplayPARSection = this.ShouldChecklistPARSectionBeDisplayed(proposalId, fullProposalDto).ToString().ToLower();
            model.IsReadOnly = this.IsProposalChecklistReadOnly(fullProposalDto);
            model.ProposalID = fullProposalDto.Id;

            // even though a collection is returned, we know that one proposal can only contain one of these dtos
            ChecklistContentDto parData = fullProposalDto.ProposalChecklistPARData;         // ChecklistContentDto
            model.ChecklistVersion = parData.Version;
            model.ProposalChecklistType = parData.ChecklistType;
            model.ShowComment = this.ShowComment(parData.Version);
            ICollection<ProposalChecklistDto> proposalChecklist = fullProposalDto.ProposalChecklistData;  // ICollection<ProposalChecklistDto>

            ProposalChecklistSaveInfo pricerSaveInfo = null;
            ProposalChecklistSaveInfo peerReviewerSaveInfo = null;

            // if proposal status != in progress, the PAR document should show both the pricer and peer responses (regardlesss if you are pricer or peer)
            if (fullProposalDto.ProposalStatus != ProposalStatus.InProgress)
            {
                model.ShowChecklistResponse = ShowChecklistResponse.ShowBoth;
            }

            if (parData != null)
            {
                if (proposalChecklist != null && proposalChecklist.Any())
                {
                    var generalChecklistData = proposalChecklist.First();
                    model.ProposalChecklistID = generalChecklistData.Id;
                    model.ProposalID = generalChecklistData.ProposalID;
                    model.UpdateDate = generalChecklistData.UpdateDate;
                    model.ShowNA = !model.ShowComment || (model.ShowComment && !generalChecklistData.DeliverChecklistDFARS);
                    if (generalChecklistData.UserSaveInfo[ChecklistResponseType.Pricer].TryGetValue(ChecklistType.ProposalAdequacyReview, out pricerSaveInfo))
                    {
                        model.PricerLastSavedDate = pricerSaveInfo.LastSaveDate.HasValue ? pricerSaveInfo.LastSaveDate.Value.ToString("MM/dd/yyyy h:mm:ss tt") : string.Empty;
                        model.PARPricerComment = pricerSaveInfo.Comment == null ? string.Empty : pricerSaveInfo.Comment;
                    }

                    if (generalChecklistData.UserSaveInfo[ChecklistResponseType.Peer].TryGetValue(ChecklistType.ProposalAdequacyReview, out peerReviewerSaveInfo))
                    {
                        model.PARPeerComment = peerReviewerSaveInfo.Comment == null ? string.Empty : peerReviewerSaveInfo.Comment;
                    }
                }

                ChecklistContentDto checklistContent = parData;
                model.ProposalAdequacyReviewID = checklistContent.ProposalAdequacyReviewID;
                ICollection<ChecklistContentItem> sortedChecklistContent = checklistContent.Content.OrderBy(x => x.SortOrder).ToList();

                model.ChecklistRows = this.AssociateChecklistContentAndResponses(proposalChecklist, proposalId, sortedChecklistContent, model.ChecklistVersion.Value);
            }

            // if this is a PTM checklist, set deliverChecklistDFARS to true, hide the N/A column, and only show Estimator (Pricer) response column.
            model.IsPTMChecklistUIEnabled = (fullProposalDto.ProposalChecklistPARData == null) ? true : this.IsPTMChecklistUIEnabled(fullProposalDto.ProposalChecklistPARData.Version);
            if (model.IsPTMChecklistUIEnabled.HasValue && model.IsPTMChecklistUIEnabled.Value)
            {
                model.ShowNA = false;
                model.ShowChecklistResponse = ShowChecklistResponse.Pricer;
            }

            model.ShowExportButton = this.DisplayExportButton(proposalId, model.ShowNA, fullProposalDto);

            // set column spans
            if (model.ShowChecklistResponse == ShowChecklistResponse.ShowBoth)
            {
                model.SecondHeaderColSpan = model.ProposalChecklistType == ProposalChecklistType.InternationalCommercial ? 8 : (model.ShowComment ? (model.ShowNA ? 10 : 8) : 7);
                model.CommentColSpan = model.ProposalChecklistType == ProposalChecklistType.InternationalCommercial ? 9 : (model.ShowComment ? (model.ShowNA ? 11 : 9) : 7);
                model.TextColSpan = model.ProposalChecklistType == ProposalChecklistType.InternationalCommercial ? 9 : (model.ShowComment ? (model.ShowNA ? 11 : 9) : 7);
                model.DocumentFinalRowColSpan = model.ProposalChecklistType == ProposalChecklistType.InternationalCommercial ? 9 : (model.ShowComment ? (model.ShowNA ? 11 : 9) : 7);
            }
            else if (model.ShowChecklistResponse == ShowChecklistResponse.Pricer)
            {
                model.SecondHeaderColSpan = model.ProposalChecklistType == ProposalChecklistType.InternationalCommercial ? 4 : (model.ShowComment ? (model.ShowNA ? 6 : 5) : 4);
                model.CommentColSpan = model.ProposalChecklistType == ProposalChecklistType.InternationalCommercial ? 5 : (model.ShowComment ? (model.ShowNA ? 7 : 6) : 5);
                model.TextColSpan = model.ProposalChecklistType == ProposalChecklistType.InternationalCommercial ? 5 : (model.ShowComment ? (model.ShowNA ? 7 : 6) : 5);
                model.DocumentFinalRowColSpan = model.ProposalChecklistType == ProposalChecklistType.InternationalCommercial ? 5 : (model.ShowComment ? (model.ShowNA ? 7 : 6) : 5);
            }
            else
            {
                // Peer Values
                model.SecondHeaderColSpan = model.ProposalChecklistType == ProposalChecklistType.InternationalCommercial ? 4 : (model.ShowComment ? (model.ShowNA ? 5 : 4) : 4);
                model.CommentColSpan = model.ProposalChecklistType == ProposalChecklistType.InternationalCommercial ? 5 : (model.ShowComment ? (model.ShowNA ? 6 : 5) : 4);
                model.TextColSpan = model.ProposalChecklistType == ProposalChecklistType.InternationalCommercial ? 5 : (model.ShowComment ? (model.ShowNA ? 6 : 5) : 4);
                model.DocumentFinalRowColSpan = model.ProposalChecklistType == ProposalChecklistType.InternationalCommercial ? 5 : (model.ShowComment ? (model.ShowNA ? 6 : 5) : 4);
            }

            model.RadioResponseColSpan = model.ShowNA ? 3 : 2;
            model.RadioResponseColSpanWidth = this.GetRadioResponseColSpanWidth(model.RadioResponseColSpan, model.ProposalChecklistType);

            model.ChecklistRows.ToList().ForEach(x => { x.DisplayText = x.DisplayText.Replace(BASE_URL_TOKEN, baseUrl); });

            return model;
        }

        /// <summary>
        /// Associates the checklist responses with the content.
        /// </summary>
        /// <param name="proposalChecklist">Proposal Checklist</param>
        /// <param name="proposalId">The proposal Id.</param>
        /// <param name="sortedChecklistContent">Checklist content</param>
        /// <param name="checklistVersion">Checklist Version</param>
        /// <returns> A list of checklist Row Model views.</returns>
        private ICollection<ChecklistRowModelView> AssociateChecklistContentAndResponses(ICollection<ProposalChecklistDto> proposalChecklist, int proposalId, ICollection<ChecklistContentItem> sortedChecklistContent, int checklistVersion)
        {
            ICollection<ChecklistRowModelView> checklistRows = new Collection<ChecklistRowModelView>();
            ICollection<ChecklistResponseItem> propCheck = new Collection<ChecklistResponseItem>();

            // if proposal checklist exists, get PAR responses from the DTO
            if (proposalChecklist != null && proposalChecklist.Any())
            {
                propCheck = proposalChecklist.First().PARResponses;
            }
            else
            {
                propCheck = this.proposalChecklistLoader.GetPARResponses(proposalId);
            }

            // get content and associate responses with each content
            foreach (ChecklistContentItem content in sortedChecklistContent)
            {
                ChecklistRowModelView row = new ChecklistRowModelView();
                row.RowType = content.TextType;
                row.DisplayText = content.Text;
                row.ChecklistContentId = content.Id;
                row.SortOrder = content.SortOrder;
                row.ColumnOrder = content.ColumnOrder;
                row.SubmissionItem = content.SubmissionItem;
                row.YesOnly = content.YesOnly;

                var responses = (from p in propCheck
                                 where p.ChecklistContentId == content.Id
                                 select p).ToList();

                row.PricerResponse = (from r in responses
                                      where r.ResponseType == ChecklistResponseType.Pricer
                                      select r.Response).FirstOrDefault();

                row.PeerResponse = (from r in responses
                                    where r.ResponseType == ChecklistResponseType.Peer
                                    select r.Response).FirstOrDefault();

                row.PricerPageNumber = (from r in responses
                                        where r.ResponseType == ChecklistResponseType.Pricer
                                        select r.PricerPageNumber).FirstOrDefault();

                row.PricerRowComment = (from r in responses
                                        where r.ResponseType == ChecklistResponseType.Pricer
                                        select r.RowComment).FirstOrDefault();

                row.PeerRowComment = (from r in responses
                                      where r.ResponseType == ChecklistResponseType.Peer
                                      select r.RowComment).FirstOrDefault();

                if (checklistVersion >= ValidationConstants.ChecklistValidationConstants.CANNED_RESPONSE_CHECKLIST_VERSION)
                {
                    foreach(var response in content.CannedResponses)
                    {
                        row.CannedResponses.Add(response.Key, response.Value);
                    }
                    
                    // Add Other Response to selection
                    if (row.CannedResponses.Any())
                    {
                        row.CannedResponses.Add(ValidationConstants.ChecklistValidationConstants.OTHER_CANNED_RESPONSE_ID, "Other");
                    }

                    row.SelectedCannedResponse = (from r in responses
                                                  where r.ResponseType == ChecklistResponseType.Pricer
                                                  select r.CannedResponseId).FirstOrDefault();

                    // check for Other selected
                    if (row.SelectedCannedResponse == null && row.PricerResponse == ChecklistResponseOption.No && !string.IsNullOrEmpty(row.PricerRowComment))
                    {
                        row.SelectedCannedResponse = ValidationConstants.ChecklistValidationConstants.OTHER_CANNED_RESPONSE_ID;
                    }
                }

                checklistRows.Add(row);
            }

            return checklistRows;
        }

        /// <summary>
        /// Gets the col span width for the radio response column.
        /// </summary>
        /// <param name="radioResponseColSpan">The col span.</param>
        /// <param name="proposalChecklistType">The proposal Checklist type</param>
        /// <returns>The col span width.</returns>
        private string GetRadioResponseColSpanWidth(int radioResponseColSpan, ProposalChecklistType proposalChecklistType)
        {
            string classWidth;
            switch (radioResponseColSpan)
            {
                case 2:
                    classWidth = proposalChecklistType == ProposalChecklistType.Default ? "YesNoWidthTwo" : "InternationalYesNoWidthTwo";
                    break;
                case 3:
                    classWidth = proposalChecklistType == ProposalChecklistType.Default ? "YesNoWidthThree" : "InternationalYesNoWidthThree";
                    break;
                default:
                    classWidth = proposalChecklistType == ProposalChecklistType.Default ? "YesNoWidthThree" : "InternationalYesNoWidthThree";
                    break;
            }

            return classWidth;
        }

        /// <summary>
        /// determines if the PPR checklist section should be displayed based on "Is Certified Cost or Pricing Data Required?" response
        /// </summary>
        /// <param name="fullProposal">Full proposal</param>
        /// <returns>true if should be displayed, false if "Is Certified Cost or Pricing Data Required?" response is No</returns>
        public bool ShouldChecklistPPRSectionBeDisplayed(FullProposal fullProposal)
        {
            if (fullProposal == null)
            {
                throw new ArgumentNullException(nameof(fullProposal));
            }

            bool toReturn = true;

            if (this.IsPTMChecklistUIEnabled(fullProposal.ProposalChecklistPPRData.Version))
            {
                // show/hide PPR checklist based on "Is Certified Cost or Pricing Data Required?" response
                if (fullProposal.IsCCPDRequired.HasValue)
                {
                    toReturn = fullProposal.IsCCPDRequired.Value;
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Display the Export button for the PAR Checklist
        /// </summary>
        /// <param name="proposalId">proposal in question</param>
        /// <param name="showNA">Show Comment?</param>
        /// <param name="fullProposalDto">full Proposal for this proposal Id</param>
        /// <returns>Display export button true or false?</returns>
        private bool DisplayExportButton(int proposalId, bool showNA, FullProposal fullProposalDto)
        {
            // Display export button?
            bool exportUser = this.IsCurrentUserPricerOrBackupOrSysAdmin(proposalId);

            return exportUser &&
                !showNA &&
                this.ShouldChecklistPARSectionBeDisplayed(proposalId, fullProposalDto);
        }

        /// <summary>
        /// Is current user in lead pricer, backup, or system admin role
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>Does current user have lead pricer, backup, or system admin role</returns>
        protected bool IsCurrentUserPricerOrBackupOrSysAdmin(int proposalId)
        {
            return this.SecurityAccess.CurrentUserHasRole(PtmRole.Pricer, proposalId) ||
                              this.SecurityAccess.CurrentUserHasRole(PtmRole.BackupPricer, proposalId) ||
                              this.SecurityAccess.CurrentUserHasRole(PtmRole.Admin, null);
        }
    }
}
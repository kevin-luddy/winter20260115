// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.Common.Email
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net.Mail;
    using System.Text;
    using GenBOE.ActionLogic.Workspace;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using GenBOE.Objects;
    using IES.Common;

    /// <summary>
    /// The Emailer class will send an email to the recipient and replace any tokens in the subject and body as required.
    /// </summary>
    /// <seealso cref="IES.Common.Emailer" />
    /// <seealso cref="GenBOE.ActionLogic.Common.Email.IBoeEmailer" />
    public class BoeEmailer : Emailer, IBoeEmailer
    {
        /// <summary>
        /// The logger
        /// </summary>
        private Logger logger = new Logger(typeof(BoeEmailer));

        /// <summary>
        /// Gets the common data mapper.
        /// </summary>
        private ICommonDataMapper CommonDataMapper { get; }

        /// <summary>
        /// Gets the security information.
        /// </summary>
        private ISecurityInformation SecurityInformation { get; }
        
        /// <summary>
        /// Gets the user loader.
        /// </summary>
        private IUserDTODataLoader UserLoader { get; }
        
        /// <summary>
        /// Gets the permission loader.
        /// </summary>
        private IPermissionsDTODataLoader PermissionLoader { get; }
        
        /// <summary>
        /// Gets the BOE comment loader.
        /// </summary>
        private IBOECommentDTODataLoader BoeCommentLoader { get; }
        
        /// <summary>
        /// Gets the data fetching scheduler.
        /// </summary>
        private IDataFetchingScheduler DataFetchingScheduler { get; }
        
        /// <summary>
        /// The factory
        /// </summary>
        private IFullObjectFactory factory;
        
        /// <summary>
        /// The BOE loader
        /// </summary>
        private IBoeDTODataLoader boeLoader;
        
        /// <summary>
        /// The workspace loader
        /// </summary>
        private IWorkspaceDTODataLoader workspaceLoader;

        /// <summary>
        /// Gets a value indicating whether all emails are disabled.
        /// </summary>
        private bool DisableAllEmails { get; }

        #region Email Delegates

        private delegate void SendBOEAuthorsChangedDelegate(Collection<UserDTO> inPreviousAuthors, FullBoe inBOE,
            UserData inUserData);

        private delegate void SendBOEUpdatedDelegate(FullBoe inBOE, ICollection<FieldChanged> inFields,
            EmailTypes emailType, UserData inUserData);

        private delegate void SendBOESubmittedForReviewDelegate(FullBoe inBOEForReview, UserData inUserData);

        private delegate void SendBOEAuthorRespondedToCommentDelegate(Collection<BOECommentDTO> inBOEComments,
            UserData inUserData);

        private delegate void SendBOEApproversAuthorApproverApprovedDelegate(BoeApproverResponseDTO inApprover,
            FullBoe inBOE, UserData inUserData);

        private delegate void SendBOEApproversAuthorApproverRejectedDelegate(BoeApproverResponseDTO inRejecter,
            FullBoe inBOE, UserData inUserData);

        private delegate void SendWorkspaceStatusChangeDelegate(FullBoe inBOE, WorkspaceDTO workspace, WbsDTO wbs,
            ClinDTO clin, Collection<UserDTO> approvers, Collection<UserDTO> authors, UserData inUserData, EmailTypes emailType);

        private delegate void SendBOEApproversEmailAwaitingApprovalDelegate(FullBoe inBOEForApproval,
            UserData inUserData);

        private delegate void SendApproverEmailBOEAwaitingApprovalDelegate(FullBoe inBOE, Collection<UserDTO> approvers,
            UserData inUserData);

        private delegate void SendWorkspaceAdminEmailAllBOEsApprovedDelegate(FullBoe inBOE, UserData inUserData);

        private delegate void SendBOEDeletedDelegate(BoeDTO inBOE, UserDTO inDeletedBy, UserData inUserData,
            Collection<int> approverIds, WbsDTO wbsAssociatedWithBoe, ClinDTO clinAssociatedWithBoe,
            FullWorkspace workspace, IDictionary<int, BOEStateModelView> boeStateDictionary);

        private delegate void SendBOEApproversChangedDelegate(Collection<UserDTO> inPreviousApprovers, FullBoe inBOE,
            UserData inUserData);

        private delegate void SendWorkspaceRestoredDelegate(FullWorkspace inWorkspace, DateTime inRestoredFromTime,
            UserData inUserData);

        private delegate void SendBOEAuthorReviewerCommentedDelegate(FullBoe inBoe, int inReviewerID,
            UserData inUserData);

        private delegate void SendBOEAuthorsApproversInUseResourceUpdatedDelegate(ResourceDTO inResource,
            Collection<FieldChanged> inFieldChanges, UserDTO inWorkspaceAdmin, FullWorkspace inWorkspace,
            UserData inUserData);

        private delegate void SendBOEAuthorsApproversDatesUpdatedDelegate(
            HashSet<UserDateChangeInfo> userDateChangeInfo, bool isErrorEmail, UserData inUserData);

        /// <summary>
        /// Send an email to BOE Authors when a WBS or CLIN was changed on a BOE (or a new BOE).
        /// </summary>
        /// <param name="boe">The BOE.</param>
        /// <param name="clinChanged">if set to <c>true</c> [CLIN changed].</param>
        /// <param name="wbsChanged">if set to <c>true</c> [WBS changed].</param>
        /// <param name="isBoeUpdated">True if BOE was updated, false if BOE was created.</param>
        /// <param name="inUserData">The current user's data.</param>
        private delegate void SendBOECLINWBSChangedDelegate(FullBoe boe, bool clinChanged, bool wbsChanged, bool isBoeUpdated, UserData inUserData);

        #endregion Email Delegates

        /// <summary>
        /// Constructor for dependency injection
        /// </summary>
        public BoeEmailer(ICommonDataMapper inCommonDataMapper,
            ISecurityInformation inSecurityInformation,
            IUserDTODataLoader inuserLoader,
            IPermissionsDTODataLoader inPermissionsLoader,
            IBOECommentDTODataLoader inBoeCommentLoader,
            IDataFetchingScheduler inDataFetchingScheduler,
            IFullObjectFactory factory,
            IBoeDTODataLoader boeLoader,
            IWorkspaceDTODataLoader workspaceLoader)
        {
            this.CommonDataMapper = inCommonDataMapper;
            this.SecurityInformation = inSecurityInformation;
            this.UserLoader = inuserLoader;
            this.PermissionLoader = inPermissionsLoader;
            this.BoeCommentLoader = inBoeCommentLoader;
            this.DataFetchingScheduler = inDataFetchingScheduler;
            this.factory = factory;
            this.boeLoader = boeLoader;
            this.workspaceLoader = workspaceLoader;
            this.DisableAllEmails = ConfigurationUtilities.GetAppSetting<bool>("DisableAllEmails", false);
        }

        /// <summary>
        /// Send an email to BOE Authors when a WBS or CLIN was changed on a BOE (or a new BOE).
        /// </summary>
        /// <param name="boe">The BOE.</param>
        /// <param name="clinChanged">if set to <c>true</c> [CLIN changed].</param>
        /// <param name="wbsChanged">if set to <c>true</c> [WBS changed].</param>
        /// <param name="isBoeUpdated">True if BOE was updated, false if BOE was created.</param>
        public void SendBOECLINWBSChanged(FullBoe boe, bool clinChanged, bool wbsChanged, bool isBoeUpdated)
        {
            if (!this.DisableAllEmails)
            {
                SendBOECLINWBSChangedDelegate emailDelegate =
                    new SendBOECLINWBSChangedDelegate(this.PrivateSendBOECLINWBSChanged);
                this.DataFetchingScheduler.FetchEmails(emailDelegate,
                    new object[] { boe, clinChanged, wbsChanged, isBoeUpdated, this.SecurityInformation.ActiveUserData });
            }
            else // if email is disabled by configuration setting
            {
                this.logger.Debug("Email is disabled by configuration setting in SendBOECLINWBSChanged");
            }
        }

        /// <summary>
        /// Send an email to BOE Authors when a WBS or CLIN was changed on a BOE (or a new BOE).
        /// </summary>
        /// <param name="boe">The BOE.</param>
        /// <param name="clinChanged">if set to <c>true</c> [CLIN changed].</param>
        /// <param name="wbsChanged">if set to <c>true</c> [WBS changed].</param>
        /// <param name="isBoeUpdated">if set to <c>true</c> [is BOE updated].</param>
        /// <param name="inUserData">The current user's data.</param>
        public void PrivateSendBOECLINWBSChanged(FullBoe boe, bool clinChanged, bool wbsChanged, bool isBoeUpdated, UserData inUserData)
        {
            if (boe == null)
            {
                throw new ArgumentNullException(nameof(boe));
            }

            if (inUserData == null)
            {
                throw new ArgumentNullException(nameof(inUserData));
            }

            // Find affected Tasks and Variables
            Dictionary<int, List<string>> variablesByTaskId = new Dictionary<int, List<string>>();
            ICollection<BoeTaskElementDTO> affectedTasks = new List<BoeTaskElementDTO>();
            foreach (BoeTaskElementDTO task in boe.Workspace.TaskElements)
            {
                List<string> variableNames = new List<string>();
                ICollection<OrdinaryVariableDto> sumOfClinVariables = task.OrdinaryVariables.Where(v => v.VariableType == VariableType.Task && v.SortBOEBy == VarSortBOEBy.CLIN && v.ValueType == VarValueType.SumOfBOEs).ToList();
                ICollection<OrdinaryVariableDto> sumOfWBSVariables = task.OrdinaryVariables.Where(v => v.VariableType == VariableType.Task && v.SortBOEBy == VarSortBOEBy.WBS && v.ValueType == VarValueType.SumOfBOEs).ToList();

                bool addedTask = false;
                if (clinChanged && sumOfClinVariables.Any())
                {
                    affectedTasks.Add(task);
                    addedTask = true;
                    variableNames.AddRange(sumOfClinVariables.Select(o => o.OrdinaryVariableName));
                }

                if (wbsChanged && sumOfWBSVariables.Any())
                {
                    if (!addedTask)
                    {
                        affectedTasks.Add(task);
                    }

                    variableNames.AddRange(sumOfWBSVariables.Select(o => o.OrdinaryVariableName));
                }

                if (variableNames.Any())
                {
                    variablesByTaskId.Add(task.Id, variableNames);
                }
            }

            // Gather affected BOEs
            ICollection<int> affectedBoeIds = affectedTasks.Select(t => t.BoeID).Distinct().ToList();

            // Common variables across emails
            WorkspaceDTO ws = boe.Workspace;
            string boeUrl = this.GetWorkspaceUrl(ws.Shortname) + @"BOE/EditBOEIndex/boe/" + boe.Id;
            string boeUrlWrapped = @"<a href=""" + boeUrl + @""">View BOE</a>";
            string changes = string.Empty;

            if (clinChanged)
            {
                if (boe.Clin == null)
                {
                    changes = "The CLIN was removed";
                }
                else
                {
                    changes = "The CLIN was changed to " + boe.Clin.ClinTitle;
                }
            }
            
            if (wbsChanged)
            {
                if (changes.Length > 0)
                {
                    changes += "<BR/>";
                }

                if (boe.Wbs == null)
                {
                    changes += "The WBS was removed";
                }
                else
                {
                    changes += "The WBS was changed to " + boe.Wbs.WbsTitle;
                }
            }

            // Gather author emails per BOE
            foreach (int boeId in affectedBoeIds)
            {
                // get a list of all the users having permissions for this BOE
                Collection<PermissionsDTO> boeRoles = this.PermissionLoader.GetBOEPermissions(new List<int>() { boeId });
                var boeRolesWithBoeId = boeRoles.Where(x => x.BOEId.HasValue).ToList();

                // mine the list of roles for what we are interested in (authors)
                var authorsLinqResult = (from r in boeRolesWithBoeId
                                         where r.Role == Role.Author
                                         select r.ETIUserId).ToList();
                var distinctAuthors = from a in authorsLinqResult.Distinct()
                                      select this.UserLoader.GetUserByID(a);
                Collection<UserDTO> authors = new Collection<UserDTO>(distinctAuthors.ToArray());

                if (!authors.Any())
                {
                    // nobody to send an email to .. don't bother
                    continue;
                }

                string emails = string.Join(", ", authors.Select(x => x.EmailAddress).ToArray());
                string sumBoeUrl = this.GetWorkspaceUrl(ws.Shortname) + @"BOE/EditBOEIndex/boe/" + boeId;
                string sumBoeUrlWrapped = @"<a href=""" + sumBoeUrl + @""">View BOE</a>";
                ICollection<BoeTaskElementDTO> boeTasks = affectedTasks.Where(t => t.BoeID == boeId).ToList();
                List<string> taskUrls = new List<string>();

                foreach (BoeTaskElementDTO task in boeTasks)
                {
                    string taskUrl = sumBoeUrl + "#LMLabor/task/" + task.Id;
                    string taskUrlWrapped = @"<a href=""" + taskUrl + @""">" + task.TaskTitle + "</a> Variables - ";
                    taskUrlWrapped += string.Join(",", variablesByTaskId[task.Id]);
                    taskUrls.Add(taskUrlWrapped);
                }

                string tasks = string.Join("<BR/>", taskUrls);

                this.SendEmail(EmailTypes.BOECLINWBSChanged, // email to send
                    emails, // recipients (reviewers)
                    null, // no CC's
                    new string[] { inUserData.DisplayName }, // subject substitution strings
                    new string[]
                    {
                    inUserData.DisplayName, /*user */ // body subsitution strings
                    boeUrlWrapped, /* modified BOE */
                    changes, /* WBS and/or CLIN changes */
                    sumBoeUrlWrapped, /* Sum Of BOE */
                    tasks, /* tasks affected */
                    isBoeUpdated ? "updated" : "created"  // updated or created
                    },
                    null,
                    inUserData,
                    boe.WorkspaceID);
            }
        }

        /// <summary>
        /// Send an email to the workspace reviewers that a BOE is ready for their review.
        /// </summary>
        /// <param name="inBOEForReview">The BOE ready for review</param>
        public void SendBOESubmittedForReview(FullBoe inBOEForReview)
        {
            if (!this.DisableAllEmails)
            {
                SendBOESubmittedForReviewDelegate emailDelegate =
                    new SendBOESubmittedForReviewDelegate(this.PrivateSendBOESubmittedForReview);
                this.DataFetchingScheduler.FetchEmails(emailDelegate,
                    new object[] { inBOEForReview, this.SecurityInformation.ActiveUserData });
            }
            else // if email is disabled by configuration setting
            {
                this.logger.Debug("Email is disabled by configuration setting in SendBOESubmittedForReview");
            }
        }

        /// <summary>
        /// this should be a private function but due to threading emails and testing, it needs to be public
        /// </summary>
        /// <param name="inBOEForReview">The BOE ready for review</param>
        /// <param name="inUserData">active user</param>
        public void PrivateSendBOESubmittedForReview(FullBoe inBOEForReview, UserData inUserData)
        {
            if (inBOEForReview == null)
            {
                throw new ArgumentNullException(nameof(inBOEForReview));
            }

            // BODY
            // The author of the following BOE has requested your review and comments.<br/><br/>
            // Workspace/Proposal: {0}<br/>
            // WBS: {1} {2}<br/>
            // BOE Title: {10}<br/>
            // CLIN: {3} {4}<br/>
            // BOE Description: {5}<br/>
            // BOE Status: {6}<br/>
            // Author: {7}<br/>
            // Approver(s): {8}<br/><br/>
            // {9}

            // get a list of all the users having permissions for this BOE
            Collection<PermissionsDTO> boeRoles = this.PermissionLoader.GetBOEPermissions(new List<int>() { inBOEForReview.Id });
            var boeRolesWithBoeId = boeRoles.Where(x => x.BOEId.HasValue).ToList();

            // mine the list of roles for what we are interested in (reviewers)
            var authorsLinqResult = (from r in boeRolesWithBoeId
                                     where r.Role == Role.Author
                                     select r.ETIUserId).ToList();
            var distinctAuthors = from a in authorsLinqResult.Distinct()
                                  select this.UserLoader.GetUserByID(a);
            Collection<UserDTO> authors = new Collection<UserDTO>(distinctAuthors.ToArray());

            // get author id
            int author = boeRolesWithBoeId.Where(a => a.Role == Role.Author).Select(a => a.ETIUserId).FirstOrDefault();

            var approversLinqResult = from r in boeRolesWithBoeId
                                      where r.Role == Role.Approver
                                      select r.ETIUserId;
            var distinctApprovers = from a in approversLinqResult.Distinct()
                                    select this.UserLoader.GetUserByID(a);
            Collection<UserDTO> approvers = new Collection<UserDTO>(distinctApprovers.ToArray());

            Collection<PermissionsDTO> wsRoles = this.PermissionLoader.GetWorkspacePermissions(inBOEForReview.WorkspaceID);
            var wsRolesWithWsId = wsRoles.Where(x => x.WorkspaceId.HasValue);

            // get all reviewers except author
            var reviewersLinqResult = from r in wsRolesWithWsId
                                      where r.Role == Role.WorkspaceReviewer
                                            && r.ETIUserId != author
                                      select r.ETIUserId;
            var distinctReviewers = from a in reviewersLinqResult.Distinct()
                                    select this.UserLoader.GetUserByID(a);
            Collection<UserDTO> reviewers = new Collection<UserDTO>(distinctReviewers.ToArray());

            if (!reviewers.Any())
            {
                // nobody to send an email to .. don't bother
                return;
            }

            string emails = string.Join(", ", reviewers.Select(x => x.EmailAddress).ToArray());
            WorkspaceDTO ws = inBOEForReview.Workspace;
            WbsDTO wbs = inBOEForReview.Wbs;
            string wbsNum = wbs == null ? "NO WBS" : wbs.WbsNumber;
            string wbsTitle = wbs == null ? "NO WBS" : wbs.WbsTitle;
            ClinDTO clin = inBOEForReview.Clin;
            string clinNum = clin == null ? "NO CLIN" : clin.ClinNumber;
            string clinTitle = clin == null ? "NO CLIN" : clin.ClinTitle;

            string url = this.GetWorkspaceUrl(ws.Shortname) + @"BOE/EditBOEIndex/boe/" + inBOEForReview.Id + "#Comments";
            string urlWrapped = @"<a href=""" + url + @""">View BOE</a>";

            this.SendEmail(EmailTypes.BOESubmittedForReview, // email to send
                emails, // recipients (reviewers)
                null, // no CC's
                new string[] { }, // subject substitution strings
                new string[]
                {
                    ws.WorkspaceName, /*wsname */ // body subsitution strings
                    wbsNum, wbsTitle, /* wbs#, wbs title */
                    clinNum, clinTitle, /* clin#, clin title */
                    inBOEForReview.Description, /* boe desc */
                    this.CommonDataMapper.getBOEStateName(inBOEForReview.State), /* boe status */
                    authors.Any()
                        ? string.Join(";", authors.Select(x => x.DisplayName))
                        : "NO AUTHOR", /* author name */
                    approvers.Any()
                        ? string.Join(";", approvers.Select(x => x.DisplayName))
                        : "NO APPROVERS", /* approvers name */
                    urlWrapped,
                    inBOEForReview.Title
                },
                null,
                inUserData,
                inBOEForReview.WorkspaceID);
        }

        /// <summary>
        /// Send an email to each user who has entered a comment that the Author has responded to
        /// </summary>
        /// <param name="inBOEComments">BOE Comment DTOs</param>
        public void SendBOEAuthorRespondedToComment(Collection<BOECommentDTO> inBOEComments)
        {
            if (!this.DisableAllEmails)
            {
                SendBOEAuthorRespondedToCommentDelegate emailDelegate =
                    new SendBOEAuthorRespondedToCommentDelegate(this.PrivateSendBOEAuthorRespondedToComment);
                this.DataFetchingScheduler.FetchEmails(emailDelegate,
                    new object[] { inBOEComments, this.SecurityInformation.ActiveUserData });
            }
            else // if email is disabled by configuration setting
            {
                this.logger.Debug("Email is disabled by configuration setting in SendBOEAuthorRespondedToComment");
            }
        }

        /// <summary>
        /// this should be a private function but due to threading emails and testing, it needs to be public
        /// </summary>
        /// <param name="inBOEComments">The in BOE comments.</param>
        /// <param name="inUserData">user</param>
        public void PrivateSendBOEAuthorRespondedToComment(Collection<BOECommentDTO> inBOEComments, UserData inUserData)
        {
            if (inBOEComments == null)
            {
                throw new ArgumentNullException(nameof(inBOEComments));
            }

            Collection<BOECommentDTO> authorComments = new Collection<BOECommentDTO>();

            // BODY
            // {0}responded to your comments on the following BOE. <br/> <br/>
            // Workspace/Proposal: {1}
            // WBS: {2} {3}<br/>
            // BOE Title: {9}<br/>
            // CLIN: {4} {5}<br/>
            // BOE Description: {6}<br/>
            // BOE Status: {7}<br/>
            // {8}

            // loop through the input collection and remove any non-Author comments
            Collection<UserDTO> commenters = new Collection<UserDTO>();
            foreach (BOECommentDTO boeComment in inBOEComments)
            {
                if (boeComment.BOEResponseToCommentID != null)
                {
                    // get all Author Comments together
                    authorComments.Add(boeComment);
                    BOECommentDTO commenterDTO = this.BoeCommentLoader
                        .GetByIds(new List<int>() { boeComment.BOEResponseToCommentID.Value }).First();
                    UserDTO user = this.UserLoader.GetUserByID(commenterDTO.BOECommentETIUserID);
                    commenters.Add(user);
                }
            }

            // if there are no author comments, don't send an email
            if (authorComments.Any())
            {
                // get author
                UserDTO author = this.UserLoader.GetUserByID(authorComments[0].BOECommentETIUserID);

                // get boe dto, all comments are associated with the same BOE ID
                FullBoe boeObject = this.factory.CreateFullBoe(authorComments[0].BoeID);

                string emails = string.Join(", ", commenters.Select(x => x.EmailAddress).ToArray());
                WorkspaceDTO ws = boeObject.Workspace;

                WbsDTO wbs = boeObject.Wbs;
                string wbsNum = wbs == null ? "NO WBS" : wbs.WbsNumber;
                string wbsTitle = wbs == null ? "NO WBS" : wbs.WbsTitle;

                ClinDTO clin = boeObject.Clin;
                string clinNum = clin == null ? "NO CLIN" : clin.ClinNumber;
                string clinTitle = clin == null ? "NO CLIN" : clin.ClinTitle;

                string url = this.GetWorkspaceUrl(ws.Shortname) + @"BOE/EditBOEIndex/boe/" + boeObject.Id + @"#Comments";
                string urlWrapped = @"<a href=""" + url + @""">View comments and responses</a>";

                this.SendEmail(EmailTypes.BOEAuthorRespondedToComment, // email to send
                    emails, // recipients (reviewers)
                    null, // no CC's
                    new string[] { }, // subject substitution strings
                    new string[]
                    {
                        author.DisplayName, // author who commented
                        ws.WorkspaceName, /*wsname */ // body subsitution strings
                        wbsNum, wbsTitle, /* wbs#, wbs title */
                        clinNum, clinTitle, /* clin#, clin title */
                        boeObject.Description, /* boe desc */ this.CommonDataMapper.getBOEStateName(boeObject.State), /* boe status */
                        urlWrapped,
                        boeObject.Title ?? string.Empty
                    },
                    null,
                    inUserData,
                    boeObject.WorkspaceID);
            }
        }

        /// <summary>
        /// Send email to BOE Author and all Approvers when an Approver has approved the BOE
        /// </summary>
        /// <param name="inApprover">The Boe Approver Response.</param>
        /// <param name="inBOE">The BOE that was approved</param>
        public virtual void SendBOEApproversAuthorApproverApproved(BoeApproverResponseDTO inApprover, FullBoe inBOE)
        {
            if (!this.DisableAllEmails)
            {
                SendBOEApproversAuthorApproverApprovedDelegate emailDelegate =
                    new SendBOEApproversAuthorApproverApprovedDelegate(this
                        .PrivateSendBOEApproversAuthorApproverApproved);
                this.DataFetchingScheduler.FetchEmails(emailDelegate,
                    new object[] { inApprover, inBOE, this.SecurityInformation.ActiveUserData });
            }
            else // if email is disabled by configuration setting
            {
                this.logger.Debug("Email is disabled by configuration setting in SendBOEApproversAuthorApproverApproved");
            }
        }

        /// <summary>
        /// this should be a private function but due to threading emails and testing, it needs to be public
        /// </summary>
        /// <param name="inApprover">approver</param>
        /// <param name="inBOE">The BOE that was approved</param>
        /// <param name="inUserData">active user</param>
        public void PrivateSendBOEApproversAuthorApproverApproved(BoeApproverResponseDTO inApprover, FullBoe inBOE,
            UserData inUserData)
        {
            if (inApprover == null)
            {
                throw new ArgumentNullException(nameof(inApprover));
            }

            if (inBOE == null)
            {
                throw new ArgumentNullException(nameof(inBOE));
            }

            // SUBJECT
            // generation: BOE has been approved by {0}
            // BODY
            // {0} has approved the following BOE.<BR/>
            // <BR/>
            // Workspace/Proposal: {1}<BR/>
            // WBS: {2} {3}<BR/>
            // BOE Title: {10}<BR/>
            // CLIN: {4} {5}<BR/>
            // BOE Description: {6}<BR/>
            // BOE Status: {7}<BR/>
            // Approvers’ Response:<BR/>
            // {8}<BR/>
            // <BR/>
            // {9}

            string authorEmails = string.Empty;

            if (inBOE.AuthorIDs.Any() || inBOE.SubcontractorAuthorIDs.Any())
            {
                var authors = from a in this.PermissionLoader.GetBOEPermissions(new List<int>() { inBOE.Id })
                        .Where(x => x.Role == Role.Author || x.Role == Role.SubcontractorAuthor).Select(x => x)
                        .ToArray()
                              select this.UserLoader.GetUserByID(a.ETIUserId);
                authorEmails = string.Join("; ", authors.Select(x => x.EmailAddress));
            }

            var approver = this.UserLoader.GetUserByID(inApprover.ETIUserID);
            IReadOnlyCollection<BoeApproverResponseDTO> boeAppovers = inBOE.ApproverResponses;
            var approvers = (from a in boeAppovers
                            select this.UserLoader.GetUserByID(a.ETIUserID)).ToList();
            var workspace = inBOE.Workspace;

            WbsDTO wbs = inBOE.Wbs;
            string wbsNum = wbs == null ? "NO WBS" : wbs.WbsNumber;
            string wbsTitle = wbs == null ? "NO WBS" : wbs.WbsTitle;

            ClinDTO clin = inBOE.Clin;
            string clinNum = clin == null ? "NO CLIN" : clin.ClinNumber;
            string clinTitle = clin == null ? "NO CLIN" : clin.ClinTitle;

            List<string> approverLines = new List<string>();

            foreach (BoeApproverResponseDTO a in boeAppovers)
            {
                approverLines.Add("&nbsp;&nbsp;&nbsp;" +
                                  approvers.First(x => x.UserID == a.ETIUserID).DisplayName + ": " +
                                  (a.ApproverResponse == ApproverReponseType.Approved
                                      ? "Approved"
                                      : "Awaiting Approval"));
            }

            string url = this.GetWorkspaceUrl(workspace.Shortname) + @"BOE/EditBOEIndex/boe/" + inBOE.Id;
            string urlWrapped = @"<a href=""" + url + @""">View BOE</a>";

            this.SendEmail(
                EmailTypes.BOEApproversAuthorApproverApproved,
                authorEmails,
                new Collection<UserDTO>(),
                new string[] { approver.DisplayName },
                new string[]
                {
                    approver.DisplayName,
                    workspace.Shortname,
                    wbsNum,
                    wbsTitle,
                    clinNum,
                    clinTitle,
                    inBOE.Description, this.CommonDataMapper.getBOEStateName(inBOE.State),
                    string.Join("<BR/>", approverLines),
                    urlWrapped,
                    inBOE.Title ?? string.Empty
                },
                null,
                inUserData,
                inBOE.WorkspaceID);
        }

        /// <summary>
        /// Send email to BOE Author and all Approvers when an Approver has rejected the BOE
        /// </summary>
        /// <param name="inRejecter">BOE Approver Rejection Response</param>
        /// <param name="inBOE">The BOE that was rejected</param>
        public virtual void SendBOEApproversAuthorApproverRejected(BoeApproverResponseDTO inRejecter, FullBoe inBOE)
        {
            if (!this.DisableAllEmails)
            {
                SendBOEApproversAuthorApproverRejectedDelegate emailDelegate =
                    new SendBOEApproversAuthorApproverRejectedDelegate(this
                        .PrivateSendBOEApproversAuthorApproverRejected);
                this.DataFetchingScheduler.FetchEmails(emailDelegate,
                    new object[] { inRejecter, inBOE, this.SecurityInformation.ActiveUserData });
            }
            else // if email is disabled by configuration setting
            {
                this.logger.Debug("Email is disabled by configuration setting in SendBOEApproversAuthorApproverRejected");
            }
        }

        /// <summary>
        /// this should be a private function but due to threading emails and testing, it needs to be public
        /// </summary>
        /// <param name="inRejecter">person who rejected the boe</param>
        /// <param name="inBOE">the boe that was rejected</param>
        /// <param name="inUserData">active user</param>
        public void PrivateSendBOEApproversAuthorApproverRejected(BoeApproverResponseDTO inRejecter, FullBoe inBOE,
            UserData inUserData)
        {
            if (inRejecter == null)
            {
                throw new ArgumentNullException(nameof(inRejecter));
            }

            if (inBOE == null)
            {
                throw new ArgumentNullException(nameof(inBOE));
            }

            // SUBJECT
            // generation: BOE has been rejected by {0}
            // BODY
            // {0} has rejected the following BOE. No more approvals can be made and any previous approvals have been removed. The BOE has been placed in Draft and the Author can resume editing of the BOE.<BR/>
            // <BR/>
            // Workspace/Proposal: {1}<BR/>
            // WBS: {2} {3}<BR/>
            // BOE Title: {9}<BR/>
            // CLIN: {4} {5}<BR/>
            // BOE Description: {6}<BR/>
            // BOE Status: {7}<BR/>
            // <BR/>
            // {8}

            string authorEmails = string.Empty;
            if (inBOE.AuthorIDs.Any() || inBOE.SubcontractorAuthorIDs.Any())
            {
                var authors = from a in this.PermissionLoader.GetBOEPermissions(new List<int>() { inBOE.Id })
                        .Where(x => x.Role == Role.Author || x.Role == Role.SubcontractorAuthor).Select(x => x)
                        .ToArray()
                              select this.UserLoader.GetUserByID(a.ETIUserId);
                authorEmails = string.Join("; ", authors.Select(x => x.EmailAddress));
            }

            var rejecter = this.UserLoader.GetUserByID(inRejecter.ETIUserID);
            var boeApprovers = this.PermissionLoader.GetBOEPermissions(new List<int>() { inBOE.Id })
                .Where(x => x.Role == Role.Approver).Select(x => x).ToArray();
            var approvers = from a in boeApprovers
                            select this.UserLoader.GetUserByID(a.ETIUserId);
            var workspace = inBOE.Workspace;

            WbsDTO wbs = inBOE.Wbs;
            string wbsNum = wbs == null ? "NO WBS" : wbs.WbsNumber;
            string wbsTitle = wbs == null ? "NO WBS" : wbs.WbsTitle;

            ClinDTO clin = inBOE.Clin;
            string clinNum = clin == null ? "NO CLIN" : clin.ClinNumber;
            string clinTitle = clin == null ? "NO CLIN" : clin.ClinTitle;

            string url = this.GetWorkspaceUrl(workspace.Shortname) + @"BOE/EditBOEIndex/boe/" + inBOE.Id;
            string urlWrapped = @"<a href=""" + url + @""">View BOE</a>";

            this.SendEmail(
                EmailTypes.BOEApproversAuthorApproverRejected,
                authorEmails,
                approvers.ToCollection(),
                new string[] { rejecter.DisplayName },
                new string[]
                {
                    rejecter.DisplayName,
                    workspace.Shortname,
                    wbsNum,
                    wbsTitle,
                    clinNum,
                    clinTitle,
                    inBOE.Description, this.CommonDataMapper.getBOEStateName(inBOE.State),
                    urlWrapped,
                    inBOE.Title ?? string.Empty
                },
                null,
                inUserData,
                inBOE.WorkspaceID);
        }

        /// <summary>
        /// Send an email to the BOE authors that a BOE's state has moved from Approved to Draft(open for edit)
        /// </summary>
        /// <param name="inBOEForEdit">The BOE.</param>
        /// <param name="inWorkspace">Workspace for the BOE.</param>
        public void SendBOEAuthorsEmailOpenedForEdit(FullBoe inBOEForEdit, FullWorkspace inWorkspace)
        {
            if (inBOEForEdit == null)
            {
                throw new ArgumentNullException(nameof(inBOEForEdit));
            }

            // BODY
            // The following BOE is now open for edits by the Author.  The Author must submit the BOE to the Approver(s) once it is ready for approval. 
            // Workspace/Proposal: {0} <br/>
            // WBS: {1} {2}<br/>
            // CLIN: {3} {4}<br/>
            // BOE: {4} <br/>
            // Author: {7}<br/>
            // Approver(s): {8}<br/><br/>
            // {9}

            Collection<PermissionsDTO> boePermissions = this.PermissionLoader.GetBOEPermissions(new List<int>() { inBOEForEdit.Id });

            // get the approvers
            var boeApprovers = boePermissions.Where(x => x.Role == Role.Approver);

            // get the authors
            var boeAuthors = boePermissions.Where(x => x.Role == Role.Author || x.Role == Role.SubcontractorAuthor);

            Collection<UserDTO> approvers = new Collection<UserDTO>();

            foreach (var boeApprover in boeApprovers)
            {
                approvers.Add(this.UserLoader.GetUserByID(boeApprover.ETIUserId));
            }

            Collection<UserDTO> authors = new Collection<UserDTO>();

            foreach (var boeAuthor in boeAuthors)
            {
                authors.Add(this.UserLoader.GetUserByID(boeAuthor.ETIUserId));
            }

            this.SendAuthorEmailBOEOpenedForEdit(inBOEForEdit, inWorkspace, approvers, authors, this.SecurityInformation.ActiveUserData);
        }

        /// <summary>
        /// Send an email to the BOE authors when a workspace is opened for edit
        /// </summary>
        /// <param name="inWorkspace">The workspace DTO the BOE is contained in</param>
        /// <exception cref="System.ArgumentNullException">inWorkspace</exception>
        public virtual void SendWorkspaceAuthorsEmailOpenedForEdit(FullWorkspace inWorkspace)
        {
            if (inWorkspace == null)
            {
                throw new ArgumentNullException(nameof(inWorkspace));
            }

            // The following BOE is now open for edits by the Author. The Author must submit the BOE to the Approver(s) once it is ready for approval. <BR/><BR/>
            // Workspace/Proposal: {0}<BR/>
            // WBS: {1} {2}<BR/>
            // CLIN: {3} {4}<BR/>
            // BOE: {5}<BR/>
            // Author: {6} <BR/>
            // Approver(s): {7}<BR/><BR/>
            // {8}

            // get all the boe IDS for this workspace .. we need to send an email for each one in the DRAFT state
            ICollection<FullBoe> boesInDraftForWorkspace = inWorkspace.Boes.Where(x => x.State == BOEState.Draft)
                .ToCollection(); // Note: DraftLocked state is not editable

            if (boesInDraftForWorkspace.Any())
            {
                ICollection<int> draftBoeIds = boesInDraftForWorkspace.Select(i => i.Id).ToCollection();
                ICollection<PermissionsDTO> allPermissions = this.PermissionLoader.GetBOEPermissions(draftBoeIds);
                ICollection<UserDTO> allUsers = this.UserLoader
                    .GetByIds(allPermissions.Select(i => i.ETIUserId).ToCollection()).ToCollection();

                foreach (FullBoe boe in boesInDraftForWorkspace)
                {
                    if (!boe.AuthorIDs.Any() && !boe.SubcontractorAuthorIDs.Any())
                    {
                        this.logger.Error("No authors present for BOE [" + boe.Description + "] contained in workspace [" +
                                   inWorkspace.WorkspaceName +
                                   "].  NOT sending email to admin that boe is opened for edit.");
                    }
                    else
                    {
                        // Permissions for the current BOE.
                        ICollection<PermissionsDTO> boePermissions =
                            allPermissions.Where(i => i.BOEId == boe.Id).ToCollection();
                        // get the authors and approvers, their emails, display names
                        // and stitch together the info for the email
                        ICollection<PermissionsDTO> boeApprovers =
                            boePermissions.Where(x => x.Role == Role.Approver).ToCollection();
                        ICollection<int> approverIds = boeApprovers.Select(a => a.ETIUserId).ToCollection();
                        ICollection<UserDTO> approvers = allUsers.Where(i => approverIds.Contains(i.UserID)).ToCollection();

                        ICollection<UserDTO> authors = allUsers.Where(i => boe.AuthorIDs.Contains(i.UserID) || boe.SubcontractorAuthorIDs.Contains(i.UserID)).ToCollection();

                        // send the email
                        this.SendAuthorEmailBOEOpenedForEdit(boe, inWorkspace, approvers, authors, this.SecurityInformation.ActiveUserData);
                    }
                }
            }
        }

        /// <summary>
        /// This function will send an author an email if the BOE has been opened for edit via the workspace or BOE
        /// </summary>
        /// <param name="inBOE">The BOE DTO.</param>
        /// <param name="inWorkspace">Workspace the BOE belongs to.</param>
        /// <param name="approvers">The list of approvers for the BOE.</param>
        /// <param name="authors">List of authors for the BOE.</param>
        /// <param name="inUserData">User data for the email.</param>
        public void SendAuthorEmailBOEOpenedForEdit(FullBoe inBOE, FullWorkspace inWorkspace,
            ICollection<UserDTO> approvers, ICollection<UserDTO> authors, UserData inUserData)
        {
            if (!this.DisableAllEmails)
            {
                if (inBOE == null)
                {
                    throw new ArgumentNullException(nameof(inBOE));
                }

                if (inWorkspace == null)
                {
                    throw new ArgumentNullException(nameof(inWorkspace));
                }

                SendWorkspaceStatusChangeDelegate emailDelegate =
                    new SendWorkspaceStatusChangeDelegate(this.PrivateSendWorkspaceStatusChange);
                // Get the WBS and CLIN from the workspace. Reason for this is if the same workspace is being continually passed to this method only
                // one database read is made as opposed to getting these object from each individual boe.
                WbsDTO wbs = inBOE.WBSID.HasValue
                    ? inWorkspace.WbsElements.FirstOrDefault(i => i.Id == inBOE.WBSID)
                    : null;
                ClinDTO clin = inBOE.CLINID.HasValue
                    ? inWorkspace.Clins.FirstOrDefault(i => i.Id == inBOE.CLINID)
                    : null;

                // The following BOE is now open for edits by the Author. The Author must submit the BOE to the Approver(s) once it is ready for approval. <BR/><BR/>
                // Workspace/Proposal: {0}<BR/>
                // WBS: {1} {2}<BR/>
                // BOE Title: {9}<BR/>
                // CLIN: {3} {4}<BR/>
                // BOE: {5}<BR/>
                // Author(s): {6} <BR/>
                // Approver(s): {7}<BR/><BR/>
                // {8}

                this.DataFetchingScheduler.FetchEmails(emailDelegate,
                    new object[] { inBOE, inWorkspace, wbs, clin, approvers, authors, inUserData, EmailTypes.BOEOpenedForEdit });
            }
            else // if email is disabled by configuration setting
            {
                this.logger.Debug("SendWorkspaceOpenedForEdit Email is disabled by DisableAllEmails configuration setting");
            }
        }

        /// <summary>
        /// This function will send an author an email if the Workspace status changes from Working to Locked
        /// </summary>
        /// <param name="workspace">Workspace the BOE belongs to.</param>
        public void SendWorkspaceWorkingToLocked(FullWorkspace workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (!this.DisableAllEmails)
            {
                // The Workspace that the following BOE belongs to has changed status from Working to Locked. The Author is no longer able to edit Travel Rates, Labor Rates, and Hour and Cost estimates in the BOE. Text is still editable for BOEs in Draft. <BR/><BR/>
                // Workspace/Proposal: {0}<BR/>
                // WBS: {1} {2}<BR/>
                // BOE Title: {9}<BR/>
                // CLIN: {3} {4}<BR/>
                // BOE: {5}<BR/>
                // Author(s): {6} <BR/>
                // Approver(s): {7}<BR/><BR/>
                // {8}

                this.SendWorkspaceStatusChange(workspace, EmailTypes.WorkspaceWorkingToLocked);
            }
            else // if email is disabled by configuration setting
            {
                this.logger.Debug("SendWorkspaceWorkingToLocked Email is disabled by DisableAllEmails configuration setting");
            }
        }

        /// <summary>
        /// This function will send an author an email if the Workspace status changes from Locked to Working
        /// </summary>
        /// <param name="workspace">Workspace the BOE belongs to.</param>
        public void SendWorkspaceLockedToWorking(FullWorkspace workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (!this.DisableAllEmails)
            {
                // The Workspace that the following BOE belongs to has changed status from Locked to Working. The BOE is once again open for edits. The Author must submit the BOE to the Approver(s) once it is ready for approval. <BR/><BR/>
                // Workspace/Proposal: {0}<BR/>
                // WBS: {1} {2}<BR/>
                // BOE Title: {9}<BR/>
                // CLIN: {3} {4}<BR/>
                // BOE: {5}<BR/>
                // Author(s): {6} <BR/>
                // Approver(s): {7}<BR/><BR/>
                // {8}

                this.SendWorkspaceStatusChange(workspace, EmailTypes.WorkspaceLockedToWorking);
            }
            else // if email is disabled by configuration setting
            {
                this.logger.Debug("SendWorkspaceLockedToWorking Email is disabled by DisableAllEmails configuration setting");
            }
        }

        /// <summary>
        /// This function will send an author an email if the Workspace status changes from Working to Initialization
        /// </summary>
        /// <param name="workspace">Workspace the BOE belongs to.</param>
        public void SendWorkspaceWorkingToInitialization(FullWorkspace workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (!this.DisableAllEmails)
            {
                // The Workspace that the following BOE belongs to has changed status from Working to Initialization. The Author is no longer able to edit the BOE. <BR/><BR/>
                // Workspace/Proposal: {0}<BR/>
                // WBS: {1} {2}<BR/>
                // BOE Title: {9}<BR/>
                // CLIN: {3} {4}<BR/>
                // BOE: {5}<BR/>
                // Author(s): {6} <BR/>
                // Approver(s): {7}<BR/><BR/>
                // {8}

                this.SendWorkspaceStatusChange(workspace, EmailTypes.WorkspaceWorkingToInitialization);
            }
            else // if email is disabled by configuration setting
            {
                this.logger.Debug("SendWorkspaceWorkingToInitialization Email is disabled by DisableAllEmails configuration setting");
            }
        }

        /// <summary>
        /// This function will send an author an email if the Workspace status changes from Initialization to Working (all times except 1st)
        /// </summary>
        /// <param name="workspace">Workspace the BOE belongs to.</param>
        public void SendWorkspaceInitializationToWorkingSubsequent(FullWorkspace workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (!this.DisableAllEmails)
            {
                // The Workspace that the following BOE belongs to has changed status from Initialization to Working. The BOE is once again open for edits. The Author must submit the BOE to the Approver(s) once it is ready for approval. <BR/><BR/>
                // Workspace/Proposal: {0}<BR/>
                // WBS: {1} {2}<BR/>
                // BOE Title: {9}<BR/>
                // CLIN: {3} {4}<BR/>
                // BOE: {5}<BR/>
                // Author(s): {6} <BR/>
                // Approver(s): {7}<BR/><BR/>
                // {8}

                this.SendWorkspaceStatusChange(workspace, EmailTypes.WorkspaceInitializationToWorkingSubsequent);
            }
            else // if email is disabled by configuration setting
            {
                this.logger.Debug("SendWorkspaceInitializationToWorkingSubsequent Email is disabled by DisableAllEmails configuration setting");
            }
        }

        /// <summary>
        /// Send the workspace status change email for the given type
        /// </summary>
        /// <param name="workspace">Workspace with status change</param>
        /// <param name="emailType">Status change email type</param>
        private void SendWorkspaceStatusChange(FullWorkspace workspace, EmailTypes emailType)
        {
            // get all the boe IDS for this workspace .. we need to send an email for each one in the DRAFT state
            ICollection<FullBoe> boesInDraftForWorkspace = workspace.Boes.Where(x => x.State == BOEState.Draft)
                .ToCollection(); // Note: DraftLocked state is not editable
            
            // Only get DraftLocked Boes if moving from Locked to Working or Working to Locked since most Boes will be in this state if the Workspace is locked
            if (emailType == EmailTypes.WorkspaceLockedToWorking || emailType == EmailTypes.WorkspaceWorkingToLocked)
            {
                boesInDraftForWorkspace.AddRange(workspace.Boes.Where(x => x.State == BOEState.DraftLocked));
            }

            if (boesInDraftForWorkspace.Any())
            {
                ICollection<int> draftBoeIds = boesInDraftForWorkspace.Select(i => i.Id).ToCollection();
                ICollection<PermissionsDTO> allPermissions = this.PermissionLoader.GetBOEPermissions(draftBoeIds);
                ICollection<UserDTO> allUsers = this.UserLoader
                    .GetByIds(allPermissions.Select(i => i.ETIUserId).ToCollection()).ToCollection();

                foreach (FullBoe boe in boesInDraftForWorkspace)
                {
                    if (!boe.AuthorIDs.Any() && !boe.SubcontractorAuthorIDs.Any())
                    {
                        this.logger.Error("No authors present for BOE [" + boe.Description + "] contained in workspace [" +
                                   workspace.WorkspaceName +
                                   "].  NOT sending email to admin that boe is opened for edit.");
                    }
                    else
                    {
                        // Permissions for the current BOE.
                        ICollection<PermissionsDTO> boePermissions =
                            allPermissions.Where(i => i.BOEId == boe.Id).ToCollection();
                        // get the authors and approvers, their emails, display names
                        // and stitch together the info for the email
                        ICollection<PermissionsDTO> boeApprovers =
                            boePermissions.Where(x => x.Role == Role.Approver).ToCollection();
                        ICollection<int> approverIds = boeApprovers.Select(a => a.ETIUserId).ToCollection();
                        ICollection<UserDTO> approvers = allUsers.Where(i => approverIds.Contains(i.UserID)).ToCollection();

                        ICollection<UserDTO> authors = allUsers.Where(i => boe.AuthorIDs.Contains(i.UserID) || boe.SubcontractorAuthorIDs.Contains(i.UserID)).ToCollection();

                        SendWorkspaceStatusChangeDelegate emailDelegate =
                            new SendWorkspaceStatusChangeDelegate(this.PrivateSendWorkspaceStatusChange);
                        // Get the WBS and CLIN from the workspace. Reason for this is if the same workspace is being continually passed to this method only
                        // one database read is made as opposed to getting these object from each individual boe.
                        WbsDTO wbs = boe.WBSID.HasValue
                            ? workspace.WbsElements.FirstOrDefault(i => i.Id == boe.WBSID)
                            : null;
                        ClinDTO clin = boe.CLINID.HasValue
                            ? workspace.Clins.FirstOrDefault(i => i.Id == boe.CLINID)
                            : null;
                        this.DataFetchingScheduler.FetchEmails(emailDelegate,
                            new object[] { boe, workspace, wbs, clin, approvers, authors, this.SecurityInformation.ActiveUserData, emailType });
                    }
                }
            }
        }

        /// <summary>
        /// this should be a private function but due to threading emails and testing, it needs to be public
        /// </summary>
        /// <param name="inBOE">the BOE DTO</param>
        /// <param name="workspace">Workspace that contains the BOE.</param>
        /// <param name="wbs">WBS associated to the BOE.</param>
        /// <param name="clin">CLIN associated to the BOE.</param>
        /// <param name="approvers">The list of approvers for the BOE.</param>
        /// <param name="authors">The list of authors for the BOE.</param>
        /// <param name="inUserData">user</param>
        /// <param name="emailType">Type of Workspace Status Change Email</param>
        public void PrivateSendWorkspaceStatusChange(BoeDTO inBOE, WorkspaceDTO workspace, WbsDTO wbs,
            ClinDTO clin, Collection<UserDTO> approvers, Collection<UserDTO> authors, UserData inUserData, EmailTypes emailType)
        {
            if (approvers == null)
            {
                throw new ArgumentNullException(nameof(approvers));
            }

            if (authors == null)
            {
                throw new ArgumentNullException(nameof(authors));
            }

            if (inBOE == null)
            {
                throw new ArgumentNullException(nameof(inBOE));
            }

            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            string wbsNum = wbs == null ? "NO WBS" : wbs.WbsNumber;
            string wbsTitle = wbs == null ? "NO WBS" : wbs.WbsTitle;

            string clinNum = clin == null ? "NO CLIN" : clin.ClinNumber;
            string clinTitle = clin == null ? "NO CLIN" : clin.ClinTitle;

            string url = this.GetWorkspaceUrl(workspace.Shortname) + @"BOE/EditBOEIndex/boe/" + inBOE.Id;
            string urlWrapped = @"<a href=""" + url + @""">View BOE</a>";

            foreach (UserDTO author in authors)
            {
                string emails = author.EmailAddress;

                this.SendEmail(
                    emailType, // the same email type is used by Workspace and BOE
                    emails, // recipients (authors)
                    null, // CC's (this email should never send CC's)
                    new string[] { }, // subject substitution strings
                    new string[]
                    {
                        workspace.WorkspaceName, /*wsname */ // body subsitution strings
                        wbsNum, wbsTitle, /* wbs#, wbs title */
                        clinNum, clinTitle, /* clin#, clin title */ this.CommonDataMapper.getBOEStateName(inBOE.State), /* boe status */
                        author.DisplayName, /* author name */
                        approvers.Any()
                            ? string.Join(";", approvers.Select(x => x.DisplayName))
                            : "NO APPROVERS", /* approvers name */
                        urlWrapped,
                        inBOE.Title ?? string.Empty
                    },
                    null,
                    inUserData,
                    inBOE.WorkspaceID);
            }
        }

        /// <summary>
        /// Send an email to the BOE Approvers that a BOE has moved from Draft to Awaiting Approval
        /// </summary>
        /// <param name="inBOEForApproval">BOE that was put in Awaiting Approval state</param>
        public void SendBOEApproversEmailAwaitingApproval(FullBoe inBOEForApproval)
        {
            if (!this.DisableAllEmails)
            {
                SendBOEApproversEmailAwaitingApprovalDelegate emailDelegate =
                    new SendBOEApproversEmailAwaitingApprovalDelegate(this._SendBOEApproversEmailAwaitingApproval);
                this.DataFetchingScheduler.FetchEmails(emailDelegate,
                    new object[] { inBOEForApproval, this.SecurityInformation.ActiveUserData });
            }
            else // if email is disabled by configuration setting
            {
                this.logger.Debug("Email is disabled by configuration setting in SendBOEApproversEmailAwaitingApproval");
            }
        }

        /// <summary>
        /// Sends the BOE approvers email awaiting approval.
        /// </summary>
        /// <param name="inBOEForApproval">The BOE for approval.</param>
        /// <param name="inUserData">The user data.</param>
        private void _SendBOEApproversEmailAwaitingApproval(FullBoe inBOEForApproval, UserData inUserData)
        {
            if (inBOEForApproval == null)
            {
                throw new ArgumentNullException(nameof(inBOEForApproval));
            }

            // BODY
            // The Author has completed work on the following BOE.  You may approve the BOE or reject it to send it back to the Author for rework.
            // Workspace/Proposal: {0} <br/>
            // WBS: {1} {2}<br/>
            // BOE Title: {9}<br/>
            // CLIN: {3} {4}<br/>
            // BOE Description: {5} <br/>
            // BOE Status: {6} <br/>
            // Author: {7}<br/>
            // Approver(s): {8}<br/><br/>
            // {9}

            var approversLinqResult = from r in this.PermissionLoader
                    .GetBOEPermissions(new List<int>() { inBOEForApproval.Id }).Where(x => x.Role == Role.Approver)
                    .Select(x => x).ToArray()
                                      select r.ETIUserId;

            var distinctApprovers = from a in approversLinqResult
                                    select this.UserLoader.GetUserByID(a);
            Collection<UserDTO> approvers = new Collection<UserDTO>(distinctApprovers.ToArray());

            this.SendApproverEmailBOEAwaitingApproval(inBOEForApproval, approvers, inUserData);
        }

        /// <summary>
        /// This function will send an approver an email if the BOE is awaiting approval for approve/reject via the workspace or BOE
        /// </summary>
        /// <param name="inBOE">the BOE DTO</param>
        /// <param name="approvers">the list of approvers</param>
        /// <param name="inUserData">The user data.</param>
        public void SendApproverEmailBOEAwaitingApproval(FullBoe inBOE, Collection<UserDTO> approvers,
            UserData inUserData)
        {
            if (!this.DisableAllEmails)
            {
                SendApproverEmailBOEAwaitingApprovalDelegate emailDelegate =
                    new SendApproverEmailBOEAwaitingApprovalDelegate(this.PrivateSendApproverEmailBOEAwaitingApproval);
                this.DataFetchingScheduler.FetchEmails(emailDelegate, new object[] { inBOE, approvers, inUserData });
            }
            else // if email is disabled by configuration setting
            {
                this.logger.Debug("Email is disabled by configuration setting in SendApproverEmailBOEAwaitingApproval");
            }
        }

        /// <summary>
        ///  this should be a private function but due to threading emails and testing, it needs to be public
        /// </summary>
        /// <param name="inBOE">the BOE DTO</param>
        /// <param name="approvers">the list of approvers </param>
        /// <param name="inUserData">user</param>
        public void PrivateSendApproverEmailBOEAwaitingApproval(FullBoe inBOE, Collection<UserDTO> approvers,
            UserData inUserData)
        {
            if (approvers == null)
            {
                throw new ArgumentNullException(nameof(approvers));
            }

            if (inBOE == null)
            {
                throw new ArgumentNullException(nameof(inBOE));
            }
            // BODY
            // The Author has completed work on the following BOE.  You may approve the BOE or reject it to send it back to the Author for rework.
            // Workspace/Proposal: {0} <br/>
            // WBS: {1} {2}<br/>
            // BOE Title: {10}<br/>
            // CLIN: {3} {4}<br/>
            // BOE Description: {5} <br/>
            // BOE Status: {6} <br/>
            // Author: {7}<br/>
            // Approver(s): {8}<br/><br/>
            // {9}

            WorkspaceDTO ws = inBOE.Workspace;

            WbsDTO wbs = inBOE.Wbs;
            string wbsNum = wbs == null ? "NO WBS" : wbs.WbsNumber;
            string wbsTitle = wbs == null ? "NO WBS" : wbs.WbsTitle;

            ClinDTO clin = inBOE.Clin;
            string clinNum = clin == null ? "NO CLIN" : clin.ClinNumber;
            string clinTitle = clin == null ? "NO CLIN" : clin.ClinTitle;

            string url = this.GetWorkspaceUrl(ws.Shortname) + @"BOE/EditBOEIndex/boe/" + inBOE.Id + "#Comments";
            string urlWrapped = @"<a href=""" + url + @""">View BOE</a>";

            string approverEmails = null;
            string approversDisplayNames = string.Empty;
            string authorDisplayNames = string.Empty;

            if (approvers.Any())
            {
                approverEmails = string.Join(", ", approvers.Select(x => x.EmailAddress));
                approversDisplayNames = string.Join("; ", approvers.Select(x => x.DisplayName));
            }

            if (inBOE.AuthorIDs.Any() || inBOE.SubcontractorAuthorIDs.Any())
            {
                var authors = from a in this.PermissionLoader.GetBOEPermissions(new List<int>() { inBOE.Id })
                        .Where(x => x.Role == Role.Author || x.Role == Role.SubcontractorAuthor).Select(x => x)
                        .ToArray()
                              select this.UserLoader.GetUserByID(a.ETIUserId);
                authorDisplayNames = string.Join("; ", authors.Select(x => x.DisplayName));
            }

            this.SendEmail(
                EmailTypes.ApproverEmailBOEAwaitingApproval, // the same email type is used by Workspace and BOE
                approverEmails, // recipients (approvers)
                null, // CC's (none)
                new string[] { }, // subject substitution strings
                new string[]
                {
                    ws.WorkspaceName, /*wsname */ // body subsitution strings
                    wbsNum, wbsTitle, /* wbs#, wbs title */
                    clinNum, clinTitle, /* clin#, clin title */
                    inBOE.Description, /* BOE description */ this.CommonDataMapper.getBOEStateName(inBOE.State), /* boe status */
                    authorDisplayNames, /* author names */
                    approversDisplayNames, /* approvers name */
                    urlWrapped,
                    inBOE.Title ?? string.Empty
                },
                null,
                inUserData,
                inBOE.WorkspaceID);
        }

        /// <summary>
        /// Send all workspace Admins an email if all the BOES in the workspace have been approved
        /// </summary>
        /// <param name="inBOE">BOE</param>
        public void SendWorkspaceAdminEmailAllBOEsApproved(FullBoe inBOE)
        {
            if (!this.DisableAllEmails)
            {
                SendWorkspaceAdminEmailAllBOEsApprovedDelegate emailDelegate =
                    new SendWorkspaceAdminEmailAllBOEsApprovedDelegate(this
                        .PrivateSendWorkspaceAdminEmailAllBOEsApproved);
                this.DataFetchingScheduler.FetchEmails(emailDelegate,
                    new object[] { inBOE, this.SecurityInformation.ActiveUserData });
            }
            else // if email is disabled by configuration setting
            {
                this.logger.Debug("Email is disabled by configuration setting in SendWorkspaceAdminEmailAllBOEsApproved");
            }
        }

        /// <summary>
        /// this should be a private function but due to threading emails and testing, it needs to be public
        /// </summary>
        /// <param name="inBOE">BOE</param>
        /// <param name="inUserData">active user</param>
        public void PrivateSendWorkspaceAdminEmailAllBOEsApproved(FullBoe inBOE, UserData inUserData)
        {
            if (inBOE == null)
            {
                throw new ArgumentNullException(nameof(inBOE));
            }
            // BODY
            // All BOEs in the Workspace have been approved. <br/> <br/>
            // Workspace/Proposal: {0} <br/>
            // Workspace Status: {1}<br/>
            // {2}

            // get all BOEs relating to the workspace
            FullWorkspace workspaceObject = this.factory.CreateFullWorkspace(inBOE.WorkspaceID);

            // loop through all the BOEs to see if they are approved. 
            foreach (FullBoe boe in workspaceObject.Boes)
            {
                // if a BOE in the workspace hasn't been set to Approved, then just leave this function, no email sent
                if (boe.State != BOEState.Approved)
                {
                    return;
                }
            }

            // get workspace Admins
            Collection<PermissionsDTO> adminRoles = this.PermissionLoader.GetWorkspacePermissions(inBOE.WorkspaceID);

            var wsAdminsLinqResult = from r in adminRoles
                                     where r.Role == Role.WorkspaceAdmin
                                     select r.ETIUserId;
            var wsAdminsDistinct = from r in wsAdminsLinqResult.Distinct()
                                   select this.UserLoader.GetUserByID(r);

            Collection<UserDTO> wsAdmins = new Collection<UserDTO>(wsAdminsDistinct.ToArray());

            string emails = string.Join(", ", wsAdmins.Select(x => x.EmailAddress).ToArray());

            WorkspaceDTO ws = inBOE.Workspace;
            string workspaceState = this.CommonDataMapper.getWorkspaceStateName(ws.WorkspaceState);

            string url = this.GetWorkspaceUrl(ws.Shortname);
            string urlWrapped = @"<a href=""" + url + @""">View Workspace</a>";

            this.SendEmail(EmailTypes.WorkspaceAdminEmailAllBOEsApproved, // email to send
                emails, // recipients (all Workspace Admins)
                null, // no CC's
                new string[] { ws.WorkspaceName }, // subject substitution strings
                new string[]
                {
                    ws.WorkspaceName, /*wsname */ // body subsitution strings,
                    workspaceState, /*workspace state */
                    urlWrapped
                },
                null,
                inUserData,
                inBOE.WorkspaceID);
        }

        /// <summary>
        /// An email is sent to BOE Authors and Approvers for WBSs that are in use
        /// (they should have been checked prior to invocation that they were in Awaiting Approval or Approved states)
        /// </summary>
        /// <param name="inBOE">BOE to email Authors and Approvers about</param>
        /// <param name="inFields">The WBS fields that were changed</param>
        public void SendBOEUpdatedToAuthorsAndApprovers(FullBoe inBOE, ICollection<FieldChanged> inFields)
        {
            // BODY
            // Workspace: {0}<BR/>
            // WBS: {1} {2}<BR/>
            // BOE Title: {12}<BR>
            // CLIN: {3} {4}<BR/>
            // BOE Description: {5}<BR/>
            // BOE Status: {6}<BR/>
            // Author: {7} <BR/>
            // Approver(s): {8}<BR/><BR/>
            // 
            // The following information has changed for the BOE by {9}. As the Author, you must verify the BOE is correct and resubmit it for approval.<BR/>
            // {10}<BR/><BR/>
            // {11}<BR/>

            this._SendBOEUpdated(inBOE, inFields, EmailTypes.BOEUpdatedToAuthorsAndApprovers,
                this.SecurityInformation.ActiveUserData);
        }

        /// <summary>
        /// An email is sent to BOE Authors and Approvers for CLIN that are in use
        /// (they should have been checked prior to invocation that they were in Awaiting Approval or Approved states)
        /// </summary>
        /// <param name="inBOE">BOE to email Authors and Approvers about</param>
        /// <param name="inFields">The CLIN fields that were changed</param>
        public void SendCLINUpdatedToAuthorsAndApprovers(FullBoe inBOE, ICollection<FieldChanged> inFields)
        {
            // BODY
            // Workspace: {0}<BR/>
            // WBS: {1} {2}<BR/>
            // BOE Title: {12}<BR>
            // CLIN: {3} {4}<BR/>
            // BOE Description: {5}<BR/>
            // BOE Status: {6}<BR/>
            // Author: {7} <BR/>
            // Approver(s): {8}<BR/><BR/>
            // 
            // The following information has changed for the BOE by {9}. As the Author, you must verify the BOE is correct and resubmit it for approval.<BR/>
            // {10}<BR/><BR/>
            // {11}<BR/>

            this._SendBOEUpdated(inBOE, inFields, EmailTypes.CLINUpdatedToAuthorsAndApprovers, this.SecurityInformation.ActiveUserData);
        }

        /// <summary>
        /// Sends the BOE updated email.
        /// </summary>
        /// <param name="inBOE">The updated BOE.</param>
        /// <param name="inFields">The fields changed.</param>
        /// <param name="emailType">Type of the email.</param>
        /// <param name="inUserData">The user data.</param>
        private void _SendBOEUpdated(FullBoe inBOE, ICollection<FieldChanged> inFields, EmailTypes emailType,
            UserData inUserData)
        {
            if (!this.DisableAllEmails)
            {
                SendBOEUpdatedDelegate emailDelegate = new SendBOEUpdatedDelegate(this.PrivateSendBOEUpdatedEmails);
                this.DataFetchingScheduler.FetchEmails(emailDelegate,
                    new object[] { inBOE, inFields, emailType, inUserData });
            }
            else // if email is disabled by configuration setting
            {
                this.logger.Debug("Email is disabled by configuration setting in SendBOEUpdated");
            }
        }

        /// <summary>
        /// this should be a private function but due to threading emails and testing, it needs to be public
        /// </summary>
        /// <param name="inBOE">boe</param>
        /// <param name="inFields">in fields</param>
        /// <param name="emailType">email type to send</param>
        /// <param name="inUserData">user</param>
        public void PrivateSendBOEUpdatedEmails(FullBoe inBOE, ICollection<FieldChanged> inFields,
            EmailTypes emailType, UserData inUserData)
        {
            if (inBOE == null)
            {
                throw new ArgumentNullException(nameof(inBOE));
            }

            if (inFields == null)
            {
                throw new ArgumentNullException(nameof(inFields));
            }

            if (inUserData == null)
            {
                throw new ArgumentNullException(nameof(inUserData));
            }

            WorkspaceDTO ws = inBOE.Workspace;

            if (!inBOE.AuthorIDs.Any() && !inBOE.SubcontractorAuthorIDs.Any())
            {
                this.logger.Error("No authors present for BOE [" + inBOE.Description + "] contained in workspace [" +
                           ws.WorkspaceName + "].  NOT sending email to Authors that a WBS was changed.");
            }
            else
            {
                // get the authors and approvers, their emails, display names
                // and stitch together the info for the email
                Collection<UserDTO> approvers = new Collection<UserDTO>();

                var approverList = this.PermissionLoader.GetBOEPermissions(new List<int>() { inBOE.Id })
                    .Where(x => x.Role == Role.Approver).Select(x => x).ToArray();
                foreach (var boeApprover in approverList)
                {
                    approvers.Add(this.UserLoader.GetUserByID(boeApprover.ETIUserId));
                }

                WbsDTO wbs = inBOE.Wbs;
                string wbsNum = wbs == null ? "NO WBS" : wbs.WbsNumber;
                string wbsTitle = wbs == null ? "NO WBS" : wbs.WbsTitle;

                ClinDTO clin = inBOE.Clin;
                string clinNum = clin == null ? "NO CLIN" : clin.ClinNumber;
                string clinTitle = clin == null ? "NO CLIN" : clin.ClinTitle;

                string url = this.GetWorkspaceUrl(ws.Shortname) + @"BOE/EditBOEIndex/boe/" + inBOE.Id;
                string urlWrapped = @"<a href=""" + url + @""">View BOE</a>";

                string authorEmails = string.Empty;
                string authorDisplayNames = string.Empty;

                if (inBOE.AuthorIDs.Any() || inBOE.SubcontractorAuthorIDs.Any())
                {
                    List<UserDTO> authors = (from a in this.PermissionLoader.GetBOEPermissions(new List<int>() { inBOE.Id })
                            .Where(x => x.Role == Role.Author || x.Role == Role.SubcontractorAuthor).Select(x => x)
                            .ToArray()
                                  select this.UserLoader.GetUserByID(a.ETIUserId)).ToList();
                    authorEmails = string.Join("; ", authors.Select(x => x.EmailAddress));
                    authorDisplayNames = string.Join("; ", authors.Select(x => x.DisplayName));

                }

                string tableHtml = _GetTableForBOEUpdates(inFields);

                this.SendEmail(emailType,
                    authorEmails, // recipients (authors)
                    emailType == EmailTypes.BOEUpdatedToAuthorsAndApprovers
                        ? approvers
                        : new Collection<UserDTO>(), // no CC's
                    new string[] { }, // subject substitution strings
                    new string[]
                    {
                        ws.WorkspaceName, /*wsname */ // body subsitution strings
                        wbsNum, wbsTitle, /* wbs#, wbs title */
                        clinNum, clinTitle, /* clin#, clin title */
                        inBOE.Description, /* boe description */ this.CommonDataMapper.getBOEStateName(inBOE.State), /* boe status */
                        authorDisplayNames, /* author names */
                        approvers.Any()
                            ? string.Join(";", approvers.Select(x => x.DisplayName))
                            : "NO APPROVERS", /* approvers name */
                        inUserData.DisplayName, /* user making update */
                        tableHtml, /* table data */
                        urlWrapped, /* link to view BOE */
                        inBOE.Title ?? string.Empty /* boe title */
                    },
                    null,
                    inUserData,
                    inBOE.WorkspaceID);
            }
        }

        /// <summary>
        /// Build an html table for use by the BOEUpdates emails
        /// </summary>
        /// <param name="inFields">WBS fields that have changed</param>
        /// <returns>html table</returns>
        private static string _GetTableForBOEUpdates(ICollection<FieldChanged> inFields)
        {
            // the grid we want to put in the email is derived from rows of the following format
            // Field       |    Old Value         |    New Value
            // ------------|----------------------|----------------
            // WBS #       |      1.1             |    1.2
            // WBS Title   |  Program Management  |    Finance

            StringBuilder tableHtml = new StringBuilder(
                @"<head>
                    <style type=""text/css"">
                                th {
                                    background-color: black;
                                    color: white;
                                }
                    </style>
                </head>
                <body>
                    <table border=""1"">
                        <tr><th>Field</th><th>Old Value</th><th>New Value</th></tr>");

            string rowsAsHtml = @"<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>";
            foreach (var row in inFields)
            {
                // take each row of information and convert it to html
                tableHtml.AppendFormat(rowsAsHtml, row.Field, row.OldValue, row.NewValue);
            }

            tableHtml.Append(
                @"</table>
               </body>");

            return tableHtml.ToString();
        }

        /// <summary>
        /// An email is sent to an Author, Approvers, and Workspace Admin if a BOE has been deleted
        /// from the Manage BOE page
        /// </summary>
        /// <param name="inBOE">The in BOE.</param>
        /// <param name="inDeletedBy">The in deleted by.</param>
        /// <param name="approvers">The approvers.</param>
        /// <param name="wbsAssociatedWithBoe">The WBS associated with BOE.</param>
        /// <param name="clinAssociatedWithBoe">The CLIN associated with BOE.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="boeStateDictionary">The BOE state dictionary.</param>
        public void SendBOEDeleted(BoeDTO inBOE, UserDTO inDeletedBy, Collection<int> approvers,
            WbsDTO wbsAssociatedWithBoe, ClinDTO clinAssociatedWithBoe, FullWorkspace workspace,
            IDictionary<int, BOEStateModelView> boeStateDictionary)
        {
            if (!this.DisableAllEmails)
            {
                SendBOEDeletedDelegate emailDelegate = new SendBOEDeletedDelegate(this.PrivateSendBOEDeleted);
                this.DataFetchingScheduler.FetchEmails(emailDelegate,
                    new object[]
                    {
                        inBOE, inDeletedBy, this.SecurityInformation.ActiveUserData, approvers, wbsAssociatedWithBoe,
                        clinAssociatedWithBoe, workspace, boeStateDictionary
                    });
            }
            else // if email is disabled by configuration setting
            {
                this.logger.Debug("Email is disabled by configuration setting in SendBOEDeleted");
            }
        }

        /// <summary>
        /// this should be a private function but due to threading emails and testing, it needs to be public
        /// </summary>
        /// <param name="inBOE">The BOE to delete.</param>
        /// <param name="inDeletedBy">user who deleted the boe.</param>
        /// <param name="inUserData">active user.</param>
        /// <param name="approvers">Collection of Approver Ids</param>
        /// <param name="wbsAssociatedWithBoe">The WBS associated with BOE.</param>
        /// <param name="clinAssociatedWithBoe">The CLIN associated with BOE.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="boeStateDictionary">The BOE state dictionary.</param>
        public void PrivateSendBOEDeleted(BoeDTO inBOE, UserDTO inDeletedBy, UserData inUserData,
            Collection<int> approvers, WbsDTO wbsAssociatedWithBoe, ClinDTO clinAssociatedWithBoe,
            FullWorkspace workspace, IDictionary<int, BOEStateModelView> boeStateDictionary)
        {
            if (inBOE == null)
            {
                throw new ArgumentNullException(nameof(inBOE));
            }

            if (inDeletedBy == null)
            {
                throw new ArgumentNullException(nameof(inDeletedBy));
            }

            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (boeStateDictionary == null)
            {
                throw new ArgumentNullException(nameof(boeStateDictionary));
            }

            // get workspace permissions
            IReadOnlyCollection<PermissionsDTO> allWorkspaceRoles = workspace.WorkspacePermissions;

            ICollection<int> wsAdminsLinqResult = (from r in allWorkspaceRoles
                                                   where r.Role == Role.WorkspaceAdmin
                                                   select r.ETIUserId).Distinct().ToCollection();

            // lets combine the ids into one set, grab them all
            // then we can create the distinct lists of authors
            Collection<int> subcontractorIds = inBOE.SubcontractorAuthorIDs == null
                ? new Collection<int>()
                : inBOE.SubcontractorAuthorIDs.Distinct().ToCollection();
            Collection<int> authorIds = inBOE.AuthorIDs == null
                ? new Collection<int>()
                : inBOE.AuthorIDs.Distinct().ToCollection();
            Collection<int> approverIds = approvers == null
                ? new Collection<int>()
                : approvers.Distinct().ToCollection();

            Collection<int> allDistinctUserIds = subcontractorIds.Union(authorIds).Union(approverIds)
                .Union(wsAdminsLinqResult).Distinct().ToCollection();
            ICollection<UserDTO> allDistinctUsers = this.UserLoader.GetByIds(allDistinctUserIds);

            Collection<UserDTO> distinctAuthors = (from u in allDistinctUsers
                                                   where authorIds.Contains(u.UserID)
                                                   select u).ToCollection();
            Collection<UserDTO> distinctApprovers = (from u in allDistinctUsers
                                                     where approverIds.Contains(u.UserID)
                                                     select u).ToCollection();

            Collection<UserDTO> distinctAdmins = (from u in allDistinctUsers
                                                  where wsAdminsLinqResult.Contains(u.UserID)
                                                  select u).ToCollection();

            // get workspace Admins
            string approversList = string.Empty;
            if (distinctApprovers != null && distinctApprovers.Any())
            {
                approversList = distinctApprovers.Any()
                    ? string.Join(", ", distinctApprovers.Select(x => x.EmailAddress).ToArray())
                    : string.Empty;
            }

            string authorsList = distinctAuthors.Any()
                ? string.Join(", ", distinctAuthors.Select(x => x.EmailAddress).ToArray())
                : string.Empty;
            string wsAdminList = string.Join(", ", distinctAdmins.Select(x => x.EmailAddress).ToArray());

            // setup email format
            string emails = string.Join(", ", authorsList, approversList, wsAdminList);

            string wbsNum = wbsAssociatedWithBoe == null ? "NO WBS" : wbsAssociatedWithBoe.WbsNumber;
            string wbsTitle = wbsAssociatedWithBoe == null ? "NO WBS" : wbsAssociatedWithBoe.WbsTitle;

            string clinNum = clinAssociatedWithBoe == null ? "NO CLIN" : clinAssociatedWithBoe.ClinNumber;
            string clinTitle = clinAssociatedWithBoe == null ? "NO CLIN" : clinAssociatedWithBoe.ClinTitle;

            string url = this.GetWorkspaceUrl(workspace.Shortname);
            string urlWrapped = @"<a href=""" + url + @""">View Workspace</a>";

            // if the BOE had no author and approvers, then don't send the email at all
            //    // BODY
            //    //The following BOE has been deleted. <br/> <br/>
            //    // Workspace/Proposal: {0}
            //    //WBS: {1} {2}<br/>
            //    //BOE Title: {9}<br/>
            //    //CLIN: {3} {4}<br/>
            //    //BOE Description: {5}<br/>
            //    //BOE Status: {6}<br/>
            //    //Deleted By: {7} <br/>
            //    //{8}
            if (!string.IsNullOrEmpty(authorsList) || !string.IsNullOrEmpty(approversList))
            {
                this.SendEmail(EmailTypes.BOEDeleted, // email to send
                    emails, // recipients (author, approvers, and Workspace Adminstrators)
                    null, // no CC's
                    new string[] { }, // subject substitution strings
                    new string[]
                    {
                        workspace.WorkspaceName, /*wsname */ // body subsitution strings
                        wbsNum, wbsTitle, /* wbs#, wbs title */
                        clinNum, clinTitle, /* clin#, clin title */
                        inBOE.Description, /* boe desc */
                        boeStateDictionary[(int) inBOE.State].BOEState, /* boe status */
                        inDeletedBy.DisplayName,
                        urlWrapped,
                        inBOE.Title ?? string.Empty
                    },
                    null,
                    inUserData,
                    inBOE.WorkspaceID);
            }
        }
                
        /// <summary>
        /// Send email to previous and current BOE Author when an author is changed.
        /// </summary>
        /// <param name="inPreviousAuthors">The previous authors.</param>
        /// <param name="inBOE">The BOE whose author was changed.</param>
        public void SendBOEAuthorsChanged(Collection<UserDTO> inPreviousAuthors, FullBoe inBOE)
        {
            if (!this.DisableAllEmails)
            {
                SendBOEAuthorsChangedDelegate emailDelegate =
                    new SendBOEAuthorsChangedDelegate(this.PrivateSendBOEAuthorsChanged);
                this.DataFetchingScheduler.FetchEmails(emailDelegate,
                    new object[] { inPreviousAuthors, inBOE, this.SecurityInformation.ActiveUserData });
            }
            else // if email is disabled by configuration setting
            {
                this.logger.Debug("Email is disabled by configuration setting in SendBOEAuthorsChanged");
            }
        }

        /// <summary>
        /// Send email to previous and current BOE Author when an author is changed
        /// </summary>
        /// <param name="inPreviousAuthors">The previous authors.</param>
        /// <param name="inBOE">The BOE whose author was changed</param>
        /// <param name="inUserData">The user data.</param>
        public void PrivateSendBOEAuthorsChanged(Collection<UserDTO> inPreviousAuthors, FullBoe inBOE, UserData inUserData)
        {
            if (inBOE == null)
            {
                throw new ArgumentNullException(nameof(inBOE));
            }

            // SUBJECT
            // generation: BOE reassigned to a new Author
            // BODY
            // The following BOE has been reassigned to a new Author.  The newly assigned author may now edit the BOE.<br/>
            // <br/>
            // Workspace/Proposal: {0}<br/>
            // WBS: {1} {2}<br/>
            // BOE Title: {11}<br/>
            // CLIN: {3} {4}<br/>
            // BOE Description: {5}<br/>
            // BOE Status: {6}<br/>
            // Previous Author: {7}<br/>
            // Newly Assigned Author: {8}<br/>
            // Approver(s): {9}<br/>
            // <br/>
            // {10}

            var workspace = inBOE.Workspace;

            WbsDTO wbs = inBOE.Wbs;
            string wbsNum = wbs == null ? "NO WBS" : wbs.WbsNumber;
            string wbsTitle = wbs == null ? "NO WBS" : wbs.WbsTitle;

            ClinDTO clin = inBOE.Clin;
            string clinNum = clin == null ? "NO CLIN" : clin.ClinNumber;
            string clinTitle = clin == null ? "NO CLIN" : clin.ClinTitle;

            Collection<PermissionsDTO> boePermissions = this.PermissionLoader.GetBOEPermissions(new List<int>() { inBOE.Id });

            var newAuthors = (from a in boePermissions
                    .Where(x => x.Role == Role.Author || x.Role == Role.SubcontractorAuthor).Select(x => x).ToArray()
                             select this.UserLoader.GetUserByID(a.ETIUserId)).ToList();

            var approvers = (from a in boePermissions.Where(x => x.Role == Role.Approver).Select(x => x).ToArray()
                            select this.UserLoader.GetUserByID(a.ETIUserId)).ToList();

            string url = this.GetWorkspaceUrl(workspace.Shortname) + @"BOE/EditBOEIndex/boe/" + inBOE.Id;
            string urlWrapped = @"<a href=""" + url + @""">View BOE</a>";

            string authorEmails = string.Join(",", string.Join(",", newAuthors.Select(a => a.EmailAddress).ToArray()));
            this.SendEmail(EmailTypes.BOEAuthorsChanged,
                authorEmails, // To new Authors
                inPreviousAuthors, // CC previous Author
                new string[] { },
                new string[]
                {
                    workspace.WorkspaceName,
                    wbsNum,
                    wbsTitle,
                    clinNum,
                    clinTitle,
                    inBOE.Description, this.CommonDataMapper.getBOEStateName(inBOE.State),
                    inPreviousAuthors.Any() ? string.Join("; ", inPreviousAuthors.Select(x => x.DisplayName)) : "NONE",
                    newAuthors.Any() ? string.Join("; ", newAuthors.Select(x => x.DisplayName)) : "NONE",
                    approvers.Any() ? string.Join("; ", approvers.Select(x => x.DisplayName)) : "NO APPROVERS",
                    urlWrapped,
                    inBOE.Title ?? string.Empty
                },
                null,
                inUserData,
                inBOE.WorkspaceID);
        }

        /// <summary>
        /// Send email to BOE Author and all previous and current Approvers when an approver is changed
        /// </summary>
        /// <param name="inPreviousApprovers">The previous approvers.</param>
        /// <param name="inBOE">The BOE whose approvers were changed</param>
        public void SendBOEApproversChanged(Collection<UserDTO> inPreviousApprovers, FullBoe inBOE)
        {
            if (!this.DisableAllEmails)
            {
                SendBOEApproversChangedDelegate emailDelegate =
                    new SendBOEApproversChangedDelegate(this.PrivateSendBOEApproversChanged);
                this.DataFetchingScheduler.FetchEmails(emailDelegate,
                    new object[] { inPreviousApprovers, inBOE, this.SecurityInformation.ActiveUserData });
            }
            else // if email is disabled by configuration setting
            {
                this.logger.Debug("Email is disabled by configuration setting in SendBOEApproversChanged");
            }
        }

        /// <summary>
        /// this should be a private function but due to threading emails and testing, it needs to be public
        /// </summary>
        /// <param name="inPreviousApprovers">previous approvers</param>
        /// <param name="inBOE">boe</param>
        /// <param name="inUserData">active user</param>
        public void PrivateSendBOEApproversChanged(Collection<UserDTO> inPreviousApprovers, FullBoe inBOE,
            UserData inUserData)
        {
            if (inPreviousApprovers == null)
            {
                throw new ArgumentNullException(nameof(inPreviousApprovers));
            }

            if (inBOE == null)
            {
                throw new ArgumentNullException(nameof(inBOE));
            }

            // SUBJECT
            // generation: BOE reassigned to new Approver(s)
            // BODY
            // The following BOE has been reassigned to new Approver(s) and is awaiting approval as shown in the Approval Status below.<br/>
            // <br/>
            // Workspace/Proposal: {0}<br/>
            // WBS: {1} {2}<br/>
            // BOE Title: {12}<br/>
            // CLIN: {3} {4}<br/>
            // BOE Description: {5}<br/>
            // BOE Status: {6}<br/>
            // Author: {7}<br/>
            // Previous Approver(s): {8}<br/>
            // Newly assigned Approver(s): {9}<br/>
            // Approval Status:<br/>
            // {10}<br/>
            // <br/>
            // {11}

            var workspace = inBOE.Workspace;

            WbsDTO wbs = inBOE.Wbs;
            string wbsNum = wbs == null ? "NO WBS" : wbs.WbsNumber;
            string wbsTitle = wbs == null ? "NO WBS" : wbs.WbsTitle;

            ClinDTO clin = inBOE.Clin;
            string clinNum = clin == null ? "NO CLIN" : clin.ClinNumber;
            string clinTitle = clin == null ? "NO CLIN" : clin.ClinTitle;

            Collection<PermissionsDTO> boePermissions = this.PermissionLoader.GetBOEPermissions(new List<int>() { inBOE.Id });

            var approvers = (from a in boePermissions.Where(x => x.Role == Role.Approver).Select(x => x).ToArray()
                            select this.UserLoader.GetUserByID(a.ETIUserId)).ToList();

            var authors = from a in boePermissions
                    .Where(x => x.Role == Role.Author || x.Role == Role.SubcontractorAuthor).Select(x => x).ToArray()
                          select this.UserLoader.GetUserByID(a.ETIUserId);
            string authorDisplayNames =
                string.Join(",", string.Join(",", authors.Select(a => a.DisplayName).ToArray()));
            List<string> approverLines = new List<string>();

            foreach (BoeApproverResponseDTO a in inBOE.ApproverResponses)
            {
                approverLines.Add("&nbsp;&nbsp;&nbsp;" +
                                  approvers.First(x => x.UserID == a.ETIUserID).DisplayName + ": " +
                                  (a.ApproverResponse == ApproverReponseType.Approved
                                      ? "Approved"
                                      : "Awaiting Approval"));
            }

            string url = this.GetWorkspaceUrl(workspace.Shortname) + @"BOE/EditBOEIndex/boe/" + inBOE.Id;
            string urlWrapped = @"<a href=""" + url + @""">View BOE</a>";

            string emails = string.Join(",", string.Join(",", approvers.Select(a => a.EmailAddress).ToArray()));

            this.SendEmail(EmailTypes.BOEApproversChanged,
                emails, // recipients (current approvers)
                inPreviousApprovers, // CC's (previous approvers)
                new string[] { }, // subject substitution strings
                new string[]
                {
                    workspace.WorkspaceName,
                    wbsNum,
                    wbsTitle,
                    clinNum,
                    clinTitle,
                    inBOE.Description, this.CommonDataMapper.getBOEStateName(inBOE.State),
                    authorDisplayNames,
                    inPreviousApprovers.Any()
                        ? string.Join("; ", inPreviousApprovers.Select(x => x.DisplayName))
                        : "NONE",
                    approvers.Any() ? string.Join("; ", approvers.Select(x => x.DisplayName)) : "NONE",
                    string.Join("<BR/>", approverLines),
                    urlWrapped,
                    inBOE.Title ?? string.Empty
                },
                null,
                inUserData,
                inBOE.WorkspaceID);
        }

        /// <summary>
        /// Send a workspace restored email to Workspace Admins, Authors, Approvers, and Reviewers
        /// </summary>
        /// <param name="inWorkspace">the workspace that was restored</param>
        /// <param name="inRestoredFromTime">the date the version was restored from.</param>
        public void SendWorkspaceRestored(FullWorkspace inWorkspace, DateTime inRestoredFromTime)
        {
            if (!this.DisableAllEmails)
            {
                SendWorkspaceRestoredDelegate emailDelegate =
                    new SendWorkspaceRestoredDelegate(this.PrivateSendWorkspaceRestored);
                this.DataFetchingScheduler.FetchEmails(emailDelegate,
                    new object[] { inWorkspace, inRestoredFromTime, this.SecurityInformation.ActiveUserData });
            }
            else // if email is disabled by configuration setting
            {
                this.logger.Debug("Email is disabled by configuration setting in SendWorkspaceRestored");
            }
        }

        /// <summary>
        /// this should be a private function but due to threading emails and testing, it needs to be public
        /// </summary>
        /// <param name="inWorkspace">the workspace that was restored</param>
        /// <param name="inRestoredFromTime">restored time</param>
        /// <param name="inUserData">active user</param>
        public void PrivateSendWorkspaceRestored(FullWorkspace inWorkspace, DateTime inRestoredFromTime,
            UserData inUserData)
        {
            if (inWorkspace == null)
            {
                throw new ArgumentNullException(nameof(inWorkspace));
            }
            
            // SUBJECT
            // generation: {0} was restored to state as of {1}
            // BODY
            // All contents of the Workspace "{0}" was restored to state as of {1}.  <BR/>     
            // The Workspace status is set to {2}  <BR/> 
            // The status of all BOEs has been set to Draft.  Previously approved BOEs will need to be reapproved.  <BR/> 
            // {3}

            Collection<PermissionsDTO> workspacePermissions = this.PermissionLoader.GetWorkspacePermissions(inWorkspace.Id);

            // get the workspace admimns, authors, approvers, and reviewers
            Collection<PermissionsDTO> workspaceAdmins = (from w in workspacePermissions
                                                          where w.Role == Role.WorkspaceAdmin
                                                          select w).ToCollection();
            Collection<PermissionsDTO> workspaceReviewers = (from w in workspacePermissions
                                                             where w.Role == Role.WorkspaceReviewer
                                                             select w).ToCollection();

            var adminLinqResult = from r in workspaceAdmins
                                  select r.ETIUserId;

            var distinctAdmins = from a in adminLinqResult.Distinct()
                                 select this.UserLoader.GetUserByID(a);

            // need to get the list of actual approvers in the workspace, not the potential list
            Collection<int> boeIds = (from b in inWorkspace.Boes
                                      select b.Id).ToCollection();
            Collection<PermissionsDTO> boePermissions = this.PermissionLoader.GetBOEPermissions(boeIds);

            var approvers = from a in boePermissions.Where(x => x.Role == Role.Approver).Select(x => x).ToArray()
                            select this.UserLoader.GetUserByID(a.ETIUserId);

            var distinctApprovers = approvers.Distinct();

            // need to get the list of actual authors in the workspace, not the potential list
            var authors = from a in boePermissions
                    .Where(x => x.Role == Role.Author || x.Role == Role.SubcontractorAuthor).Select(x => x).ToArray()
                          select this.UserLoader.GetUserByID(a.ETIUserId);

            var distinctAuthors = authors.Distinct();

            var reviewerLinqResult = from r in workspaceReviewers
                                     select r.ETIUserId;

            var distinctReviewers = from a in reviewerLinqResult.Distinct()
                                    select this.UserLoader.GetUserByID(a);

            var distinctUsers = distinctAdmins.Union(distinctApprovers).Union(distinctAuthors).Union(distinctReviewers);

            string emails = string.Join(", ", distinctUsers.Select(x => x.EmailAddress).ToArray());

            string url = this.GetWorkspaceUrl(inWorkspace.Shortname);
            string urlWrapped = @"<a href=""" + url + @""">View Workspace</a>";

            string restoredDateTime = inRestoredFromTime.ToShortDateString() + " " +
                                      inRestoredFromTime.ToShortTimeString();

            this.SendEmail(EmailTypes.WorkspaceRestored,
                emails, // recipients (Workspace Admins)
                null, // CC's (none)
                new string[] { inWorkspace.WorkspaceName, restoredDateTime },
                new string[]
                {
                    inWorkspace.WorkspaceName,
                    restoredDateTime, this.CommonDataMapper.getWorkspaceStateName(inWorkspace.WorkspaceState),
                    urlWrapped
                },
                null,
                inUserData,
                inWorkspace.Id);
        }

        /// <summary>
        /// Send BOE Author an email if a reviewer has added comments to the BOE
        /// </summary>
        /// <param name="inBoe">The affected boe.</param>
        /// <param name="inReviewerID">The id of the BOE Reviewer.</param>
        public void SendBOEAuthorReviewerCommented(FullBoe inBoe, int inReviewerID)
        {
            if (!this.DisableAllEmails)
            {
                SendBOEAuthorReviewerCommentedDelegate emailDelegate =
                    new SendBOEAuthorReviewerCommentedDelegate(this.PrivateSendBOEAuthorReviewerCommented);
                this.DataFetchingScheduler.FetchEmails(emailDelegate,
                    new object[] { inBoe, inReviewerID, this.SecurityInformation.ActiveUserData });
            }
            else // if email is disabled by configuration setting
            {
                this.logger.Debug(
                    "Email is disabled by configuration setting in SendBOEAuthorReviewerCommented");
            }
        }

        /// <summary>
        /// this should be a private function but due to threading emails and testing, it needs to be public
        /// </summary>
        /// <param name="inBoe">boe</param>
        /// <param name="inReviewerID">reviewer</param>
        /// <param name="inUserData">active user</param>
        public void PrivateSendBOEAuthorReviewerCommented(FullBoe inBoe, int inReviewerID, UserData inUserData)
        {
            if (inBoe == null)
            {
                throw new ArgumentNullException(nameof(inBoe));
            }
            // BODY
            // {0} submitted or updated comments for the following BOE:
            // Workspace/Proposal: {1}
            // WBS: {2} {3}
            // BOE Title: {9}
            // CLIN: {4} {5}
            // BOE Status: {6}
            // BOE Description: {7}
            // {8}

            // get author emails
            string emails = string.Empty;
            if (inBoe.AuthorIDs.Any() || inBoe.SubcontractorAuthorIDs.Any())
            {
                var authors = from a in this.PermissionLoader.GetBOEPermissions(new List<int>() { inBoe.Id })
                        .Where(x => x.Role == Role.Author || x.Role == Role.SubcontractorAuthor).Select(x => x)
                        .ToArray()
                              select this.UserLoader.GetUserByID(a.ETIUserId);
                emails = string.Join("; ", authors.Select(x => x.EmailAddress));
            }

            // get reviewer
            UserDTO reviewer = this.UserLoader.GetUserByID(inReviewerID);

            WorkspaceDTO ws = inBoe.Workspace;
            WbsDTO wbs = inBoe.Wbs;
            string wbsNum = wbs == null ? "NO WBS" : wbs.WbsNumber;
            string wbsTitle = wbs == null ? "NO WBS" : wbs.WbsTitle;

            ClinDTO clin = inBoe.Clin;
            string clinNum = clin == null ? "NO CLIN" : clin.ClinNumber;
            string clinTitle = clin == null ? "NO CLIN" : clin.ClinTitle;

            string url = this.GetWorkspaceUrl(ws.Shortname) + @"BOE/EditBOEIndex/boe/" + inBoe.Id + @"#Comments";
            string urlWrapped = @"<a href=""" + url + @""">View comments</a>";

            this.SendEmail(EmailTypes.BOEAuthorReviewerCommented, // email to send
                emails, // recipients (author)
                null, // no CC's
                new string[] { reviewer.DisplayName }, // subject substitution strings
                new string[]
                {
                    reviewer.DisplayName, // author who commented
                    ws.WorkspaceName, /*wsname */ // body subsitution strings
                    wbsNum, wbsTitle, /* wbs#, wbs title */
                    clinNum, clinTitle, /* clin#, clin title */ this.CommonDataMapper.getBOEStateName(inBoe.State), /* boe state*/
                    inBoe.Description, /* boe desc */
                    urlWrapped,
                    inBoe.Title ?? string.Empty
                },
                null,
                inUserData,
                inBoe.WorkspaceID);
        }

        /// <summary>
        /// Send BOE Authors and Approvers an email that an In Use Resource has been updated
        /// </summary>
        /// <param name="inResource">in use resource that was updated</param>
        /// <param name="inFieldChanges">resource changes</param>
        /// <param name="inWorkspaceAdmin"> admin who made the resource changed</param>
        /// <param name="inWorkspace">workspace the resource resides in</param>
        public void SendBOEAuthorsApproversInUseResourceUpdated(ResourceDTO inResource,
            Collection<FieldChanged> inFieldChanges, UserDTO inWorkspaceAdmin, FullWorkspace inWorkspace)
        {
            if (!this.DisableAllEmails)
            {
                SendBOEAuthorsApproversInUseResourceUpdatedDelegate emailDelgate =
                    new SendBOEAuthorsApproversInUseResourceUpdatedDelegate(this
                        .PrivateSendBOEAuthorsApproversInUseResourceUpdated);
                this.DataFetchingScheduler.FetchEmails(emailDelgate,
                    new object[]
                    {
                        inResource, inFieldChanges, inWorkspaceAdmin, inWorkspace, this.SecurityInformation.ActiveUserData
                    });
            }
            else // if email is disabled by configuration setting
            {
                this.logger.Debug("Email is disabled by configuration setting in SendBOEAuthorsApproversInUseResourceUpdated");
            }
        }

        /// <summary>
        /// this should be a private function but due to threading emails and testing, it needs to be public
        /// </summary>
        /// <param name="inResource">in use resource that was updated</param>
        /// <param name="inFieldChanges">resource changes</param>
        /// <param name="inWorkspaceAdmin"> admin who made the resource changed</param>
        /// <param name="inWorkspace">workspace the resource resides in</param>
        /// <param name="inUserData">active user</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
            "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public void PrivateSendBOEAuthorsApproversInUseResourceUpdated(ResourceDTO inResource,
            Collection<FieldChanged> inFieldChanges, UserDTO inWorkspaceAdmin, FullWorkspace inWorkspace,
            UserData inUserData)
        {
            if (inResource == null)
            {
                throw new ArgumentNullException(nameof(inResource));
            }

            if (inWorkspaceAdmin == null)
            {
                throw new ArgumentNullException(nameof(inWorkspaceAdmin));
            }

            if (inWorkspace == null)
            {
                throw new ArgumentNullException(nameof(inWorkspace));
            }

            // BODY
            // {0} has modified resource {1} in the following BOEs. If additional changes are needed to the BOEs, please request the Workspace Administrator move the BOEs back to Draft state. <BR/> 
            // Workspace/Proposal: {2} <BR/> <BR/>
            // Resource Changes <BR/>   <BR/>
            // {3} <BR/> TABLE of RESOURCE CHANGES
            // {4}  DYNAMIC LIST OF ALL WBS, CLIN, BOES THAT USE RESOURCE <BR/>

            string emails = string.Empty;
            Collection<UserDTO> approvers = new Collection<UserDTO>();

            string tableHtml = _GetTableForInUseResourceUpdates(inFieldChanges);
            string boeData = string.Empty;

            ICollection<int> boeIDs = this.boeLoader.GetIdsByResourceId(inResource.Id);
            ICollection<FullBoe> boes = this.factory.CreateFullBoes(boeIDs);
            Collection<PermissionsDTO> boePermissions = this.PermissionLoader.GetBOEPermissions(boeIDs);
            IDictionary<int, BOEStateModelView> boeStates = this.CommonDataMapper.getBOEStatesDictionary();

            // get all BOEs effected by resource changed
            foreach (FullBoe boe in boes)
            {
                // if the boe isn't Approved or Awaiting Approval, do not add this BOE to the email
                if (boe.State.Equals(BOEState.Approved) || boe.State.Equals(BOEState.AwaitingApproval))
                {
                    var authors = from a in boePermissions
                            .Where(x => x.Role == Role.Author || x.Role == Role.SubcontractorAuthor).Where(x => x.BOEId == boe.Id).Select(x => x)
                            .ToArray()
                                  select this.UserLoader.GetUserByID(a.ETIUserId);
                    emails = string.Join("; ", authors.Select(x => x.DisplayName));
                    var boeApprovers = boePermissions.Where(x => x.Role == Role.Approver).Where(x => x.BOEId == boe.Id).Select(x => x).ToArray();
                    var approvers2 = from a in boeApprovers
                                     select this.UserLoader.GetUserByID(a.ETIUserId);
                    approvers = new Collection<UserDTO>(approvers2.ToArray());

                    // get BOE info that will be all strung together in the final email (not in table format)
                    WbsDTO wbs = boe.Wbs;
                    string wbsNum = wbs == null ? "NO WBS" : wbs.WbsNumber;
                    string wbsTitle = wbs == null ? "NO WBS" : wbs.WbsTitle;

                    ClinDTO clin = boe.Clin;
                    string clinNum = clin == null ? "NO CLIN" : clin.ClinNumber;
                    string clinTitle = clin == null ? "NO CLIN" : clin.ClinTitle;

                    boeData = boeData + "<BR/>" + "WBS: " +
                              Utilities.FormatNumberTitleString(wbsNum, wbsTitle, " ") + "<BR/>";
                    boeData = boeData + "BOE Title: " + boe.Title + "<BR/>";
                    boeData = boeData + "CLIN: " +
                              Utilities.FormatNumberTitleString(clinNum, clinTitle, " ") + "<BR/>";
                    boeData = boeData + "BOE Description: " + boe.Description + "<BR/>";
                    boeData = boeData + "BOE Status: " + boeStates[(int)boe.State].BOEState + "<BR/>";

                    string url = this.GetWorkspaceUrl(inWorkspace.Shortname) + @"BOE/EditBOEIndex/boe/" + boe.Id;
                    string urlWrapped = @"<a href=""" + url + @""">View BOE</a>";
                    boeData = boeData + urlWrapped + "<BR/><BR/>";
                }
            }

            // if the resource is in use, but not by an BOEs in Approved or Awaiting Approval state, don't send email
            if (boeData.Contains("WBS"))
            {
                this.SendEmail(EmailTypes.BOEAuthorsApproversInUseResourceUpdated, // email to send
                    emails, // recipients (author)
                    approvers, // CC approvers
                    new string[] { inWorkspaceAdmin.DisplayName }, // subject substitution strings
                    new string[]
                    {
                        inWorkspaceAdmin.DisplayName, // admin who changed resource,
                        inResource.ResourceDesc, /*resource desc*/
                        inWorkspace.WorkspaceName, /*workspace name  */ // body subsitution strings
                        tableHtml, /* table of resource field changes */
                        boeData /* dynamic BOE Data inclues WBS, BOE Title CLIN, BOE Desc, Boe Status, and View BOE URl */
                    },
                    null,
                    inUserData,
                    inWorkspace.Id);
            }
        }

        /// <summary>
        /// Send BOE Authors and Approvers an email that dates have been updated
        /// </summary>
        /// <param name="userDateChangeInfo">user date change (adjust) information.</param>
        /// <param name="isErrorEmail">Whether to use the error or normal dateshift email</param>
        public void SendBOEAuthorsApproversDatesUpdated(HashSet<UserDateChangeInfo> userDateChangeInfo, bool isErrorEmail)
        {
            if (userDateChangeInfo.Any())
            {
                if (!this.DisableAllEmails)
                {
                    SendBOEAuthorsApproversDatesUpdatedDelegate emailDelgate =
                        new SendBOEAuthorsApproversDatesUpdatedDelegate(this.PrivateSendBOEAuthorsApproversDatesUpdated);
                    this.DataFetchingScheduler.FetchEmails(emailDelgate,
                        new object[] { userDateChangeInfo, isErrorEmail, this.SecurityInformation.ActiveUserData });
                }
                else // if email is disabled by configuration setting
                {
                    this.logger.Debug("Email is disabled by configuration setting in SendBOEAuthorsApproversDatesUpdated");
                }
            }
        }

        /// <summary>
        /// this should be a private function but due to threading emails and testing, it needs to be public
        /// </summary>
        /// <param name="userDateChangeInfo">The user date change (adjust) information.</param>
        /// <param name="isErrorEmail">Whether to use the error or normal dateshift email</param>
        /// <param name="inUserData">active user</param>
        /// <exception cref="System.ArgumentNullException">userDateChangeInfo</exception>
        public void PrivateSendBOEAuthorsApproversDatesUpdated(HashSet<UserDateChangeInfo> userDateChangeInfo,
            bool isErrorEmail, UserData inUserData)
        {
            if (userDateChangeInfo == null)
            {
                throw new ArgumentNullException(nameof(userDateChangeInfo));
            }

            Collection<UserDTO> approvers = new Collection<UserDTO>();

            foreach (UserDateChangeInfo user in userDateChangeInfo)
            {
                HashSet<DateChangeInfo> changeInfo = user.GetAllDateChangeInfo();

                string workspaceUrl = this.GetWorkspaceUrl(user.workspaceName);
                string workspaceUrlWrapped = @"<a href=""" + workspaceUrl + @""">View Workspace</a>";

                string tableHtml = this._GetTableForDateUpdates(changeInfo, user.workspaceName);

                this.SendEmail( isErrorEmail ? EmailTypes.BOEAuthorsApproversDatesUpdatedError : EmailTypes.BOEAuthorsApproversDatesUpdated, // email to send
                    user.email, // recipients (author)
                    approvers, // CC approvers
                    new string[] { user.changedBy, user.dateShiftType /* date shift type */ }, // subject substitution strings
                    new string[]
                    {
                        user.changedBy, // admin who changed resource,
                        user.workspaceName, /*workspace name  */ // body subsitution strings
                        workspaceUrlWrapped,
                        tableHtml /* table of resource field changes */
                    },
                    null,
                    inUserData,
                    user.workspaceId);
            }
        }

        /// <summary>
        /// Gets the table for date updates.
        /// </summary>
        /// <param name="changeInfo">The change information.</param>
        /// <param name="inWorkspaceName">Name of the in workspace.</param>
        /// <returns>An html table for date updates.</returns>
        private string _GetTableForDateUpdates(HashSet<DateChangeInfo> changeInfo, string inWorkspaceName)
        {
            // the grid we want to put in the email is derived from rows of the following format
            // Field            |    Old Value         |    New Value
            // -----------------|----------------------|----------------
            // Description      |      1CE1- DS        |    1DE2-ES
            // Segment Region   |       1C             |    1D
            // Labor Type       |       E1             |    E2
            // Segment          |       DS             |    ES

            StringBuilder tableHtml = new StringBuilder(
                @"<head>
                    <style type=""text/css"">
                                th {
                                    background-color: black;
                                    color: white;
                                }
                    </style>
                </head>
                <body>
                    <table border=""1"">
                        <tr><th>WBS</th><th>CLIN</th><th>Old Start Date</th><th>Old End Date</th><th>New Start Date</th><th>New End Date</th></tr>");

            string rowsAsHtml = @"<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td></tr>";

            foreach (DateChangeInfo row in changeInfo)
            {
                // take each row of information and convert it to html
                string url = this.GetWorkspaceUrl(inWorkspaceName) + @"BOE/EditBOEIndex/boe/" + row.boeID;
                string wbsUrlWrapped;
                if (!string.IsNullOrEmpty(row.wbsDisplayID) || !string.IsNullOrEmpty(row.wbsTitle))
                {
                    wbsUrlWrapped = @"<a href=""" + url + @"""><" + row.wbsDisplayID + "><" + row.wbsTitle + "></a>";
                }
                else
                {
                    wbsUrlWrapped = string.Empty;
                }

                string clinUrlWrapped;
                if (!string.IsNullOrEmpty(row.clinDisplayID) || !string.IsNullOrEmpty(row.clinTitle))
                {
                    clinUrlWrapped = @"<a href=""" + url + @"""><" + row.clinDisplayID + "><" + row.clinTitle + "></a>";
                }
                else
                {
                    clinUrlWrapped = string.Empty;
                }

                tableHtml.AppendFormat(rowsAsHtml, wbsUrlWrapped, clinUrlWrapped,
                    row.originalStartDate.ToString("MM/dd/yyyy"), row.originalEndDate.ToString("MM/dd/yyyy"),
                    row.newStartDate.ToString("MM/dd/yyyy"), row.newEndDate.ToString("MM/dd/yyyy"));
            }

            tableHtml.Append(
                @"</table>
               </body>");

            return tableHtml.ToString();
        }

        /// <summary>
        /// Gets the table for in use resource updates.
        /// </summary>
        /// <param name="inFields">The in fields.</param>
        /// <returns>The table for In-Use Resource updates.</returns>
        private static string _GetTableForInUseResourceUpdates(ICollection<FieldChanged> inFields)
        {
            // the grid we want to put in the email is derived from rows of the following format
            // Field            |    Old Value         |    New Value
            // -----------------|----------------------|----------------
            // Description      |      1CE1- DS        |    1DE2-ES
            // Segment Region   |       1C             |    1D
            // Labor Type       |       E1             |    E2
            // Segment          |       DS             |    ES

            StringBuilder tableHtml = new StringBuilder(
                @"<head>
                    <style type=""text/css"">
                                th {
                                    background-color: black;
                                    color: white;
                                }
                    </style>
                </head>
                <body>
                    <table border=""1"">
                        <tr><th>Field</th><th>Old Value</th><th>New Value</th></tr>");

            string rowsAsHtml = @"<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>";

            foreach (var row in inFields)
            {
                // take each row of information and convert it to html
                tableHtml.AppendFormat(rowsAsHtml, row.Field, row.OldValue, row.NewValue);
            }

            tableHtml.Append(
                @"</table>
               </body>");

            return tableHtml.ToString();
        }

        /// <summary>
        /// Get the workspace URL for a given workspacename
        /// </summary>
        /// <param name="workspaceShortName">The workspace shortname.</param>
        /// <returns>The workspace Url.</returns>
        private string GetWorkspaceUrl(string workspaceShortName)
        {
            // attempt to get the request context .. DNE in mock testing though
            // so just default to our dev environment and let 'real' ASP take
            // care of things when running within IIS
            string serverUrl = ConfigurationUtilities.GetAppSetting("ServerURL");
            string protocol = serverUrl.StartsWith(@"http://") || serverUrl.StartsWith(@"https://") ? string.Empty : @"http://";
            string url = protocol + serverUrl + "/" + workspaceShortName + @"/";

            return url;
        }

        /// <summary>
        /// Sends email messages based on parameters and uses Host
        /// Validation Note:
        /// - The method will validate that the # of tokens in the subject/body are equal to the number of tokens sent in
        /// (i.e. "Hi, {0}, I'm a message" should have 1 value in the *ReplaceTokens array)
        /// Special Logic:
        /// - If the web.config contains a true value for "EmailsToCurrentlyLoggedInUser",
        /// this method will attempt to locate the currently logged in users' email.
        /// General Note:
        /// Check your spam folder as it appears messages will go there by default!
        /// </summary>
        /// <param name="inEmailTypeToSend">The email to send</param>
        /// <param name="inRecipient">recipient(s) of the email (use comma between names if multiple)</param>
        /// <param name="inCClist">CC list for the email</param>
        /// <param name="inSubjectReplaceTokens">tokens in the subject email to replace, empty if none to replace</param>
        /// <param name="inBodyReplaceTokens">tokens in the body of the email to replace, empty if none to replace</param>
        /// <param name="inAttachment">attachment to add, null if none to attach</param>
        /// <param name="inUserData">The user data.</param>
        /// <param name="workspaceId">The workspace id, used for finding overrides from system email settings.</param>
       public void SendEmail(EmailTypes inEmailTypeToSend, string inRecipient, Collection<UserDTO> inCClist,
            string[] inSubjectReplaceTokens, string[] inBodyReplaceTokens, Attachment inAttachment, UserData inUserData, int workspaceId)
        {
            if (!this.DisableAllEmails)
            {
                if (inSubjectReplaceTokens == null)
                {
                    throw new ArgumentNullException(nameof(inSubjectReplaceTokens));
                }

                if (inBodyReplaceTokens == null)
                {
                    throw new ArgumentNullException(nameof(inBodyReplaceTokens));
                }

                // if passed in as null just make sure it's an empty array so we can manage easier
                if (inCClist == null)
                {
                    inCClist = new Collection<UserDTO>();
                }

                this.logger.Debug("EMAIL - Entering sendemail for " + inEmailTypeToSend.ToString() + " to recipient [" +
                           inRecipient + "]");

                // get the email to send so we can validate the number of tokens to replace in the email
                // is the same as the number of tokens we were passed in to replace
                EmailModelDomain emailFromDb = (from e in this.CommonDataMapper.GetEmails()
                                                where e.EmailType == inEmailTypeToSend
                                                select e).First();

                // Get the workspaceOverride (if any) for this emailtype
                WorkspaceEmailOverrideDTO workspaceOverride = this.workspaceLoader.GetWorkspaceEmailOverrides(workspaceId).FirstOrDefault(o => o.EmailType == inEmailTypeToSend);

                bool sendEmail = emailFromDb.DefaultOn;
                if (!emailFromDb.Forced && workspaceOverride != null)
                {
                    sendEmail = workspaceOverride.TurnOn;
                }

                if (sendEmail)
                {
                    // convert database email to our struct and then pass along to other 'worker' method
                    EmailContent emailToSend = new EmailContent
                    {
                        Body = emailFromDb.Body,
                        Subject = emailFromDb.Subject
                    };

                    base.SendEmail(emailToSend, inRecipient, this.ConvertCCList(inCClist), inSubjectReplaceTokens, inBodyReplaceTokens,
                        inAttachment, inUserData);
                }
            }
        }

        /// <summary>
        /// Convert User DTO cc list to UserData
        /// </summary>
        /// <param name="ccList">list of DTOs to convert</param>
        /// <returns>cc list as Collection of UserData</returns>
        private ICollection<UserData> ConvertCCList(ICollection<UserDTO> ccList)
        {
            ICollection<UserData> ccData = new Collection<UserData>();

            foreach (UserDTO cc in ccList)
            {
                ccData.Add(new UserData()
                {
                    Email = cc.EmailAddress,
                    Ntid = cc.NTID,
                    DisplayName = cc.DisplayName
                });
            }

            return ccData;
        }
    }
}

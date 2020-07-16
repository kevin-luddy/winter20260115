// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Web.Mvc;
    using GenBOE.DataBridge.DTO;
    using GenTRAC.ActionLogic.GeneralHelper;
    using GenTRAC.ActionLogic.Mediator;
    using GenTRAC.ActionLogic.ModelView.Admin;
    using GenTRAC.ActionLogic.Validation;
    using GenTRAC.DataBridge.Common.Security;
    using GenTRAC.DataBridge.DTO;
    using GenTRAC.Objects;
    using GenTRAC.Objects.FullObject;
    using IES.Common;
    using IES.Common.Exceptions;
    using IES.Common.PickList;

    /// <summary>
    /// The business logic class for the Admin Controller
    /// </summary>
    public class AdminControllerLogic : GenTRACControllerLogic
    {
        #region Properties

        /// <summary>
        /// The logger
        /// </summary>
        private readonly IES.Common.Logger log = new IES.Common.Logger(typeof(AdminControllerLogic));

        /// <summary>
        /// Active Directory Utilities
        /// </summary>
        private readonly IES.Common.IActiveDirectoryUtilities adUtils = null;

        /// <summary>
        /// User Mediator
        /// </summary>
        private readonly IUserMediator userMediator = null;

        /// <summary>
        /// Permissions Mapper
        /// </summary>
        private readonly ISystemPermissionMapper permissionsMapper = null;

        /// <summary>
        /// Permissions Mediator
        /// </summary>
        private readonly ISystemPermissionMediator permissionsMediator = null;

        /// <summary>
        /// Html Helper 
        /// </summary>
        private readonly IHtmlHelper htmlHelper = null;

        /// <summary>
        /// org structure data mapper
        /// </summary>
        private readonly IOrgStructureDataMapper orgStructureDataMapper = null;

        /// <summary>
        /// Manage Proposal Info Loader
        /// </summary>
        private readonly IManageProposalInfoLoader manageProposalInfoLoader = null;

        /// <summary>
        /// The workspace dto data loader.
        /// </summary>
        private readonly GenBOE.DataBridge.DTO.IWorkspaceDTODataLoader workspaceDTODataLoader;

        /// <summary>
        /// Bulk Archive Loader
        /// </summary>
        private readonly IBulkArchiveLoader bulkArchiveLoader = null;

        /// <summary>
        /// Approvals Controller Logic
        /// </summary>
        private readonly ApprovalsControllerLogic approvalsControllerLogic = null;

        /// <summary>
        /// The name of the manage proposal info form, needed for validation
        /// </summary>
        public const string MANAGE_PROPOSAL_INFO_FORM = "manageProposalInfoForm";

        /// <summary>
        /// The name of the Bulk Archive form, needed for validation
        /// </summary>
        public const string BULK_ARCHIVE_FORM = "BulkArchiveForm";

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inSecurityAccess">Security Access</param>
        /// <param name="inProposalLoader">Proposal Loader</param>
        /// <param name="htmlHelper">Html Helper</param>
        /// <param name="inUserMapper">User Mapper</param>
        /// <param name="activeDirectoryUtils">Active Directory Utilities</param>
        /// <param name="userMediator">User Mediator</param>
        /// <param name="inPermissionsMapper">Permissions Mapper</param>
        /// <param name="inPermissionMediator">Permissions Mediator</param>
        /// <param name="objectFactory">Object Factory</param>
        /// <param name="inOrgStructureMapper">Org structure mapper</param>
        /// <param name="inManageProposalInfoLoader">Manage Proposal Info Loader</param>
        /// <param name="inBulkArchiveLoader">Bulk Archive Loader</param>
        /// <param name="approvalsLoader">Approvals Loader</param>
        /// <param name="proposalChecklistLoader">Proposal Checklist Loader</param>
        /// <param name="inChecklistMediator">Checklist Mediator</param>
        /// <param name="inProposalMediator">Proposal Mediator</param>
        /// <param name="workspaceDTODataLoader">The Workspace DTO Data Loader</param>
        /// <param name="approvalsControllerLogic">Approvals controller logic</param>
        public AdminControllerLogic(
            ISecurityAccess inSecurityAccess,
            IProposalLoader inProposalLoader,
            IHtmlHelper htmlHelper,
            IUserMapper inUserMapper,
            IES.Common.IActiveDirectoryUtilities activeDirectoryUtils,
            IUserMediator userMediator,
            ISystemPermissionMapper inPermissionsMapper,
            ISystemPermissionMediator inPermissionMediator,
            IFullObjectFactory objectFactory,
            IOrgStructureDataMapper inOrgStructureMapper,
            IManageProposalInfoLoader inManageProposalInfoLoader,
            IBulkArchiveLoader inBulkArchiveLoader,
            IApprovalsLoader approvalsLoader,
            IProposalChecklistLoader proposalChecklistLoader,
            IChecklistMediator inChecklistMediator,
            IProposalMediator inProposalMediator,
            IWorkspaceDTODataLoader workspaceDTODataLoader,
            ApprovalsControllerLogic approvalsControllerLogic)
            : base(inSecurityAccess, inProposalLoader, inUserMapper, objectFactory, approvalsLoader, proposalChecklistLoader, inChecklistMediator, inProposalMediator)
        {
            this.adUtils = activeDirectoryUtils;
            this.userMediator = userMediator;
            this.permissionsMapper = inPermissionsMapper;
            this.permissionsMediator = inPermissionMediator;
            this.htmlHelper = htmlHelper;
            this.orgStructureDataMapper = inOrgStructureMapper;
            this.manageProposalInfoLoader = inManageProposalInfoLoader;
            this.bulkArchiveLoader = inBulkArchiveLoader;
            this.workspaceDTODataLoader = workspaceDTODataLoader;
            this.approvalsControllerLogic = approvalsControllerLogic;
        }

        #region System Admin

        /// <summary>
        /// Get the model view for managing permissions
        /// </summary>
        /// <returns>Manage Permissions model view</returns>
        public ManagePermissionsModelView GetManagePermissionsModelView()
        {
            ManagePermissionsModelView model = new ManagePermissionsModelView();
            model.ViewerLinesOfBusinessList = new Collection<SelectListItem>();
            model.ProposalSetupAdminLinesOfBusinessList = new Collection<SelectListItem>();
            ICollection<PickListDto> linesOfBusiness = this.orgStructureDataMapper.GetAllLinesOfBusiness().PropertyNameSort("Text").ToList();
            foreach (PickListDto lineOfBusiness in linesOfBusiness)
            {
                model.ViewerLinesOfBusinessList.Add(new SelectListItem() { Value = lineOfBusiness.Id.ToString(), Text = lineOfBusiness.Text });
                model.ProposalSetupAdminLinesOfBusinessList.Add(new SelectListItem() { Value = lineOfBusiness.Id.ToString(), Text = lineOfBusiness.Text });
            }

            return model;
        }

        /// <summary>
        /// Default Bulk Archive Model View that's populated with all Lines of Business and the "All" Program Area
        /// </summary>
        /// <returns>Bulk Archive Model View</returns>
        public BulkArchiveModelView GetBulkArchiveModelView()
        {
            BulkArchiveModelView bulkArchiveModelView = new BulkArchiveModelView();

            ICollection<PickListDto> linesOfBusiness = this.orgStructureDataMapper.GetAllLinesOfBusiness();

            // Find an acceptable option index for the "All" drop-down element
            int lineOfBusinessSelectOptionIndex = 0;

            // Add the "All" Line of Business option
            bulkArchiveModelView.LineOfBusinessOptions.Add(new SelectListItem() { Value = lineOfBusinessSelectOptionIndex.ToString(), Text = "All" });

            foreach (PickListDto lineOfBusiness in linesOfBusiness)
            {
                bulkArchiveModelView.LineOfBusinessOptions.Add(new SelectListItem() { Value = lineOfBusiness.Id.ToString(), Text = lineOfBusiness.Text });
                lineOfBusinessSelectOptionIndex = lineOfBusiness.Id + 1;
            }

            // Populate the Program Area Drop-down
            bulkArchiveModelView.ProgramAreaOptions.Add(new SelectListItem() { Value = "0", Text = "All" });

            return bulkArchiveModelView;
        }

        /// <summary>
        /// Returns a permissions grid model view
        /// </summary>
        /// <param name="permissionsGridModelView">Permissions Grid Model View</param>
        /// <returns>Populated Permissions grid model view</returns>
        public PermissionsGridModelView GetManagePermissionsGridModelView(PermissionsGridModelView permissionsGridModelView)
        {
            if (permissionsGridModelView == null)
            {
                throw new ArgumentNullException(nameof(permissionsGridModelView));
            }

            ICollection<SystemPermissionDto> allSystemPermissionRoles = this.permissionsMapper.GetSystemPermissions();

            // Get the unique userIds so we can iterate over them to get the roles for each user
            ICollection<int> distinctUserIds = (from x in allSystemPermissionRoles
                                                select x.UserId).Distinct().ToList();

            permissionsGridModelView.PermissionData.Clear();
            ICollection<PickListDto> linesOfBusiness = this.orgStructureDataMapper.GetAllLinesOfBusiness();
            foreach (int x in distinctUserIds)
            {
                permissionsGridModelView.PermissionData.Add(this.GetUserPermissionDataByUserId(x, allSystemPermissionRoles, linesOfBusiness));
            }

            // Sort by UserType descending (so groups appear first), then by UserName.
            permissionsGridModelView.PermissionData = permissionsGridModelView.PermissionData
                .OrderBy(x => x.UserType, IES.Common.SortOrder.Descending)
                .ThenBy(x => x.UserName).ToList();

            return permissionsGridModelView;
        }

        /// <summary>
        /// Returns a user's system permissions
        /// </summary>
        /// <param name="userId">The User Id</param>
        /// <param name="allSystemPermissions">The system permissions for the user</param>
        /// <param name="allLinesOfBusiness">All LOBs.</param>
        /// <returns>ModelView for the system permissions data</returns>
        private PermissionsModelView GetUserPermissionDataByUserId(int userId, ICollection<SystemPermissionDto> allSystemPermissions,
            ICollection<PickListDto> allLinesOfBusiness)
        {
            UserDTO theUser = this.UserMapper.GetById(userId);

            PermissionsModelView toReturn = new PermissionsModelView();

            if (theUser != null)
            {
                toReturn.UserId = theUser.Id;
                toReturn.UserType = theUser.UserType;
                toReturn.UserName = theUser.DisplayName;
                toReturn.UserNtId = theUser.Ntid;

                // Get all the permissions for the user
                SystemPermissionDto[] permissionsForUser = (from x in allSystemPermissions
                                          where x.UserId == userId
                                          select x).ToArray();

                Collection<PtmRole> rolesForUser = new Collection<PtmRole>();

                // Select all the roles the user has access to.
                IEnumerable<PtmRole> distinctRoles = (from x in permissionsForUser
                                     select x.Role).Distinct();

                foreach (PtmRole distinctRole in distinctRoles)
                {
                    // Add the role to the modelview for the user.
                    rolesForUser.Add(distinctRole);
                }

                toReturn.Roles = rolesForUser;

                // Set up the Lines of Business for the Viewer Role.
                SystemPermissionDto viewerRole = (from x in permissionsForUser
                                                  where x.Role == PtmRole.Viewer
                                                  select x).FirstOrDefault();

                if (viewerRole != null)
                {
                    toReturn.ViewerLinesOfBusiness = viewerRole.LineOfBusinessIDs;
                    ICollection<string> lineOfBusinessNames = allLinesOfBusiness.Where(x => viewerRole.LineOfBusinessIDs.Contains(x.Id)).Select(x => x.Text).ToList();
                    toReturn.ViewerLinesOfBusinessDisplay = string.Format("Viewer ({0})", string.Join(", ", lineOfBusinessNames));
                }

                // Set up the Lines of Business for the Proposal Setup Admin Role.
                SystemPermissionDto proposalSetupAdminRole = (from x in permissionsForUser
                                                        where x.Role == PtmRole.ProposalSetupAdmin
                                                        select x).FirstOrDefault();

                if (proposalSetupAdminRole != null)
                {
                    toReturn.ProposalSetupAdminLinesOfBusiness = proposalSetupAdminRole.LineOfBusinessIDs;
                    ICollection<string> lineOfBusinessNames = allLinesOfBusiness.Where(x => proposalSetupAdminRole.LineOfBusinessIDs.Contains(x.Id)).Select(x => x.Text).ToList();
                    toReturn.ProposalSetupAdminLinesOfBusinessDisplay = string.Format("Proposal Setup Admin ({0})", string.Join(", ", lineOfBusinessNames));
                }
            }

            // Return the user info.
            return toReturn;
        }

        /// <summary>
        /// Saves an edit to a users permission roles
        /// </summary>
        /// <param name="inModelViewsToSave">The modelViews to Save</param>
        /// <returns>Collection of select list items</returns>
        public bool SaveUserPermissionRoles(Collection<PermissionsModelView> inModelViewsToSave)
        {
            if (inModelViewsToSave != null)
            {
                // Remove duplicate NTID records
                List<string> distinctNTIDs = inModelViewsToSave.Select(a => a.UserNtId).Distinct().ToList();
                foreach (string ntid in distinctNTIDs)
                {
                    if (inModelViewsToSave.Count(a => a.UserNtId == ntid) > 1)
                    {
                        inModelViewsToSave.Remove(inModelViewsToSave.First(a => a.UserNtId == ntid));
                    }
                }

                Collection<SystemPermissionDto> allPermissionsToSave = new Collection<SystemPermissionDto>();

                UserDTO theUser = null;
                bool isGroup = false;

                foreach (PermissionsModelView userToSave in inModelViewsToSave)
                {
                    ICollection<SystemPermissionDto> currentUserPermissions = null;
                    if (userToSave.UserId < 1)
                    {
                        int newUserId = -1;

                        string ntId = null;
                        string entityTrimmed = userToSave.UserNtId.Trim();

                        // check to see if this is a group
                        // only groups contain the '.' char, check to see if this one exists in our database
                        if (this.adUtils.IsGroup(entityTrimmed))
                        {
                            isGroup = true;
                        }

                        ntId = entityTrimmed;

                        IES.Common.UserData adInfoAboutUser = this.adUtils.GetUserByQualifiedAccount(ntId, isGroup);

                        theUser = this.UserMapper.GetByUserData(adInfoAboutUser);

                        if (theUser == null)
                        {
                            // user didn't exist .. save them
                            UserDTO newUser = new UserDTO
                            {
                                FirstName = adInfoAboutUser.FirstName,
                                LastName = adInfoAboutUser.LastName,
                                DisplayName = adInfoAboutUser.DisplayName,
                                EmailAddress = adInfoAboutUser.Email,
                                Ntid = adInfoAboutUser.Ntid,
                                PhoneNumber = adInfoAboutUser.Phone,
                                Id = -1,
                                UpdateDate = DateTime.Now,
                                Updateable = IES.Common.UpdateType.Upsert,
                                IsGroup = isGroup,
                                IsUsPerson = adInfoAboutUser.IsUsPerson,
                                IsSubcontractor = adInfoAboutUser.IsSubcontractor
                            };

                            newUserId = (int)this.userMediator.SaveUser(newUser);

                            theUser = this.UserMapper.GetById(newUserId);
                        }
                    }
                    else
                    {
                        theUser = this.UserMapper.GetById(userToSave.UserId);
                    }

                    // Get the current user system permissions
                    currentUserPermissions = this.permissionsMapper.GetSystemPermissions().Where(x => (x.UserId == theUser.Id)).ToList();

                    // If the role for the user already exists, leave it.  If not, create it.
                    foreach (PtmRole role in userToSave.Roles)
                    {
                        SystemPermissionDto permissionExists = null;

                        if (currentUserPermissions != null)
                        {
                            permissionExists = (from p in currentUserPermissions
                                                where p.Role == role
                                                select p).FirstOrDefault();
                        }

                        if (permissionExists != null && role == PtmRole.Viewer)
                        {
                            // delete existing viewer entry
                            SystemPermissionDto deletePermission = new SystemPermissionDto();
                            deletePermission.Id = permissionExists.Id;
                            deletePermission.Role = role;
                            deletePermission.UserId = theUser.Id;
                            deletePermission.UpdateDate = permissionExists.UpdateDate;
                            deletePermission.Updateable = IES.Common.UpdateType.Deleted;
                            allPermissionsToSave.Add(deletePermission);
                        }

                        if (permissionExists != null && role == PtmRole.ProposalSetupAdmin)
                        {
                            // delete existing Proposal Setup Admin entry
                            SystemPermissionDto deletePermission = new SystemPermissionDto();
                            deletePermission.Id = permissionExists.Id;
                            deletePermission.Role = role;
                            deletePermission.UserId = theUser.Id;
                            deletePermission.UpdateDate = permissionExists.UpdateDate;
                            deletePermission.Updateable = IES.Common.UpdateType.Deleted;
                            allPermissionsToSave.Add(deletePermission);
                        }

                        if (permissionExists == null || role == PtmRole.Viewer || role == PtmRole.ProposalSetupAdmin)
                        {
                            SystemPermissionDto newPermission = new SystemPermissionDto();

                            newPermission.Role = role;
                            newPermission.UserId = theUser.Id;
                            newPermission.Updateable = IES.Common.UpdateType.Upsert;
                            if (role == PtmRole.Viewer)
                            {
                                newPermission.LineOfBusinessIDs = userToSave.ViewerLinesOfBusiness;
                            }
                            else if (role == PtmRole.ProposalSetupAdmin)
                            {
                                newPermission.LineOfBusinessIDs = userToSave.ProposalSetupAdminLinesOfBusiness;
                            }

                            allPermissionsToSave.Add(newPermission);
                        }
                    }

                    // If there exists a permission role for the user that is not selected, delete it
                    if (currentUserPermissions != null)
                    {
                        foreach (SystemPermissionDto permission in currentUserPermissions)
                        {
                            int deletePermission = (from p in userToSave.Roles
                                                    where p == permission.Role
                                                    select p).Count();

                            if (deletePermission < 1)
                            {
                                permission.Updateable = IES.Common.UpdateType.Deleted;
                                allPermissionsToSave.Add(permission);
                            }
                        }
                    }
                }

                this.permissionsMediator.SavePermissionDtos(allPermissionsToSave);
            }

            return true;
        }

        /// <summary>
        /// Validates the current permissions attempting to be saved.
        /// </summary>
        /// <param name="permissionsToSave">Permissions to Save</param>
        /// <returns>The list of Validation Messages to throw back to the user.</returns>
        public ICollection<ValidationMessage> ValidatePermissionsToSave(Collection<PermissionsModelView> permissionsToSave)
        {
            if (permissionsToSave == null || !permissionsToSave.Any())
            {
                throw new ArgumentNullException(nameof(permissionsToSave));
            }

            // Get the existing permissions
            PermissionsGridModelView theModelView = new PermissionsGridModelView();

            this.GetManagePermissionsGridModelView(theModelView);

            // Get the before save users with system Admin. We will keep this as a running tally of who was/will be an admin
            ICollection<PermissionsModelView> allUsersWithSystemAdmin = (from x in theModelView.PermissionData
                                                                         where x.Roles.Contains(PtmRole.Admin)
                                                                         select x).ToList();

            UserDTO currentUser = this.UserMapper.GetActiveUser();

            List<ValidationMessage> validationErrors = new List<ValidationMessage>();

            // We need to track the total number before save because of the requirement to allow a system admin to delete all, but then it does not delete himself.
            int totalNumberOfUsersWithSystemAdminBeforeSave = allUsersWithSystemAdmin.Count;

            foreach (PermissionsModelView permissionsModel in permissionsToSave)
            {
                // We only pass in the user Id for deletes.
                if (permissionsModel.UserId < 0)
                {
                    if (string.IsNullOrEmpty(permissionsModel.UserNtId))
                    {
                        validationErrors.Add(new ValidationMessage(ValidationConstants.AdminValidationConstants.USER_OR_GROUP_IS_REQUIRED));
                    }
                    else
                    {
                        // validate the userName -> if it's invalid, return back an error message
                        string entityTrimmed = permissionsModel.UserNtId.Trim();
                        if (!string.IsNullOrEmpty(permissionsModel.UserNtId) && !string.IsNullOrEmpty(entityTrimmed))
                        {
                            // If the entity is not a valid group, check to see if it is a valid user.
                            if (!this.adUtils.IsGroup(entityTrimmed))
                            {
                                IES.Common.UserData user = this.adUtils.GetUserByQualifiedAccount(entityTrimmed, false);
                                if (user == null)
                                {
                                    validationErrors.Add(new ValidationMessage(string.Format(ValidationConstants.AdminValidationConstants.USER_OR_GROUP_NAME_IS_NOT_VALID, permissionsModel.UserNtId)));
                                }
                            }
                        }
                        else
                        {
                            validationErrors.Add(new ValidationMessage(ValidationConstants.AdminValidationConstants.USER_OR_GROUP_IS_REQUIRED));
                        }
                    }

                    if (permissionsModel.Roles.Contains(PtmRole.Viewer) && !permissionsModel.ViewerLinesOfBusiness.Any())
                    {
                        validationErrors.Add(new ValidationMessage(ValidationConstants.AdminValidationConstants.VIEWER_LINE_OF_BUSINESS_REQUIRED));
                    }

                    if (permissionsModel.Roles.Contains(PtmRole.ProposalSetupAdmin) && !permissionsModel.ProposalSetupAdminLinesOfBusiness.Any())
                    {
                        validationErrors.Add(new ValidationMessage(ValidationConstants.AdminValidationConstants.PROPOSAL_SETUP_ADMIN_LINES_OF_BUSINESS_REQUIRED));
                    }

                    if (!permissionsModel.Roles.Any())
                    {
                        validationErrors.Add(new ValidationMessage(ValidationConstants.AdminValidationConstants.ROLE_IS_REQUIRED));
                    }

                    if (validationErrors.Any())
                    {
                        return validationErrors;
                    }
                }
                else
                {
                    // If we are passing the user Ids we are doing deletes.
                    permissionsModel.Roles.Clear();
                }

                // We are checking to see if the user is already a system admin.  If so and keeping permissions no need to track
                PermissionsModelView userAlreadyIsSystemAdmin = (from x in allUsersWithSystemAdmin
                                                                 where x.UserNtId == permissionsModel.UserNtId
                                                                 select x).FirstOrDefault();

                if (permissionsModel.Roles.Contains(PtmRole.Admin))
                {
                    // If the user was not a system admin, but is now, add them to the total system admin group for tracking validation.
                    if (userAlreadyIsSystemAdmin == null)
                    {
                        allUsersWithSystemAdmin.Add(permissionsModel);
                    }
                }
                else
                {
                    // If they previously were a system admin remove them from the total system admin group for tracking validation
                    if (userAlreadyIsSystemAdmin != null)
                    {
                        allUsersWithSystemAdmin.Remove(userAlreadyIsSystemAdmin);
                    }
                }
            }

            // We now have the total amount of system admin users.  Check to verify at least one is left.
            // If only one, then prevent the deletion
            if (!allUsersWithSystemAdmin.Any())
            {
                if (totalNumberOfUsersWithSystemAdminBeforeSave > 1)
                {
                    PermissionsModelView yourself = permissionsToSave.FirstOrDefault(a => a.UserNtId == currentUser.Ntid);

                    if (yourself != null)
                    {
                        permissionsToSave.Remove(yourself);
                    }
                }
                else
                {
                    validationErrors.Add(new ValidationMessage("Failed to delete user because at least one Administrator is required."));
                }
            }

            return validationErrors;
        }

        /// <summary>
        /// Breaks down a group into a list of users
        /// </summary>
        /// <param name="groupId">group id</param>
        /// <returns>html list of users</returns>
        public string BreakdownGroup(int groupId)
        {
            return this.htmlHelper.BreakdownGroup(groupId);
        }

        /// <summary>
        /// Returns the active user's ID for storing in ViewBag
        /// </summary>
        /// <returns>Active User's ID</returns>
        public int GetActiveUserId()
        {
            return this.UserMapper.GetActiveUser().Id;
        }

        /// <summary>
        /// Get Proposal ID by Tracking Number
        /// </summary>
        /// <param name="inTrackingNumber">Tracking Number</param>
        /// <returns>Proposal Id</returns>
        public int GetProposalIdByTrackingNumber(string inTrackingNumber)
        {
            return this.ProposalLoader.GetIdByTrackingNumber(inTrackingNumber);
        }

        /// <summary>
        /// Get the Manage Proposal Info Details View
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>Manage Proposal Info Details View</returns>
        public ManageProposalInfoDetailsView GetManageProposalInfoDetailsView(int proposalId)
        {
            ManageProposalInfoDetailsView manageProposalInfoDetailsView = new ManageProposalInfoDetailsView();

            FullProposal fullProposal = this.GetFullProposalDto(proposalId);
            manageProposalInfoDetailsView.ProposalID = fullProposal.Id;
            manageProposalInfoDetailsView.TrackingNumber = fullProposal.IsForecastProposal ? fullProposal.ForecastedTrackingNumber : fullProposal.TrackingNumber;
            manageProposalInfoDetailsView.ProposalTitle = fullProposal.ProposalTitle;
            manageProposalInfoDetailsView.OldStatus = fullProposal.ProposalStatus;
            manageProposalInfoDetailsView.UpdateDate = fullProposal.UpdateDate;

            // populate dates and price when Completed
            if (fullProposal.ProposalStatus == ProposalStatus.Completed || fullProposal.ProposalStatus == ProposalStatus.Submitted)
            {
                manageProposalInfoDetailsView.ShowCompletedSection = true;

                ICollection<ProposalChecklistDto> checklists = fullProposal.ProposalChecklistData;

                if (checklists != null && checklists.Any())
                {
                    ProposalChecklistDto checklist = checklists.First();
                    if (checklist.ProposalSubmittalDate.HasValue)
                    {
                        manageProposalInfoDetailsView.OldProposalSubmittalDate = checklist.ProposalSubmittalDate.Value.ToString("MM/dd/yyyy");
                        manageProposalInfoDetailsView.NewProposalSubmittalDate = manageProposalInfoDetailsView.OldProposalSubmittalDate;
                    }

                    if (checklist.SubmittedValue.HasValue)
                    {
                        manageProposalInfoDetailsView.OldTotalPrice = checklist.SubmittedValue.Value.ToString();
                        manageProposalInfoDetailsView.NewTotalPrice = manageProposalInfoDetailsView.OldTotalPrice;
                    }
                }

                ICollection<ProposalChecklistSaveInfo> pricerSaveInfo = fullProposal.ProposalChecklistSaveInfo.Where(x => x.ResponseType == ChecklistResponseType.Pricer && x.SubmitDate != null).ToList();
                if (pricerSaveInfo.Any())
                {
                    DateTime pricerSubmitDate = (from x in pricerSaveInfo select x.SubmitDate).Max().Value;
                    manageProposalInfoDetailsView.OldChecklistSubmittedDatePricer = pricerSubmitDate.ToString("MM/dd/yyyy");
                    manageProposalInfoDetailsView.NewChecklistSubmittedDatePricer = manageProposalInfoDetailsView.OldChecklistSubmittedDatePricer;
                }

                ICollection<ProposalChecklistSaveInfo> peerSaveInfo = fullProposal.ProposalChecklistSaveInfo.Where(x => x.ResponseType == ChecklistResponseType.Peer && x.SubmitDate != null).ToList();
                if (peerSaveInfo.Any())
                {
                    manageProposalInfoDetailsView.ShowChecklistSubmittedDatePeer = true;
                    DateTime peerSubmitDate = (from x in peerSaveInfo select x.SubmitDate).Max().Value;
                    manageProposalInfoDetailsView.OldChecklistSubmittedDatePeer = peerSubmitDate.ToString("MM/dd/yyyy");
                    manageProposalInfoDetailsView.NewChecklistSubmittedDatePeer = manageProposalInfoDetailsView.OldChecklistSubmittedDatePeer;
                }
                else
                {
                    manageProposalInfoDetailsView.ShowChecklistSubmittedDatePeer = false;
                }
            }
            else
            {
                manageProposalInfoDetailsView.ShowCompletedSection = false;
            }

            // populate valid state transitions
            List<ProposalStatus> validStates = this.GetValidStateTransitions(fullProposal);
            validStates.ForEach(x => manageProposalInfoDetailsView.ProposalStatusList.Add(new SelectListItem()
            {
                Text = x.GetDescription(),
                Value = ((int)x).ToString()
            }));

            return manageProposalInfoDetailsView;
        }

        /// <summary>
        /// Save manage proposal
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <param name="manageProposalInfo">Manage proposal info</param>
        /// <returns>Proposal Id of saved entry</returns>
        public int? SaveManageProposalInfo(int proposalId, ManageProposalInfoDetailsView manageProposalInfo)
        {
            if (manageProposalInfo == null)
            {
                throw new ArgumentNullException(nameof(manageProposalInfo));
            }

            FullProposal fullProposal = this.GetFullProposalDto(proposalId);
            if (!this.GetValidStateTransitions(fullProposal).Contains(manageProposalInfo.NewStatus))
            {
                throw new ArgumentException(string.Format("Invalid transition from {0} to {1}. Refresh page to re-synch with database.", fullProposal.ProposalStatus, manageProposalInfo.NewStatus));
            }

            DateTime? proposalSubmittalDate = this.DetermineIfDateChanged(manageProposalInfo.OldProposalSubmittalDate, manageProposalInfo.NewProposalSubmittalDate);
            long? totalPrice = this.DetermineIfPriceChanged(manageProposalInfo.OldTotalPrice, manageProposalInfo.NewTotalPrice);
            DateTime? checklistSubmittedDatePricer = this.DetermineIfDateChanged(manageProposalInfo.OldChecklistSubmittedDatePricer, manageProposalInfo.NewChecklistSubmittedDatePricer);
            DateTime? checklistSubmittedDatePeer = this.DetermineIfDateChanged(manageProposalInfo.OldChecklistSubmittedDatePeer, manageProposalInfo.NewChecklistSubmittedDatePeer);

            int? toReturn;

            using (IES.Common.StopwatchTimer sw = new IES.Common.StopwatchTimer("AdminControllerLogic.SaveManageProposalInfoDetails", this.log))
            {
                ManageProposalInfoDto manageProposalInfoDto = new ManageProposalInfoDto()
                {
                    ProposalId = proposalId,
                    UpdateDate = manageProposalInfo.UpdateDate,
                    NewProposalStatus = manageProposalInfo.NewStatus,
                    ProposalSubmittalDate = proposalSubmittalDate,
                    TotalPrice = totalPrice,
                    ChecklistSubmittedDatePricer = checklistSubmittedDatePricer,
                    ChecklistSubmittedDatePeer = checklistSubmittedDatePeer
                };

                toReturn = this.manageProposalInfoLoader.SaveProposalInfo(manageProposalInfoDto);
            }

            if(manageProposalInfo.OldStatus == ProposalStatus.NoBid && manageProposalInfo.NewStatus == ProposalStatus.InProgress)
            {
                // reset workflow for setting to no bid
                this.approvalsControllerLogic.ResetWorkflow(proposalId);
            }

            return toReturn;
        }

        /// <summary>
        /// Returns new date if date has changed, null otherwise
        /// </summary>
        /// <param name="oldDateString">Old date string</param>
        /// <param name="newDateString">New date string</param>
        /// <returns>New date if date has changed, null otherwise</returns>
        private DateTime? DetermineIfDateChanged(string oldDateString, string newDateString)
        {
            DateTime? dateToReturn = null;
            if (!string.IsNullOrEmpty(oldDateString) && !string.IsNullOrEmpty(newDateString))
            {
                DateTime oldDate = oldDateString.ToDateTime("MM/dd/yyyy");
                DateTime newDate = newDateString.ToDateTime("MM/dd/yyyy");
                if (oldDate != newDate)
                {
                    dateToReturn = newDate;
                }
            }

            return dateToReturn;
        }

        /// <summary>
        /// Returns new price if price has changed, null otherwise
        /// </summary>
        /// <param name="oldPriceString">Old price string</param>
        /// <param name="newPriceString">New price string</param>
        /// <returns>New price if price has changed, null otherwise</returns>
        private long? DetermineIfPriceChanged(string oldPriceString, string newPriceString)
        {
            long? priceToReturn = null;
            if (!string.IsNullOrEmpty(oldPriceString) && !string.IsNullOrEmpty(newPriceString))
            {
                long oldPrice = long.Parse(oldPriceString.Replace(",", string.Empty));
                long newPrice = long.Parse(newPriceString.Replace(",", string.Empty));
                if (oldPrice != newPrice)
                {
                    priceToReturn = newPrice;
                }
            }

            return priceToReturn;
        }

        /// <summary>
        /// Validates the manage proposal info data
        /// </summary>
        /// <param name="manageProposalInfo">Manage proposal info model view</param>
        /// <param name="inValidationErrors">validation error collection</param>
        public void ValidateManageProposalInfo(ManageProposalInfoDetailsView manageProposalInfo, ICollection<ValidationMessage> inValidationErrors)
        {
            if (manageProposalInfo == null)
            {
                throw new ArgumentNullException(nameof(manageProposalInfo));
            }

            if (inValidationErrors == null)
            {
                throw new ArgumentNullException(nameof(inValidationErrors));
            }

            if (manageProposalInfo.OldStatus == ProposalStatus.Completed || manageProposalInfo.OldStatus == ProposalStatus.Submitted)
            {
                if (string.IsNullOrEmpty(manageProposalInfo.NewProposalSubmittalDate))
                {
                    inValidationErrors.Add(new ValidationMessage(ValidationConstants.ManageProposalInfoValidationConstants.SUBMITTAL_DATE_REQUIRED));
                }
                else
                {
                    try
                    {
                        manageProposalInfo.NewProposalSubmittalDate.ToDateTime("MM/dd/yyyy");
                    }
                    catch (FormatException)
                    {
                        inValidationErrors.Add(new ValidationMessage(ValidationConstants.ManageProposalInfoValidationConstants.SUBMITTAL_DATE_FORMAT));
                    }
                }

                if (string.IsNullOrEmpty(manageProposalInfo.NewTotalPrice))
                {
                    inValidationErrors.Add(new ValidationMessage(ValidationConstants.ManageProposalInfoValidationConstants.TOTAL_PRICE_REQUIRED));
                }

                if (string.IsNullOrEmpty(manageProposalInfo.NewChecklistSubmittedDatePricer))
                {
                    inValidationErrors.Add(new ValidationMessage(ValidationConstants.ManageProposalInfoValidationConstants.CHECKLIST_DATE_PRICER_REQUIRED));
                }
                else
                {
                    try
                    {
                        manageProposalInfo.NewChecklistSubmittedDatePricer.ToDateTime("MM/dd/yyyy");
                    }
                    catch (FormatException)
                    {
                        inValidationErrors.Add(new ValidationMessage(ValidationConstants.ManageProposalInfoValidationConstants.CHECKLIST_DATE_PRICER_FORMAT));
                    }
                }

                if (manageProposalInfo.ShowChecklistSubmittedDatePeer)
                {
                    if (string.IsNullOrEmpty(manageProposalInfo.NewChecklistSubmittedDatePeer))
                    {
                        inValidationErrors.Add(new ValidationMessage(ValidationConstants.ManageProposalInfoValidationConstants.CHECKLIST_DATE_PEER_REQUIRED));
                    }
                    else
                    {
                        try
                        {
                            manageProposalInfo.NewChecklistSubmittedDatePeer.ToDateTime("MM/dd/yyyy");
                        }
                        catch (FormatException)
                        {
                            inValidationErrors.Add(new ValidationMessage(ValidationConstants.ManageProposalInfoValidationConstants.CHECKLIST_DATE_PEER_FORMAT));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Validates the delete proposal action. 
        /// Ensure that there are no RDSB or GenBOE records tied to a proposal that a user / admin is trying to delete.
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <param name="inValidationErrors">validation error collection</param>
        public void ValidateDeleteProposal(int? proposalId, ICollection<ValidationMessage> inValidationErrors)
        {
            if (inValidationErrors == null)
            {
                throw new ArgumentNullException(nameof(inValidationErrors));
            }

            if (proposalId.HasValue)
            {
                FullProposal fullProposal = this.GetFullProposalDto(proposalId.Value);
                if (fullProposal != null)
                {
                    // Forecasted proposals cannot have linked RDSB docs or BOE Workspaces 
                    // Because Forcasted proposals have a null Tracking Number, checking here can cause a false positive 
                    // if there are BOEs without a proposal tracking number, preventing deletion
                    if (!fullProposal.IsForecastProposal)
                    {
                        // check for linked RDSB documents
                        if (fullProposal.DocumentId.HasValue)
                        {
                            inValidationErrors.Add(new ValidationMessage(ValidationConstants.DeleteProposalValidationConstants.HAS_LINKED_RDSB_DOCUMENT));
                        }

                        // pull information from BOE and check for linked workspaces
                        ICollection<GenBOE.Dtos.WorkspaceDTO> workspaces = this.workspaceDTODataLoader.GetAllWsNamesAndTrackingNumberInfo();
                        if (workspaces.Any(n => n.TrackingNumber == fullProposal.TrackingNumber))
                        {
                            inValidationErrors.Add(new ValidationMessage(ValidationConstants.DeleteProposalValidationConstants.HAS_LINKED_BOE_WORKSPACE));
                        }
                    }
                }
                else
                {
                    inValidationErrors.Add(new ValidationMessage(ValidationConstants.DeleteProposalValidationConstants.INVALID_PROPOSAL_ID));
                }
            }
            else
            {
                inValidationErrors.Add(new ValidationMessage(ValidationConstants.DeleteProposalValidationConstants.NULL_PROPOSAL_ID));
            }
        }

        /// <summary>
        /// Determine valid state transitions for a proposal
        /// </summary>
        /// <param name="fullProposal">Full Proposal</param>
        /// <returns>List of valid states proposal can transition to</returns>
        internal List<ProposalStatus> GetValidStateTransitions(FullProposal fullProposal)
        {
            List<ProposalStatus> validStates = new List<ProposalStatus>();

            // add current status as default
            validStates.Add(fullProposal.ProposalStatus);

            if (fullProposal.ProposalStatus == ProposalStatus.InProgress || fullProposal.ProposalStatus == ProposalStatus.Completed 
                || fullProposal.ProposalStatus == ProposalStatus.Submitted || fullProposal.ProposalStatus == ProposalStatus.NoBid)
            { 
                if(fullProposal.ProposalStatus == ProposalStatus.NoBid)
                {
                    validStates.Add(ProposalStatus.InProgress);
                }

                validStates.Add(ProposalStatus.Archived);
                validStates.Add(ProposalStatus.Deleted);
            }
            else if (fullProposal.ProposalStatus == ProposalStatus.Archived || fullProposal.ProposalStatus == ProposalStatus.Deleted)
            {
                // determine prior state of proposal
                ProposalStatus priorStatus = ProposalStatus.InProgress;

                // if data was migrated with checklist version 0, proposal can only return to completed
                if (fullProposal.ProposalChecklistPPRData.Version == 0)
                {
                    priorStatus = ProposalStatus.Completed;
                }
                else
                {
                    int question1ContentId = fullProposal.ProposalChecklistPPRData.Content.Where(x => x.TextType == ChecklistTextType.Question).ToList().OrderBy(x => x.SortOrder).Select(x => x.Id).First();

                    if (fullProposal.ProposalChecklistData != null && fullProposal.ProposalChecklistData.Any())
                    {
                        ProposalChecklistDto checklistData = fullProposal.ProposalChecklistData.First();
                        ChecklistResponseItem question1Response = checklistData.PPRResponses.FirstOrDefault(x => x.ChecklistContentId == question1ContentId);

                        if (question1Response != null)
                        {
                            if (question1Response.Response == ChecklistResponseOption.Yes)
                            {
                                // check if pricer has submitted
                                ICollection<ProposalChecklistSaveInfo> pricerSubmits =
                                    fullProposal.ProposalChecklistSaveInfo.Where(x => x.ResponseType == ChecklistResponseType.Pricer && x.SubmitDate != null).ToList();

                                if (pricerSubmits.Any())
                                {
                                    priorStatus = ProposalStatus.Completed;
                                }
                            }
                            else if (question1Response.Response == ChecklistResponseOption.No)
                            {
                                // check if pricer and peer have submitted
                                ICollection<ProposalChecklistSaveInfo> pricerSubmits =
                                    fullProposal.ProposalChecklistSaveInfo.Where(x => x.ResponseType == ChecklistResponseType.Pricer && x.SubmitDate != null).ToList();
                                ICollection<ProposalChecklistSaveInfo> peerSubmits =
                                    fullProposal.ProposalChecklistSaveInfo.Where(x => x.ResponseType == ChecklistResponseType.Peer && x.SubmitDate != null).ToList();

                                if (pricerSubmits.Any() && peerSubmits.Any())
                                {
                                    priorStatus = ProposalStatus.Completed;
                                }
                            }
                        }
                    }
                }

                validStates.Add(priorStatus);

                if (fullProposal.ProposalStatus == ProposalStatus.Archived)
                {
                    validStates.Add(ProposalStatus.Deleted);
                }
                else if (fullProposal.ProposalStatus == ProposalStatus.Deleted)
                {
                    validStates.Add(ProposalStatus.Archived);
                }
            }

            return validStates;
        }

        /// <summary>
        /// Get the full proposal DTO
        /// </summary>
        /// <param name="proposalId">Proposal Id</param>
        /// <returns>full Proposal DTO</returns>
        private FullProposal GetFullProposalDto(int proposalId)
        {
            ProposalDto proposal = this.ProposalLoader.GetById(proposalId);
            FullProposal fullProposal = this.ObjectFactory.CreateFullProposal(proposal);

            return fullProposal;
        }

        /// <summary>
        /// Return available ProgramAreas for selected line of business
        /// </summary>
        /// <param name="lineOfBusiness">Line of Business to filter</param>
        /// <returns>Program Area or "All" designating all of them</returns>
        public string GetProgramAreasForLineOfBusinessForBulkArchive(string lineOfBusiness)
        {
            StringBuilder selectList = new StringBuilder();
            
            if (string.IsNullOrEmpty(lineOfBusiness))
            {
                return selectList.ToString();
            }
            else if (lineOfBusiness == "All")
            {
                selectList.Append(string.Format("<option value=\"{0}\">{1}</option>", 0, "All"));
                return selectList.ToString();
            }
            else
            {
                int lineOfBusinessId = int.Parse(lineOfBusiness);
                ICollection<PickListDto> programAreas = this.orgStructureDataMapper.GetAllProgramAreas().Where(x => x.ParentIds.Contains(lineOfBusinessId)).ToList();

                selectList.Append(string.Format("<option value=\"{0}\">{1}</option>", 0, "All"));

                foreach (PickListDto programArea in programAreas)
                {
                    selectList.Append(string.Format("<option value=\"{0}\">{1}</option>",
                        programArea.Id, programArea.Text));
                }

                return selectList.ToString();
            }
        }

        /// <summary>
        /// Given a set of criteria, find out the number of affected proposals.
        /// </summary>
        /// <param name="model">Model View</param>
        /// <returns>a string</returns>
        public int SearchBulkArchive(BulkArchiveModelView model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            
            DateTime? startDate = null;

            if (!string.IsNullOrEmpty(model.StartDate))
            {
                startDate = model.StartDate.ToDateTime("MM/dd/yyyy");
            }

            DateTime? endDate = null;

            if (!string.IsNullOrEmpty(model.EndDate))
            {
                endDate = model.EndDate.ToDateTime("MM/dd/yyyy").AddDays(1);
            }
            
            BulkArchiveDto bulkArchiveDto = new BulkArchiveDto()
            {
                StartDate = startDate,
                EndDate = endDate,
                LineOfBusiness = model.LineOfBusiness,
                ProgramArea = model.ProgramArea
            };

            int? results = this.bulkArchiveLoader.SearchBulkArchive(bulkArchiveDto);

            return results.HasValue ? results.Value : 0;
        }

        /// <summary>
        /// Given a set of criteria, bulk archive those affected proposals.
        /// </summary>
        /// <param name="model">Model View</param>
        /// <returns>Number of proposals archived</returns>
        public int ApplyBulkArchive(BulkArchiveModelView model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            
            DateTime? startDate = null;

            if (!string.IsNullOrEmpty(model.StartDate))
            {
                startDate = model.StartDate.ToDateTime("MM/dd/yyyy");
            }

            DateTime? endDate = null;

            if (!string.IsNullOrEmpty(model.EndDate))
            {
                endDate = model.EndDate.ToDateTime("MM/dd/yyyy").AddDays(1);
            }

            BulkArchiveDto bulkArchiveDto = new BulkArchiveDto()
            {
                StartDate = startDate,
                EndDate = endDate,
                LineOfBusiness = model.LineOfBusiness,
                ProgramArea = model.ProgramArea
            };

            return this.bulkArchiveLoader.ApplyBulkArchive(bulkArchiveDto);
        }

        /// <summary>
        /// Validate the Bulk Archive Model View values.
        /// </summary>
        /// <param name="model">Bulk Archive Model View</param>
        /// <param name="inValidationErrors">Validation Errors</param>
        public void ValidateBulkArchiveRequest(BulkArchiveModelView model, ICollection<ValidationMessage> inValidationErrors)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (inValidationErrors == null)
            {
                throw new ArgumentNullException(nameof(inValidationErrors));
            }

            DateTime? startDate = null;

            if (!string.IsNullOrEmpty(model.StartDate))
            {
                try
                {
                    startDate = model.StartDate.ToDateTime("MM/dd/yyyy");
                }
                catch (FormatException)
                {
                    inValidationErrors.Add(new ValidationMessage(ValidationConstants.AdminValidationConstants.CREATED_DATE_FORMAT));
                    // If the Start Date is invalid, then simply return since no other validation message apply.
                    return;
                }
            }

            DateTime? endDate = null;

            if (!string.IsNullOrEmpty(model.EndDate))
            {
                try
                {
                    endDate = model.EndDate.ToDateTime("MM/dd/yyyy");
                }
                catch (FormatException)
                {
                    inValidationErrors.Add(new ValidationMessage(ValidationConstants.AdminValidationConstants.CREATED_DATE_FORMAT));
                    // If the End Date is invalid, then simply return since the last validation message does not apply
                    return;
                }
            }

            if(startDate != null && endDate != null)
            {
                if(startDate > endDate) 
                {
                    inValidationErrors.Add(new ValidationMessage(ValidationConstants.AdminValidationConstants.START_DATE_BEFORE_END_DATE));
                }
            }
        }

        #endregion
    }
}
// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.Common.Security
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GenTRAC.DataBridge.DTO;
    using IES.Common;

    /// <summary>
    /// Class the requests CRUD Authorizations to a given page for given roles for the currently logged in user
    /// </summary>
    public class SecurityAccess : GenTRAC.DataBridge.Common.Security.ISecurityAccess
    {
        /// <summary>
        /// The logger
        /// </summary>
        private IES.Common.Logger log = new IES.Common.Logger(typeof(SecurityAccess));

        /// <summary>
        /// Used to load in all security accesses for user in the system
        /// </summary>
        private ISecurityMapper securityMapper = null;

        /// <summary>
        /// Proposal Loader
        /// </summary>
        private IProposalLoader proposalLoader = null;

        /// <summary>
        /// The security matrix, first index for pages, 2nd index for roles
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Member")]
        private int[,] securityMatrix; // not possible to replace with jagged array per CA1814

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="inSecurityMapper">The security mapper</param>
        /// <param name="inProposalLoader">Proposal Loader</param>
        public SecurityAccess(ISecurityMapper inSecurityMapper, IProposalLoader inProposalLoader)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            this.log.Info("Starting to initialize security matrix...");

            this.securityMapper = inSecurityMapper;
            this.proposalLoader = inProposalLoader;

            // get the indexes size, go to the database vs. looking at the enum since the 2 should be in sync
            int numOfPages = Convert.ToInt32(Enum.GetValues(typeof(PtmSecurityPage)).Cast<PtmSecurityPage>().Max());

            int numOfRoles = Convert.ToInt32(Enum.GetValues(typeof(PtmRole)).Cast<PtmRole>().Max());

            // add 1 to each node to allocate the correct number of nodes in the array
            // since it's a 0 index
            this.securityMatrix = new int[numOfPages + 1, numOfRoles + 1];

            this.InitializeMatrix();

            sw.Stop();
            this.log.Info("Finished initializing security matrix.  Took " + sw.ElapsedMilliseconds + " milliseconds.");
        }

        /// <summary>
        /// Initializes all elements in the matrix to none, then adds each page
        /// </summary>
        private void InitializeMatrix()
        {
            // initial the matrix with no access for all dictionary combinations
            foreach (PtmSecurityPage page in Enum.GetValues(typeof(PtmSecurityPage)))
            {
                foreach (PtmRole role in Enum.GetValues(typeof(PtmRole)))
                {
                    this.InitializeMatrix(new PtmSecurityPage[] { page }, new PtmRole[] { role }, SecurityAuthorization.None);
                }
            }

            // Home
            this.InitializeMatrix(new PtmSecurityPage[] { PtmSecurityPage.Home },
                     new PtmRole[] { PtmRole.NotSet },
                     SecurityAuthorization.CreateReadUpdateDelete);

            // Admin
            this.InitializeMatrix(new PtmSecurityPage[] { PtmSecurityPage.Admin },
                new PtmRole[] { PtmRole.Admin },
                SecurityAuthorization.CreateReadUpdateDelete);

            // Reports
            this.InitializeMatrix(new PtmSecurityPage[] { PtmSecurityPage.Reports },
                new PtmRole[] { PtmRole.Admin, PtmRole.Viewer, PtmRole.SystemPricer },
                SecurityAuthorization.Read);

            // Proposals
            this.InitializeMatrix(new PtmSecurityPage[] { PtmSecurityPage.Proposal },
                new PtmRole[] { PtmRole.Admin, PtmRole.SystemPricer, PtmRole.Pricer }, SecurityAuthorization.CreateReadUpdateDelete);

            this.InitializeMatrix(new PtmSecurityPage[] { PtmSecurityPage.Proposal },
                new PtmRole[] { PtmRole.LOBEstLead }, SecurityAuthorization.CreateReadUpdateDeleteForecast);

            this.InitializeMatrix(new PtmSecurityPage[] { PtmSecurityPage.Proposal },
                new PtmRole[] { PtmRole.CostVolumeLead, PtmRole.AdditionalPricingResource1, PtmRole.AdditionalPricingResource2, PtmRole.BackupPricer, PtmRole.ProposalSetupAdmin }, SecurityAuthorization.ReadUpdate);

            this.InitializeMatrix(new PtmSecurityPage[] { PtmSecurityPage.Proposal },
                new PtmRole[] { PtmRole.CaptureManager, PtmRole.AdditionalUser, PtmRole.PeerReviewer, PtmRole.ContractsPOC, PtmRole.SupplyChainPOCMatl, PtmRole.SupplyChainPOCSubs, PtmRole.Viewer, PtmRole.CoverSheetApprover, PtmRole.PricingVerification },
                SecurityAuthorization.Read);

            // Checklist
            this.InitializeMatrix(new PtmSecurityPage[] { PtmSecurityPage.Checklist },
                new PtmRole[] { PtmRole.Pricer }, SecurityAuthorization.CreateReadUpdateDelete);

            this.InitializeMatrix(new PtmSecurityPage[] { PtmSecurityPage.Checklist },
                new PtmRole[] { PtmRole.Admin, PtmRole.BackupPricer }, SecurityAuthorization.ReadUpdate);

            this.InitializeMatrix(new PtmSecurityPage[] { PtmSecurityPage.Checklist },
                new PtmRole[] { PtmRole.PeerReviewer, PtmRole.AdditionalPricingResource1, PtmRole.AdditionalPricingResource2, PtmRole.AdditionalUser, PtmRole.CaptureManager, PtmRole.ContractsPOC, PtmRole.CostVolumeLead, PtmRole.SupplyChainPOCMatl, PtmRole.SupplyChainPOCSubs, PtmRole.SystemPricer, PtmRole.Viewer, PtmRole.CoverSheetApprover, PtmRole.PricingVerification, PtmRole.LOBEstLead },
                SecurityAuthorization.Read);

            this.InitializeMatrix(new PtmSecurityPage[] { PtmSecurityPage.ChecklistReport }, new PtmRole[] { PtmRole.Pricer, PtmRole.BackupPricer, PtmRole.Admin }, SecurityAuthorization.Read);

            // Approvals
            this.InitializeMatrix(new PtmSecurityPage[] { PtmSecurityPage.Approvals },
                new PtmRole[] { PtmRole.Pricer, PtmRole.CoverSheetApprover, PtmRole.PricingVerification, PtmRole.PeerReviewer, PtmRole.LOBEstLead }, SecurityAuthorization.ReadUpdate);

            this.InitializeMatrix(new PtmSecurityPage[] { PtmSecurityPage.Approvals }, new PtmRole[] { PtmRole.Admin, PtmRole.Viewer }, SecurityAuthorization.Read);

            // Post Submittal Attachments
            this.InitializeMatrix(new PtmSecurityPage[] { PtmSecurityPage.PostSubmittalAttachments },
                new[] { PtmRole.Admin, PtmRole.Pricer, PtmRole.BackupPricer, PtmRole.CostVolumeLead },
                SecurityAuthorization.CreateReadUpdateDelete);

            this.InitializeMatrix(new[] { PtmSecurityPage.PostSubmittalAttachments },
                new[] { PtmRole.CoverSheetApprover, PtmRole.PricingVerification, PtmRole.PeerReviewer, PtmRole.LOBEstLead, PtmRole.Viewer },
                SecurityAuthorization.Read);

            // Certificate Timeline
            this.InitializeMatrix(new PtmSecurityPage[] { PtmSecurityPage.CertificationTimeline },
                new[] { PtmRole.Admin, PtmRole.Pricer, PtmRole.BackupPricer, PtmRole.CostVolumeLead, PtmRole.LOBEstLead },
                SecurityAuthorization.CreateReadUpdateDelete);

            this.InitializeMatrix(new[] { PtmSecurityPage.CertificationTimeline },
                new[] { PtmRole.CoverSheetApprover, PtmRole.PricingVerification, PtmRole.PeerReviewer, PtmRole.Viewer },
                SecurityAuthorization.Read);
        }

        /// <summary>
        /// Initialize elements in the matrix
        /// This is a convenience method to quickly and easily populate nodes in the matrix.
        /// </summary>
        /// <param name="inPages">Pages to seed in the matrix</param>
        /// <param name="inRoles">Roles to seed in the matrix from enums SecurityAdminRole, Role, SecurityWorkspaceRole</param>
        /// <param name="inAuthorization">Authorizations to seed in the matrix</param>
        private void InitializeMatrix(PtmSecurityPage[] inPages, PtmRole[] inRoles, SecurityAuthorization inAuthorization)
        {
            foreach (PtmRole role in inRoles)
            {
                foreach (PtmSecurityPage page in inPages)
                {
                    this.securityMatrix[(int)page, (int)role] = (int)inAuthorization;
                }
            }
        }

        /// <summary>
        /// Return true/false depending on users authorizations for the roles requested
        /// </summary>
        /// <param name="inPermissions">The roles to determine and see if user has any of these roles in the system</param>
        /// <param name="highestRole">Highest role corresponding to the highest authorization level</param>
        /// <returns>Return true if the user has any of the roles passed in, false otherwise</returns>
        public SecurityAuthorization IsAuthorized(SecurityPermissionsRequested inPermissions, out PtmRole highestRole)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            if (inPermissions == null)
            {
                throw new ArgumentNullException(nameof(inPermissions));
            }

            IReadOnlyCollection<SecurityPermissionsResponse> rolesForUser = this.securityMapper.GetRolesForLoggedInUser();

            // for each user we need to see if they are in the admin groups
            SecurityAuthorization authorization = this._GetAuthorizationsRoles(inPermissions, rolesForUser, out highestRole);

            sw.Stop();
            this.log.Debug(
                string.Format("isAuthorized(SecurityPermissions) took [{0}] milliseconds for Request {1}",
                                    sw.ElapsedMilliseconds,
                                    inPermissions.ToString()));

            return authorization;
        }

        /// <summary>
        /// Test whether the current user has the designated role
        /// </summary>
        /// <param name="role">The role to test</param>
        /// <param name="proposalId">Optional proposal id, if applicable for authorization</param>
        /// <returns>Yes or no</returns>
        public bool CurrentUserHasRole(PtmRole role, int? proposalId)
        {
            IReadOnlyCollection<SecurityPermissionsResponse> rolesForUser = this.securityMapper.GetRolesForLoggedInUser();
            return proposalId.HasValue ? rolesForUser.Any(r => r.ProposalID == proposalId.Value && r.AuthorizedRole == role) : rolesForUser.Any(r => r.AuthorizedRole == role);
        }

        /// <summary>
        /// Get the highest level of authorizations for the roles 
        /// </summary>
        /// <param name="inPermission">The requested permissions</param>
        /// <param name="inPermissionResponses">The permission responses</param>
        /// <param name="highestRole">Highest role corresponding to the highest authorization level</param>
        /// <returns>The highest security permission present between for the roles the user has</returns>
        private SecurityAuthorization _GetAuthorizationsRoles(SecurityPermissionsRequested inPermission, IReadOnlyCollection<SecurityPermissionsResponse> inPermissionResponses, out PtmRole highestRole)
        {
            bool proposalRequired = false;

            switch (inPermission.PageToCheck)
            {
                case PtmSecurityPage.Admin:
                case PtmSecurityPage.Home:
                case PtmSecurityPage.Reports:
                case PtmSecurityPage.None:
                case PtmSecurityPage.ChecklistReport:
                    proposalRequired = false;
                    break;
                case PtmSecurityPage.Proposal:
                    if (inPermission.ProposalId.HasValue && inPermission.ProposalId != -1)
                    {
                        proposalRequired = true;
                    }
                    else
                    {
                        proposalRequired = false;
                    }

                    break;
                case PtmSecurityPage.Checklist:
                case PtmSecurityPage.Approvals:
                case PtmSecurityPage.PostSubmittalAttachments:
                case PtmSecurityPage.CertificationTimeline:
                    proposalRequired = true;
                    break;
                default:
                    throw new ArgumentException("Page " + inPermission.PageToCheck + " not found.");
            }

            if (proposalRequired && inPermission.ProposalId == null)
            {
                throw new ArgumentException("The proposal ID is required for security checks and was not supplied for page type " + inPermission.PageToCheck);
            }

            SecurityAuthorization highestAuthorizationLevel = SecurityAuthorization.None;
            highestRole = PtmRole.NotSet;
            ProposalStatus? proposalStatus = null;
            bool isForecastProposal = false;
            if (proposalRequired)
            {
                ProposalDto proposal = this.proposalLoader.GetById(inPermission.ProposalId.Value);
                proposalStatus = proposal.ProposalStatus;
                isForecastProposal = proposal.IsForecastProposal;
            }

            foreach (SecurityPermissionsResponse permissionResponse in inPermissionResponses)
            {
                switch (permissionResponse.AuthorizedRole)
                {
                    // the Role.None case needs to remain in the switch since it gets assigned permissions in the security matrix.
                    case PtmRole.NotSet:
                    case PtmRole.Admin:
                        break;
                    case PtmRole.AdditionalUser:
                    case PtmRole.AdditionalPricingResource1:
                    case PtmRole.AdditionalPricingResource2:
                    case PtmRole.CaptureManager:
                    case PtmRole.CostVolumeLead:
                    case PtmRole.ContractsPOC:
                    case PtmRole.PeerReviewer:
                    case PtmRole.Pricer:
                    case PtmRole.SupplyChainPOCMatl:
                    case PtmRole.SupplyChainPOCSubs:
                    case PtmRole.BackupPricer:
                    case PtmRole.SystemPricer:
                    case PtmRole.Viewer:
                    case PtmRole.ProposalSetupAdmin:
                    case PtmRole.CoverSheetApprover:
                    case PtmRole.LOBEstLead:
                    case PtmRole.PricingVerification:
                    case PtmRole.ProposalMgr:
                    case PtmRole.GenBoeWorkspaceCreator:
                        if (proposalRequired && permissionResponse.ProposalID != inPermission.ProposalId)
                        {
                            // continue to the next iteration foreach
                            continue;
                        }

                        // don't allow non-admin or system pricer roles to get to New Proposal page with no proposal ID
                        if (permissionResponse.AuthorizedRole != PtmRole.SystemPricer && inPermission.PageToCheck == PtmSecurityPage.Proposal && !proposalRequired)
                        {
                            continue;
                        }

                        // only Admins can see Deleted proposals
                        if (proposalRequired && proposalStatus.HasValue)
                        {
                            if (proposalStatus.Value == ProposalStatus.Deleted)
                            {
                                continue;
                            }
                        }

                        break;
                    default:
                        this.log.Error("Unknown role returned from database " + permissionResponse.AuthorizedRole);
                        break;
                }

                // Check the matrix ...
                // check the authorization and get the max from our running list
                SecurityAuthorization currentAuthorizationLevel = (SecurityAuthorization)this.securityMatrix[(int)inPermission.PageToCheck, (int)permissionResponse.AuthorizedRole];
                if (currentAuthorizationLevel > highestAuthorizationLevel)
                {
                    if (currentAuthorizationLevel == SecurityAuthorization.CreateReadUpdateDeleteForecast)
                    {
                        if (highestAuthorizationLevel == SecurityAuthorization.CreateReadUpdateDelete)
                        {
                            // just break out early
                            continue;
                        }

                        if (isForecastProposal)
                        {
                            // User has CreateReadUpdateDelete rights for this Forecast Proposal
                            currentAuthorizationLevel = SecurityAuthorization.CreateReadUpdateDelete;
                        }
                        else
                        {
                            // Roll back to Read level rights for non-Forecast Proposal
                            currentAuthorizationLevel = SecurityAuthorization.Read;
                        }
                    }

                    // Extra level check in case the rights needed rolled back to Read for Non-Forecast
                    if (currentAuthorizationLevel > highestAuthorizationLevel)
                    {
                        highestAuthorizationLevel = currentAuthorizationLevel;

                        if (IES.Common.EnumUtilities.Compare<PtmRole>(permissionResponse.AuthorizedRole, highestRole) == 1)
                        {
                            highestRole = permissionResponse.AuthorizedRole;
                        }
                    }
                }
            }

            return highestAuthorizationLevel;
        }
    }
}

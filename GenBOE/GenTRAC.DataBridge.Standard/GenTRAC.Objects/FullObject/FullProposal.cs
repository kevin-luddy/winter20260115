// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.Objects.FullObject
{
    using System.Collections.Generic;
    using System.Reflection;
    using GenTRAC.DataBridge.DTO;
    using Microsoft.Practices.Unity;

    /// <summary>
    /// Full Proposal
    /// </summary>
    public class FullProposal : ProposalDto
    {
        /// <summary>
        /// Retriever
        /// </summary>
        private IRetriever retriever;

        /// <summary>
        /// Current User
        /// </summary>
        private UserDTO currentUser = null;

        /// <summary>
        /// Proposal Permissions
        /// </summary>
        private ICollection<ProposalPermissionDto> permissions = null;

        /// <summary>
        /// Proposal checklist
        /// </summary>
        private ICollection<ProposalChecklistDto> checklists = null;

        /// <summary>
        /// PPR checklist content data
        /// </summary>
        private ChecklistContentDto checklistPPRContent = null;

        /// <summary>
        /// PAR checklist content data
        /// </summary>
        private ChecklistContentDto checklistPARContent = null;

        /// <summary>
        /// Proposal checklist save info
        /// </summary>
        private ICollection<ProposalChecklistSaveInfo> checklistSaveInfo = null;

        /// <summary>
        /// Constructor
        /// </summary>
        private FullProposal()
            : base()
        {
            this.retriever = IES.Standard.classes.GenBOEUnityContainer.Container.Resolve(typeof(IRetriever)) as IRetriever;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="proposal">Capture Dto</param>
        internal FullProposal(ProposalDto proposal)
            : this()
        {
            if (proposal != null)
            {
                foreach (PropertyInfo prop in proposal.GetType().GetProperties())
                {
                    if (prop.CanWrite)
                    {
                        this.GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(proposal, null), null);
                    }
                }
            }
        }

        /// <summary>
        /// Current User
        /// </summary>
        public UserDTO CurrentUser
        {
            get
            {
                if (this.currentUser == null)
                {
                    this.currentUser = this.retriever.GetCurrentUser();
                }

                return this.currentUser;
            }
        }

        /// <summary>
        /// Proposal Permissions
        /// </summary>
        public ICollection<ProposalPermissionDto> Permissions
        {
            get
            {
                if (this.permissions == null)
                {
                    this.permissions = this.retriever.GetProposalPermissions(this.Id);
                }

                return this.permissions;
            }
        }

        /// <summary>
        /// proposal checklists
        /// </summary>
        public ICollection<ProposalChecklistDto> ProposalChecklistData
        {
            get
            {
                if (this.checklists == null)
                {
                    this.checklists = this.retriever.GetProposalChecklists(this.Id);
                }

                return this.checklists;
            }
        }

        /// <summary>
        /// proposal checklist PPR data
        /// </summary>
        public ChecklistContentDto ProposalChecklistPPRData
        {
            get
            {
                if (this.checklistPPRContent == null)
                {
                    this.checklistPPRContent = this.retriever.GetPPRChecklistContent(this.Id);
                }

                return this.checklistPPRContent;
            }
        }

        /// <summary>
        /// proposal checklist PAR data
        /// </summary>
        public ChecklistContentDto ProposalChecklistPARData
        {
            get
            {
                if (this.checklistPARContent == null)
                {
                    this.checklistPARContent = this.retriever.GetPARChecklistContent(this.Id);
                }

                return this.checklistPARContent;
            }
        }

        /// <summary>
        /// proposal checklist save info
        /// </summary>
        public ICollection<ProposalChecklistSaveInfo> ProposalChecklistSaveInfo
        {
            get
            {
                if (this.checklistSaveInfo == null)
                {
                    this.checklistSaveInfo = this.retriever.GetAllChecklistSaveInfo(this.Id);
                }

                return this.checklistSaveInfo;
            }
        }
    }
}

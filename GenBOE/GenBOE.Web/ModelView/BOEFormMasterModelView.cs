// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
    using System.Collections.Generic;
    using ActionLogic.ModelView.BOE;
    using GenBOE.Dtos;
    using IES.Common;
    using GenBOE.Objects;

    /// <summary>
    /// Master Model View for BOE Forms Creation and Update.
    /// </summary>
    /// <seealso cref="GenBOE.Web.ModelView.GenBOEMasterModelView" />
    public class BOEFormMasterModelView : GenBOEMasterModelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BOEFormMasterModelView"/> class.
        /// </summary>
        public BOEFormMasterModelView() : base()
        {
            ProposalTitleAndRfpNumber = string.Empty;
        }

        /// <summary>
        /// The IWTA Resources to show.
        /// </summary>
        public ICollection<ResourceDTO> IWTAResources { get; set; }

        /// <summary>
        /// In-use IWTA Resource Ids.
        /// </summary>
        public ICollection<int> IWTAInUseResourceIds { get; set; }

        /// <summary>
        /// In-use Sub Resource Ids.
        /// </summary>
        public ICollection<int> SubInUseResourceIds { get; set; }

        /// <summary>
        /// The Sub Resources to show.
        /// </summary>
        public ICollection<ResourceDTO> SubResources { get; set; }

        /// <summary>
        /// The BOE Form Type
        /// </summary>
        public BOEFormType BOEFormType
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the pboe model.
        /// </summary>
        public BOEFormPBOEModelView PBOEModel { get; set; }

        /// <summary>
        /// Gets or sets the iboe model.
        /// </summary>
        public BOEFormIBOEModelView IBOEModel { get; set; }

        /// <summary>
        /// Returns the version of the current BOE Form type
        /// </summary>
        public int Version
        {
            get
            {
                if (BOEFormType == BOEFormType.IBOE)
                {
                    return IBOEModel.Version;
                }
                else
                {
                    return PBOEModel.Version;
                }
            }
        }

        /// <summary>
        /// Gets or sets a read-only string containing the "Proposal Title / RFP Number" from the workspace.
        /// This value is NOT persisted in the GenBOE DB.  It is displayed in read-only format for the user.
        ///
        /// Usage Notes:
        /// - if the Workspace.ProposalTitle field in the DB is empty & valid Workspace.TrackingNumber was provided:
        /// -- Show/Export -> { Proposal Number } / { RFP } (as read-only values from PTM DB).
        /// --- Please note the above displays horribly in "non-edit" mode, it should be a single line.
        /// 
        /// - else if (Workspace.ProposalTitle field is in the DB, or Workspace.TrackingNumber is invalid or empty)
        /// -- Show the textbox & allow edit
        /// -- Export -> the Workspace.ProposalTitle field
        /// </summary>
        public string ProposalTitleAndRfpNumber { get; set; }

        /// <summary>
        /// Gets or sets the initial workspace title.
        /// </summary>
        public string InitialWorkspaceTitle { get; set; }

        /// <summary>
        /// Gets or sets the clins.
        /// </summary>
        public ICollection<FullClin> Clins { get; set; }

        /// <summary>
        /// Gets or sets the type of the contract.
        /// </summary>
        public int ContractType { get; set; }
    }
}

// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.ObjectModel;

namespace GenBOE.ActionLogic.ModelView
{
	/// <summary>
	/// Model view for the initial results when importing BOEs
	/// </summary>
    public class ImportBoeResultsModelView
    {
		/// <summary>
		/// ctor
		/// </summary>
        public ImportBoeResultsModelView()
        {
            this.AuthorIDs = new Collection<int>();
            this.SubcontractorAuthorIDs = new Collection<int>();
            this.ApproverIDs = new Collection<int>();
        }

		/// <summary>
		/// The BOE ID
		/// </summary>
        public int BoeID { get; set; }

		/// <summary>
		/// The BOE Title
		/// </summary>
        public string BOETitle { get; set; }

		/// <summary>
		/// The WBS as a string
		/// </summary>
        public string WbsString { get; set; }

		/// <summary>
		/// The WBS ID
		/// </summary>
        public int? WbsID { get; set; }

		/// <summary>
		/// The CLIN as a string
		/// </summary>
        public string ClinString { get; set; }

		/// <summary>
		/// The CLIN ID
		/// </summary>
        public int? ClinID { get; set; }

		/// <summary>
		/// The Author IDs
		/// </summary>
        public Collection<int> AuthorIDs { get; set; }

		/// <summary>
		/// The Subcontractor Author IDs
		/// </summary>
        public Collection<int> SubcontractorAuthorIDs { get; set; }

		/// <summary>
		/// The Approver IDs
		/// </summary>
        public Collection<int> ApproverIDs { get; set; }

		/// <summary>
		/// The Status of the BOE
		/// </summary>
        public int Status { get; set; }

		/// <summary>
		/// The BOE XREF ID
		/// </summary>
        public int? BoeXrefID { get; set; }

		/// <summary>
		/// Is the BOE Material?
		/// </summary>
        public bool isMaterial { get; set; }

		/// <summary>
		/// Is the BOE a multi CLIN/WBS?
		/// </summary>
        public bool IsMultiClinWbs { get; set; }
                
		/// <summary>
		/// The type of import being performed on this BOE row
		/// </summary>
        // This needs to stay an "int" and not a "ImportResult" in order to translate correctly during model binding.
        public int ImportType { get; set; }

		/// <summary>
		/// The Workspace shortname
		/// </summary>
		public string WorkspaceShortName { get; set; }

		/// <summary>
		/// The WCB ID (WBS/CLIN/BOE association ID)
		/// </summary>
		public int? WCBID { get; set; }
    }
}

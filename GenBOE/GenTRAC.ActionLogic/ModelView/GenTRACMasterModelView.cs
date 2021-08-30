// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView
{
    /// <summary>
    /// Model View for the GenTRAC master page
    /// </summary>
    public class GenTRACMasterModelView
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public GenTRACMasterModelView()
        {
            this.ProposalName = "Unable to get Proposal name";
            this.HeaderFooter = "Lockheed Martin Proprietary Information";
        }

        /// <summary>
        /// The Proposal Name
        /// </summary>
        public string ProposalName { get; set; }

        /// <summary>
        /// The header/footer text
        /// </summary>
        public string HeaderFooter { get; set; }
    }
}

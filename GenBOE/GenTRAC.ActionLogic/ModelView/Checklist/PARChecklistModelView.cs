// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.ActionLogic.ModelView.Checklist
{
    /// <summary>
    /// Export PAR Checklist Report Model View
    /// </summary>
    public class PARChecklistModelView
    {
        /// <summary>
        /// the proposal id
        /// </summary>
        public int ProposalID { get; set; }

        /// <summary>
        /// the proposal checklist version
        /// </summary>
        public int? ChecklistVersion { get; set; }

        /// <summary>
        /// default constuctor
        /// </summary>
        public PARChecklistModelView()
        {
        }
    }
}

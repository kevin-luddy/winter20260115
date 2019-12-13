// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    /// <summary>
    /// Travel NonZone Fees and Costs DTO at the Workspace Level.
    /// </summary>
    public class WorkspaceRMSTravelNonzoneFeesAndCostsDTO : MSTTravelNonzoneFeesAndCostsDTO
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public WorkspaceRMSTravelNonzoneFeesAndCostsDTO()
            : base()
        {
        }

        /// <summary>
        /// ID of the Workspace
        /// </summary>
        public int WorkspaceId { get; set; }
    }
}
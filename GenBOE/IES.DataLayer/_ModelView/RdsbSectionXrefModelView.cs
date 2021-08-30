// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.ModelViews
{
    /// <summary>
    /// ModelView for RDSB Section Xref
    /// </summary>
    public class RdsbSectionXrefModelView : IESUpdateableModelView
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public RdsbSectionXrefModelView()
        {
            this.Id = -1;
            this.SectionId = -1;
            this.RdsbDocumentInformationId = -1;
        }

        /// <summary>
        /// Gets/Sets Rate Code ID
        /// </summary>
        public int SectionId { get; set; }

        /// <summary>
        /// Gets/Sets RDSB Document Information ID
        /// </summary>
        public int RdsbDocumentInformationId { get; set; }

        /// <summary>
        /// Propagates the new 'parent' DTO Id to all first level 'child' DTOs in collections.
        /// </summary>
        /// <param name="newParentId">new id of the parent DTO</param>
        protected override void PropagateNewParentIdToChildDTOs(int newParentId)
        {
            //// There are no child collections in this dto so there is no work to do.
        }
    }
}

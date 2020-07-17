// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using IES.Common;

    /// <summary>
    /// This is the labor spread data associated with the BOE DTO
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class ResourceSpreadDto : UpdateableDTO, IBOEMembership
    {
        public ResourceSpreadDto()
        {
            Id = -1;
            LaborSpreadDate = DateTime.MinValue;
            LaborSpreadValue = 0;
        }

        // the month associated with the labor spread
        public DateTime LaborSpreadDate { get; set; }

        // the labor spread value
        public decimal LaborSpreadValue { get; set; }

        public int BoeID { get; set; }

        public int LaborTypeId { get; set; }

        /// <summary>
        /// Propagates the new 'parent' DTO Id to all first level 'child' DTOs in collections.
        /// </summary>
        /// <param name="newParentId">new id of the parent DTO</param>
        override protected void PropagateNewParentIdToChildDTOs(int newParentId)
        {
          //// There are no child collections in this dto so there is no work to do.
        }
    }
}

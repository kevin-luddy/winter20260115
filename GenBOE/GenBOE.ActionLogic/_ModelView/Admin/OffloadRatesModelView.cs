// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Admin
{
    using System.Collections.Generic;
    using Dtos;

    /// <summary>
    /// Model view for Offloading Rates.
    /// </summary>
    public class OffloadRatesModelView
    {
        public ICollection<ResourceDTO> Resources { get; set; }

        public ICollection<PerformingOrgDTO> PerformingOrgs { get; set; }

        public ICollection<ResourceDTO> SubcontractorResources { get; set; }

    }
}
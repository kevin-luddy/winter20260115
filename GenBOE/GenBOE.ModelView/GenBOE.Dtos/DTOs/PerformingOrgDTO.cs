// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using IES.Common;

    [Serializable()]
    [ExcludeFromCodeCoverage]
    public class PerformingOrgDTO : UpdateableDTO
    {
        public PerformingOrgDTO()
        {
            Id = -1;
            PerformingOrgName = string.Empty;
            PerformingOrgDesc = string.Empty;
            IsSystemPerfOrg = false;
        }

        public string PerformingOrgName { get; set; }
        public string PerformingOrgDesc { get; set; }

        // used only to determine if this performing org is a system or workspace performing org for caching purposes
        public bool IsSystemPerfOrg { get; set; }
    }
}

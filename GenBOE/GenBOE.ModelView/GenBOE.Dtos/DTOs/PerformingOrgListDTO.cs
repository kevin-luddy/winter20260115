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
    public class PerformingOrgListDTO : UpdateableDTO
    {
        public PerformingOrgListDTO()
        {
            PerformingOrgListID = 0;
            PerformingOrgListName = string.Empty;
        }

        public int PerformingOrgListID { get; set; }
        public string PerformingOrgListName { get; set; }
    }
}

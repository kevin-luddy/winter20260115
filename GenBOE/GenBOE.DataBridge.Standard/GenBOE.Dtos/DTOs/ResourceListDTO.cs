// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using IES.Standard;

    [Serializable()]
    [ExcludeFromCodeCoverage]
    public class ResourceListDTO : UpdateableDTO
    {
        public ResourceListDTO()
        {
            ResourceListID = 0;
            ResourceListName = string.Empty;
        }

        public int ResourceListID { get; set; }
        public string ResourceListName { get; set; }
    }
}

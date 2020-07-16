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

    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class GroupDTO : UpdateableDTO
    {
        public GroupDTO()
        {
            ID = -1;
            Name = string.Empty;
        }

        public int ID { get; set; }

        public string Name { get; set; }
    }
}

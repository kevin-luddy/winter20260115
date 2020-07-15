// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using GenBOE.Dtos;
    using IES.Common;

    public interface IResourceTypeLoader : IBulkDataLoader<ResourceTypeDto>
    {
        ICollection<ResourceTypeDto> GetByBoeId(int boeId);
    }
}

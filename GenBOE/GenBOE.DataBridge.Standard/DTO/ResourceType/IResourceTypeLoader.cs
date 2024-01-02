// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using GenBOE.Dtos;
    using IES.Standard;

    public interface IResourceTypeLoader : IBulkDataLoader<ResourceTypeDto>
    {
        ICollection<ResourceTypeDto> GetByBoeId(int boeId);
    }
}

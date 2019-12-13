// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using GenBOE.Dtos;
    using IES.Common;

    public interface IResourceSpreadLoader : IBulkDataLoader<ResourceSpreadDto>
    {
        int DeleteByResourceTypeId(int resourceTypeId);
    }
}

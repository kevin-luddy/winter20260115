// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using GenBOE.Dtos;
    using IES.Standard;

    public interface IResourceSpreadLoader : IBulkDataLoader<ResourceSpreadDto>
    {
        int DeleteByResourceTypeId(int resourceTypeId);
    }
}

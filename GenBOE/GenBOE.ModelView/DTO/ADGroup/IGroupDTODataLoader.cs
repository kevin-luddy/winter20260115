// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using GenBOE.Dtos;

    public interface IGroupDTODataLoader
    {
        /// <summary>
        /// Create a GroupDTO for the given Group Id
        /// </summary>
        /// <param name="inGroupId">the Group Id</param>
        /// <returns>the GroupDTO</returns>
        GroupDTO GetGroupById(int inGroupId);
    }
}

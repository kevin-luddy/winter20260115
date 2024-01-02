// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Linq;
    using IES.Standard;
    using GenBOE.Dtos;
    using GenBOE.Models;

    public class GroupDTODataLoader : IGroupDTODataLoader
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public GroupDTODataLoader() { }

        /// <summary>
        /// Create a GroupDTO for the given Group Id
        /// </summary>
        /// <param name="inGroupId">the Group Id</param>
        /// <returns>the GroupDTO</returns>
        
        virtual public GroupDTO GetGroupById(int inGroupId)
        {
            GroupDTO toReturn = null;

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                toReturn = (from c in gbe.ETIGroups
                                  where c.ETIGroupID == inGroupId
                                  select new GroupDTO
                                  {
                                      ID = c.ETIGroupID,
                                      Name = c.GroupName,
                                      UpdateDate = c.UpdateDT
                                  }).FirstOrDefault();
            }

            return toReturn;
        }
    }
}
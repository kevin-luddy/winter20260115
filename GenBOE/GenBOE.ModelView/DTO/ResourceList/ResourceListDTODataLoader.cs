// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System;
    using System.Linq;
    using IES.Common;
    using GenBOE.Models;
    using GenBOE.Dtos;

    public class ResourceListDTODataLoader : GenBOE.DataBridge.DTO.IResourceListDTODataLoader
    {
        private Logger _log = new Logger(typeof(ResourceListDTODataLoader));
        public ResourceListDTODataLoader() { }

        /// <summary>
        /// Get the resource list DTO
        /// </summary>
        /// <param name="inResourceListID">resource List ID</param>
        /// <returns>resource List DTO</returns>
        [DbQuery]
        virtual public ResourceListDTO GetResourceList(int inResourceListID)
        {
            ResourceListDTO toReturn = null;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
					ResourceListDTO resourceList =
                       (from r in gbe.ResourceLists
                        where r.ResourceListID == inResourceListID
                        select new ResourceListDTO
                        {
                            ResourceListID = r.ResourceListID,
                            ResourceListName = r.ResourceListName,
                            UpdateDate = r.UpdateDT
                        }).FirstOrDefault();

                    toReturn = resourceList;
                }
            }

            return toReturn;
        }

        #region Commit


        /// <summary>
        /// Save the Resource List
        /// </summary>
        /// <param name="inResourceList">default resource list</param>
        virtual public int SaveResourceList(ResourceListDTO inResourceList)
        {
            if (inResourceList == null)
            {
                throw new ArgumentNullException(nameof(inResourceList));
            }
            if (inResourceList.Updateable == UpdateType.None)
            {
                throw new ArgumentException("please supply the Updateable argument");
            }
            if (inResourceList.Updateable == UpdateType.Deleted)
            {
                throw new ArgumentOutOfRangeException(nameof(inResourceList));
            }

            int id = 0;

            if (inResourceList.Updateable == UpdateType.Upsert)
            {
                id = UpdateResourceList(inResourceList);
                inResourceList.ResourceListID = id;
            }


            return id;
        }

        /// <summary>
        /// Clear all resources from the given list
        /// </summary>
        /// <param name="inResourceList"></param>
        virtual public void ClearResourceList(ResourceListDTO inResourceList)
        {
            if (inResourceList == null)
            {
                throw new ArgumentNullException(nameof(inResourceList));
            }

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                if (inResourceList.Updateable == UpdateType.Upsert)
                {
                    gbe.deleteResourceList(inResourceList.ResourceListID, inResourceList.UpdateDate);
                }
            }

        }

        /// <summary>
        /// update a  resource list
        /// </summary>
        /// <param name="inUpdateResourceList">Resource List to update</param>
        private int UpdateResourceList(ResourceListDTO inUpdateResourceList)
        {
            if (inUpdateResourceList == null)
            {
                throw new ArgumentNullException(nameof(inUpdateResourceList));
            }
            int newId = 0;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    newId = gbe.upsertResourceList(inUpdateResourceList.ResourceListID, inUpdateResourceList.ResourceListName, inUpdateResourceList.UpdateDate).FirstOrDefault().Value;
                }
            }

            return newId;
        }

        #endregion Commit
    }
}

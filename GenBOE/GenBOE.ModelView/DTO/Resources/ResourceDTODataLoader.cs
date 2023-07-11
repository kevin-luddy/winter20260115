// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using IES.Common;
    using GenBOE.Dtos;
    using GenBOE.Models;

    public class ResourceDTODataLoader : IResourceDTODataLoader
    {
        private Logger _log = new Logger(typeof(ResourceDTODataLoader));

        // the global resource list ID is always 1
        private const int GLOBAL_LIST_ID = 1;

        public const int RESOURCE_NAME_LENGTH = 20;
        public const int RESOURCE_DESC_LENGTH = 100;

        /// <summary>
        /// Global List Id
        /// </summary>
        virtual public int GlobalListID
        {
            get
            {
                return GLOBAL_LIST_ID;
            }
        }

        public ResourceDTODataLoader() { }

        #region Retrieves

        
        /// <summary>
        /// Get resources given a resource list ID
        /// </summary>
        /// <param name="inResourceListID">resource list ID</param>
        /// <returns>resource dtos</returns>
        [DbQuery]
        virtual public ICollection<ResourceDTO> GetByListId(int inResourceListID)
        {
            List<ResourceDTO> result = new List<ResourceDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    if (inResourceListID == GLOBAL_LIST_ID)
                    {
                        result = (from r in gbe.Resources
                                  where r.ResourceListID == inResourceListID && !r.DeletedFlag
                                  select new ResourceDTO
                                  {
                                      Id = r.ResourceID,
                                      ResourceName = r.ResourceName,
                                      ResourceDesc = r.ResourceDescription,
                                      SegRegion = r.SegmentRegion,
                                      LaborType = r.LaborType,
                                      ElementOfCost = (ElementOfCostType)r.CostElementID,
                                      SegmentTypeInt = r.SegmentID,
                                      UpdateDate = r.UpdateDT,
                                      isSystemResource = r.ResourceListID == GLOBAL_LIST_ID,
                                      RateType = (RateType)r.RateTypeID
                                  }).ToList();
                    }
                    else
                    { 
                    result = (from r in gbe.Resources
                              join w in gbe.WorkspaceResources on r.ResourceID equals w.SystemResourceID
                              where w.ResourceListID == inResourceListID
                              select new ResourceDTO
                              {
                                  Id = r.ResourceID,
                                  ResourceName = r.ResourceName,
                                  ResourceDesc = r.ResourceDescription,
                                  SegRegion = r.SegmentRegion,
                                  LaborType = r.LaborType,
                                  ElementOfCost = (ElementOfCostType)r.CostElementID,
                                  SegmentTypeInt = r.SegmentID,
                                  UpdateDate = r.UpdateDT,
                                  isSystemResource = r.ResourceListID == GLOBAL_LIST_ID,
                                  RateType = (RateType)r.RateTypeID
                              }).ToList();
                    }
                }

                result.ForEach(x => 
                {
                    x.CalculatedSegment = this.FigureOutSegment(x.ResourceName);
                });
            }

            return result;
        }

        /// <summary>
        /// Get resource data by Resource ID
        /// </summary>
        /// <param name="inResourceIDs">resource IDs</param>
        /// <returns>resource data</returns>
        [DbQuery]
        virtual public ICollection<ResourceDTO> GetByIds(ICollection<int> inResourceIDs)
        {
            if (inResourceIDs == null) { throw new ArgumentNullException(nameof(inResourceIDs)); }

            List<ResourceDTO> result;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    result = (from r in gbe.Resources
                              where inResourceIDs.Contains(r.ResourceID)
                              select
                                new ResourceDTO()
                                {
                                   Id = r.ResourceID,
                                   ResourceName = r.ResourceName,
                                   ResourceDesc = r.ResourceDescription,
                                   SegRegion = r.SegmentRegion,
                                   LaborType = r.LaborType,
                                   ElementOfCost = (ElementOfCostType)r.CostElementID,
                                   SegmentTypeInt = r.SegmentID,
                                   UpdateDate = r.UpdateDT,
                                   isSystemResource = r.ResourceListID == GLOBAL_LIST_ID,
                                   RateType = (RateType)r.RateTypeID
                                }).ToList();
                }

                result.ForEach(x =>
                {
                    x.CalculatedSegment = this.FigureOutSegment(x.ResourceName);
                });
            }

            return result;
        }

		/// <summary>
		/// Gets the Resource Names by an incoming list of Resource IDs.
		/// </summary>
		/// <param name="inResourceIDs">Resource IDs</param>
		/// <returns>Collection of Resource Names</returns>
		/// <exception cref="ArgumentNullException">Null exception on no resource IDs.</exception>
		virtual public IList<string> GetResourceNamesByIds(IList<int> inResourceIDs)
        {
			if (inResourceIDs == null) { throw new ArgumentNullException(nameof(inResourceIDs)); }

			IList<string> result;

			using (StopwatchTimer sw = new StopwatchTimer(this._log))
			{
				using (GenBoeEntities gbe = new GenBoeEntities())
				{
					result = (from r in gbe.Resources
							  where inResourceIDs.Contains(r.ResourceID)
							  select r.ResourceName).ToList();
				}
			}

			return result;
		}

        /// <summary>
        /// Get Resource By Resource Name and List Id
        /// </summary>
        /// <param name="inResourceName">Resource Name</param>
        /// <param name="inResourceListID">List Id</param>
        /// <returns>Resource Dto</returns>
        [DbQuery]
        virtual public ResourceDTO GetByNameAndListId(string inResourceName, int inResourceListID)
        {
            ResourceDTO result = null;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    if (inResourceListID == GLOBAL_LIST_ID)
                    {
                        result = (from r in gbe.Resources
                                  where r.ResourceListID == inResourceListID && !r.DeletedFlag
                                    && r.ResourceName == inResourceName
                                  select new ResourceDTO
                                  {
                                      Id = r.ResourceID,
                                      ResourceName = r.ResourceName,
                                      ResourceDesc = r.ResourceDescription,
                                      SegRegion = r.SegmentRegion,
                                      LaborType = r.LaborType,
                                      ElementOfCost = (ElementOfCostType)r.CostElementID,
                                      SegmentTypeInt = r.SegmentID,
                                      UpdateDate = r.UpdateDT,
                                      isSystemResource = r.ResourceListID == GLOBAL_LIST_ID,
                                      RateType = (RateType)r.RateTypeID
                                  }).FirstOrDefault();
                    }
                    else
                    {
                        result = (from r in gbe.Resources
                                  join w in gbe.WorkspaceResources on r.ResourceID equals w.SystemResourceID
                                  where w.ResourceListID == inResourceListID && r.ResourceName == inResourceName
                                  select new ResourceDTO
                                  {
                                      Id = r.ResourceID,
                                      ResourceName = r.ResourceName,
                                      ResourceDesc = r.ResourceDescription,
                                      SegRegion = r.SegmentRegion,
                                      LaborType = r.LaborType,
                                      ElementOfCost = (ElementOfCostType)r.CostElementID,
                                      SegmentTypeInt = r.SegmentID,
                                      UpdateDate = r.UpdateDT,
                                      isSystemResource = r.ResourceListID == GLOBAL_LIST_ID,
                                      RateType = (RateType)r.RateTypeID
                                  }).FirstOrDefault();
                    }
                }

                // only calculate if a result was found from the DB
                if (result != null)
                {
                    result.CalculatedSegment = this.FigureOutSegment(result.ResourceName);
                }
            }

            return result;
        }

        /// <summary>
        /// Get Resource By Resource Desc and List Id
        /// </summary>
        /// <param name="inResourceDescription">Resource Description</param>
        /// <param name="inResourceListID">List Id</param>
        /// <returns>Resource Dto</returns>
        [DbQuery]
        virtual public ResourceDTO GetByDescriptionAndListId(string inResourceDescription, int inResourceListID)
        {
            ResourceDTO result = null;

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    if (inResourceListID == GLOBAL_LIST_ID)
                    {
                        result = (from r in gbe.Resources
                                  where r.ResourceListID == inResourceListID && !r.DeletedFlag
                                    && r.ResourceDescription.Equals(inResourceDescription, StringComparison.CurrentCultureIgnoreCase)
                                  select new ResourceDTO
                                  {
                                      Id = r.ResourceID,
                                      ResourceName = r.ResourceName,
                                      ResourceDesc = r.ResourceDescription,
                                      SegRegion = r.SegmentRegion,
                                      LaborType = r.LaborType,
                                      ElementOfCost = (ElementOfCostType)r.CostElementID,
                                      SegmentTypeInt = r.SegmentID,
                                      UpdateDate = r.UpdateDT,
                                      isSystemResource = r.ResourceListID == GLOBAL_LIST_ID,
                                      RateType = (RateType)r.RateTypeID
                                  }).FirstOrDefault();
                    }
                    else
                    {
                        result = (from r in gbe.Resources
                                  join w in gbe.WorkspaceResources on r.ResourceID equals w.SystemResourceID
                                  where w.ResourceListID == inResourceListID && r.ResourceDescription.Equals(inResourceDescription, StringComparison.CurrentCultureIgnoreCase)
                                  select new ResourceDTO
                                  {
                                      Id = r.ResourceID,
                                      ResourceName = r.ResourceName,
                                      ResourceDesc = r.ResourceDescription,
                                      SegRegion = r.SegmentRegion,
                                      LaborType = r.LaborType,
                                      ElementOfCost = (ElementOfCostType)r.CostElementID,
                                      SegmentTypeInt = r.SegmentID,
                                      UpdateDate = r.UpdateDT,
                                      isSystemResource = r.ResourceListID == GLOBAL_LIST_ID,
                                      RateType = (RateType)r.RateTypeID
                                  }).FirstOrDefault();
                    }
                }

                if (result != null)
                { 
                    result.CalculatedSegment = this.FigureOutSegment(result.ResourceName);
                }
            }

            return result;
        }

        /// <summary>
        /// Get resource IDs by resource List ID and element of cost type
        /// </summary>
        /// <param name="inResourceListID"></param>
        /// <param name="inElementOfCostType"></param>
        /// <returns></returns>
        [DbQuery]
        virtual public ICollection<ResourceDTO> GetByListIdAndElementOfCost(int inResourceListID, ICollection<ElementOfCostType> inElementOfCostTypes)
        {
            if (inElementOfCostTypes == null) { throw new ArgumentNullException(nameof(inElementOfCostTypes)); }

            List<int> elementOfCostIDs = inElementOfCostTypes.Select(x => (int)x).ToList();

            List<ResourceDTO> result = new List<ResourceDTO>();

            using (StopwatchTimer sw = new StopwatchTimer(this._log))
            {
                using (GenBoeEntities gbe = new GenBoeEntities())
                {
                    if (inResourceListID == GLOBAL_LIST_ID)
                    {
                        result = (from r in gbe.Resources
                                  where r.ResourceListID == inResourceListID && !r.DeletedFlag
                                    && elementOfCostIDs.Contains(r.CostElementID)
                                  select new ResourceDTO
                                  {
                                      Id = r.ResourceID,
                                      ResourceName = r.ResourceName,
                                      ResourceDesc = r.ResourceDescription,
                                      SegRegion = r.SegmentRegion,
                                      LaborType = r.LaborType,
                                      ElementOfCost = (ElementOfCostType)r.CostElementID,
                                      SegmentTypeInt = r.SegmentID,
                                      UpdateDate = r.UpdateDT,
                                      isSystemResource = r.ResourceListID == GLOBAL_LIST_ID,
                                      RateType = (RateType)r.RateTypeID
                                  }).ToList();
                    }
                    else
                    {
                        result = (from r in gbe.Resources
                                  join w in gbe.WorkspaceResources on r.ResourceID equals w.SystemResourceID
                                  where w.ResourceListID == inResourceListID && elementOfCostIDs.Contains(r.CostElementID)
                                  select new ResourceDTO
                                  {
                                      Id = r.ResourceID,
                                      ResourceName = r.ResourceName,
                                      ResourceDesc = r.ResourceDescription,
                                      SegRegion = r.SegmentRegion,
                                      LaborType = r.LaborType,
                                      ElementOfCost = (ElementOfCostType)r.CostElementID,
                                      SegmentTypeInt = r.SegmentID,
                                      UpdateDate = r.UpdateDT,
                                      isSystemResource = r.ResourceListID == GLOBAL_LIST_ID,
                                      RateType = (RateType)r.RateTypeID
                                  }).ToList();
                    }
                }

                result.ForEach(x =>
                {
                    x.CalculatedSegment = this.FigureOutSegment(x.ResourceName);
                });
            }

            return result;
        }

        /// <summary>
        /// Is Resource Name Unique?
        /// </summary>
        /// <param name="resourceName">Resource Name</param>
        /// <param name="resourceListID">List Id</param>
        /// <param name="resourceID">Resource Id</param>
        /// <returns>Is resource name unique</returns>
        [DbQuery]
        virtual public bool IsResourceNameUnique(string resourceName, int resourceListID, int? resourceID)
        {
            bool alreadyExists = false;

            if (!string.IsNullOrEmpty(resourceName))
            {
                using (StopwatchTimer sw = new StopwatchTimer(this._log))
                {
                    string lookForName = resourceName.Trim().ToUpper();
                    
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        IEnumerable<int> resourceIds;

                        if (resourceListID == GLOBAL_LIST_ID)
                        {
                            resourceIds = (from r in gbe.Resources
                                             where r.ResourceListID == resourceListID && !r.DeletedFlag && r.ResourceName.ToUpper() == lookForName
                                             select r.ResourceID);
                        }
                        else
                        {
                            resourceIds = (from r in gbe.Resources
                                             join w in gbe.WorkspaceResources on r.ResourceID equals w.SystemResourceID
                                             where w.ResourceListID == resourceListID && r.ResourceName.ToUpper() == lookForName
                                             select r.ResourceID);
                        }

                        alreadyExists = resourceIds.Any(x => x != resourceID);
                    }
                }
            }

            return !alreadyExists;
        }

        /// <summary>
        /// Is Resource Desc Unique
        /// </summary>
        /// <param name="resourceDesc">Resource Description</param>
        /// <param name="resourceListID">List Id</param>
        /// <param name="resourceID">Resource Id</param>
        /// <returns>Is resource description unique</returns>
        [DbQuery]
        virtual public bool IsResourceDescriptionUnique(string resourceDesc, int resourceListID, int? resourceID)
        {
            bool alreadyExists = false;

            if (!string.IsNullOrEmpty(resourceDesc))
            {
                using (StopwatchTimer sw = new StopwatchTimer(this._log))
                {
                    string lookForDesc = resourceDesc.Trim().ToUpper();
                    
                    using (GenBoeEntities gbe = new GenBoeEntities())
                    {
                        IEnumerable<int> resourceIds;

                        if (resourceListID == GLOBAL_LIST_ID)
                        {
                            resourceIds = (from r in gbe.Resources
                                           where r.ResourceListID == resourceListID && !r.DeletedFlag && r.ResourceDescription.ToUpper() == lookForDesc
                                           select r.ResourceID);
                        }
                        else
                        {
                            resourceIds = (from r in gbe.Resources
                                           join w in gbe.WorkspaceResources on r.ResourceID equals w.SystemResourceID
                                           where w.ResourceListID == resourceListID && r.ResourceDescription.ToUpper() == lookForDesc
                                           select r.ResourceID);
                        }

                        alreadyExists = resourceIds.Any(x => x != resourceID);
                    }
                }
            }

            return !alreadyExists;
        }

        /// <summary>
        /// Get Resources By List Id and Element of Cost Type
        /// </summary>
        /// <param name="inResourceListID">List Id</param>
        /// <param name="inElementOfCostType">Element of Cost</param>
        /// <returns>Resource Dtos</returns>
        virtual public ICollection<ResourceDTO> GetByListIdAndElementOfCost(int inResourceListID, ElementOfCostType inElementOfCostType)
        {
            return this.GetByListIdAndElementOfCost(inResourceListID, new Collection<ElementOfCostType>() { inElementOfCostType });
        }

        /// <summary>
        /// Get Global Resources
        /// </summary>
        /// <returns>Global Resources</returns>
        virtual public ICollection<ResourceDTO> GetGlobalResources()
        {
            return this.GetByListId(GLOBAL_LIST_ID);
        }

        /// <summary>
        /// Get Resource by Id
        /// </summary>
        /// <param name="inResourceID">Resource Id</param>
        /// <returns>Resource Dto</returns>
        virtual public ResourceDTO GetById(int inResourceID)
        {
            return this.GetByIds(new Collection<int>() { inResourceID }).FirstOrDefault();
        }

        #endregion

        #region Commit

        /// <summary>
        /// Save the system resources
        /// </summary>
        /// <param name="inSystemResources">the system Resources</param>
        virtual public Dictionary<int, int> SaveSystemResources(ICollection<ResourceDTO> inSystemResources)
        {
            if (inSystemResources == null)
            {
                throw new ArgumentNullException(nameof(inSystemResources));
            }

            Dictionary<int, int> toReturn = new Dictionary<int, int>();

            foreach (ResourceDTO resource in inSystemResources)
            {
                int originalResourceId = resource.Id;
                if (resource.Updateable == UpdateType.None)
                {
                    throw new ArgumentException("please supply the Updateable argument");
                }

                if (resource.Updateable == UpdateType.Deleted)
                {
                    DeleteSystemResource(resource);

                    toReturn.Add(originalResourceId, resource.Id);
                }
                else if (resource.Updateable == UpdateType.Upsert)
                {
                    resource.LaborType = resource.LaborType.Trim();
                    resource.ResourceDesc = resource.ResourceDesc.Trim();
                    resource.ResourceName = resource.ResourceName.Trim();
                    resource.SegRegion = resource.SegRegion.Trim();

                    toReturn.Add(originalResourceId, UpdateSystemResource(resource));
                }
            }
            return toReturn;
        }

        /// <summary>
        /// save the workspace resources
        /// </summary>
        /// <param name="inWorkspaceResources">workspace resources</param>
        /// <returns></returns>
        virtual public Dictionary<int, int> SaveWorkspaceResources(WorkspaceDTO inWorkspace, ICollection<ResourceDTO> inWorkspaceResources)
        {

            if (inWorkspaceResources == null)
            {
                throw new ArgumentNullException(nameof(inWorkspaceResources));
            }
            if (inWorkspace == null)
            {
                throw new ArgumentNullException(nameof(inWorkspace));
            }

            Dictionary<int, int> toReturn = new Dictionary<int, int>();

            foreach (ResourceDTO resource in inWorkspaceResources)
            {
                int originalResourceId = resource.Id;
                if (resource.Updateable == UpdateType.None)
                {
                    throw new ArgumentException("please supply the Updateable argument");
                }

                if (resource.Updateable == UpdateType.Deleted)
                {
                    DeleteWorkspaceResource(resource, inWorkspace.ResourceListID);

                    toReturn.Add(originalResourceId, resource.Id);
                }
                else if (resource.Updateable == UpdateType.Upsert)
                {
                    resource.LaborType = resource.LaborType.Trim();
                    resource.ResourceDesc = resource.ResourceDesc.Trim();
                    resource.ResourceName = resource.ResourceName.Trim();
                    resource.SegRegion = resource.SegRegion.Trim();

                    toReturn.Add(originalResourceId, UpdateWorkspaceResource(resource, inWorkspace.ResourceListID));
                }
            }
            return toReturn;
        }

        /// <summary>
        /// Delete a system resource
        /// </summary>
        /// <param name="inDeleteResource"></param>
        private void DeleteSystemResource(ResourceDTO inDeleteResource)
        {
            if (inDeleteResource == null)
            {
                throw new ArgumentNullException(nameof(inDeleteResource));
            }

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                gbe.deleteSystemResourceByResourceID(inDeleteResource.Id, inDeleteResource.UpdateDate);

            }
        }

        /// <summary>
        /// delete a workspace resource
        /// </summary>
        /// <param name="inDeleteResource"></param>
        private void DeleteWorkspaceResource(ResourceDTO inDeleteResource, int inWorkspaceResourceListID)
        {
            if (inDeleteResource == null)
            {
                throw new ArgumentNullException(nameof(inDeleteResource));
            }

            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                gbe.deleteWorkspaceResourceByResourceID(inDeleteResource.Id, inWorkspaceResourceListID);

            }

        }

        /// <summary>
        /// update a system resource
        /// </summary>
        /// <param name="inUpdateResource">default resource</param>
        private int UpdateSystemResource(ResourceDTO inUpdateResource)
        {
            if (inUpdateResource == null)
            {
                throw new ArgumentNullException(nameof(inUpdateResource));
            }
            int? segmentValue = null;
            int resultID = 0;


            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                if (inUpdateResource.Segment != SegmentType.None)
                {
                    segmentValue = (int)inUpdateResource.Segment;
                }

                resultID = Convert.ToInt32(gbe.upsertSystemResource(
                    inUpdateResource.Id,
                    inUpdateResource.ResourceName,
                    inUpdateResource.ResourceDesc,
                    inUpdateResource.SegRegion,
                    inUpdateResource.LaborType,
                    segmentValue,
                    GLOBAL_LIST_ID,
                    (int)inUpdateResource.ElementOfCost,
                    inUpdateResource.UpdateDate,
                    (int)inUpdateResource.RateType).FirstOrDefault());

                // if the result ID is not a positive number, something bad went wrong so log it
                if (resultID < 0)
                {
                    _log.Error("The returned ID from upsertResource SP was negative");

                }
                else
                {
                    // If this was a new resource, set the new ID on the DTO for later use, if necessary
                    if (inUpdateResource.Id < 0)
                    {
                        inUpdateResource.Id = resultID;
                    }
                }
            }


            return resultID;

        }

        /// <summary>
        /// update a workspace resource
        /// </summary>
        /// <param name="inUpdateResource">default resource</param>
        private int UpdateWorkspaceResource(ResourceDTO inUpdateResource, int inWorkspaceResourceListID)
        {
            if (inUpdateResource == null)
            {
                throw new ArgumentNullException(nameof(inUpdateResource));
            }

            int? segmentValue = null;
            int resultID = 0;


            using (GenBoeEntities gbe = new GenBoeEntities())
            {
                if (inUpdateResource.Segment != SegmentType.None)
                {
                    segmentValue = (int)inUpdateResource.Segment;
                }
                var results = gbe.upsertWorkspaceResource(
                    inUpdateResource.Id,
                    inUpdateResource.ResourceName,
                    inUpdateResource.ResourceDesc,
                    inUpdateResource.SegRegion,
                    inUpdateResource.LaborType,
                    segmentValue,
                    inWorkspaceResourceListID,
                    (int)inUpdateResource.ElementOfCost,
                    inUpdateResource.UpdateDate,
                    (int)inUpdateResource.RateType);

                foreach (int? result in results)
                    {
                        resultID = Convert.ToInt32(result);
                    }
                

                // if the result ID is not a positive number, something bad went wrong so log it
                if (resultID < 0)
                {
                    _log.Error("The returned ID from upsertResource SP was negative");

                }
                else
                {
                    // If this was a new resource, set the new ID on the DTO for later use, if necessary
                    if (inUpdateResource.Id < 0)
                    {
                        inUpdateResource.Id = resultID;
                    }
                }
            }


            return resultID;

        }

        #endregion 

        #region Private Helpers

        /// <summary>
        /// Figures out the segment, based on the resource Name
        /// </summary>
        /// <param name="resourceName">Name of the resource</param>
        /// <returns>Segment</returns>
        private SegmentType FigureOutSegment(string resourceName)
        {
            string resourceIdSegment = string.Empty;

            if (!string.IsNullOrEmpty(resourceName))
            {
                resourceIdSegment = resourceName.Substring(0, 1);
            }

            SegmentType result;

            switch (resourceIdSegment)
            {
                case "1":
                case "2":
                    result = SegmentType.DS;
                    break;
                case "3":
                case "4":
                    result = SegmentType.ES;
                    break;
                case "5":
                case "6":
                    result = SegmentType.TS;
                    break;
                default:
                    result = SegmentType.None;
                    break;
            }

            return result;
        }
        #endregion
    }
}
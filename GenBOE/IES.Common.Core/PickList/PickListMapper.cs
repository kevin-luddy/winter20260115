// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.PickList
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using IES.Common.Core;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Models;
	using IES.Common.Core.Utilities;
	using Microsoft.Extensions.Logging;

	/// <summary>
	/// Pick List Mapper
	/// </summary>
	public abstract class PickListMapper : IPickListMapper
    {
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger log;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected PickListMapper(ILogger<PickListMapper> logger)
        {
            this.log = logger;
        }

        /// <summary>
        /// Get pick lists for the specified type
        /// </summary>
        /// <param name="pickListType">Pick List Type</param>
        /// <param name="loadChildren">if set to <c>true</c> [load children].</param>
        /// <returns>Corresponding Pick List</returns>
        public PickListGridMV GetPickListValues(PickListEnum pickListType, bool loadChildren = false)
        {
            PickListGridMV gridMv = null;

            IPickListLoader loader = this.GetPickListSettings(pickListType);

            if (loader != null)
            {
                using (StopwatchTimer sw = new StopwatchTimer("PickListMapper.GetPickListValues", this.log))
                {
                    ICollection<PickListDto> result = loader.GetPickListValues();

                    gridMv = new PickListGridMV()
                    {
                        PickListId = (int)pickListType,
                        PickListName = pickListType.GetDescription(),
                        PickLists = result ?? new List<PickListDto>(),
                        ContainsParent = loader.ParentPickList.HasValue,
                        AllowsMultipleParents = loader.AllowsMultipleParents,
                        ContainsChildren = loader.ChildrenPickList.HasValue
                    };

                    if (gridMv.ContainsParent)
                    {
                        gridMv.Parents = this.GetSelectListPickList(loader.ParentPickList.Value, null, true, true);
                    }

                    if (loadChildren && gridMv.ContainsChildren)
                    {
                        // Get the Children Loader and load the values
                        IPickListLoader childrenLoader = this.GetPickListSettings(loader.ChildrenPickList.Value);
                        gridMv.Children = childrenLoader.GetPickListValues();
                    }
                }
            }

            return gridMv;
        }

        /// <summary>
        /// Gets the Pick List values in a select list type
        /// </summary>
        /// <param name="pickListType">Pick List Type</param>
        /// <param name="selectedItem">Currently Selected Item</param>
        /// <param name="includeEmptySelect">If true, include the "Select PickList type" in the items.</param>
        /// <param name="includeInactive">If true, include all Inactive items in the dropdown.</param>
        /// <returns>Dropdown selection</returns>
        public ICollection<SelectListItem> GetSelectListPickList(PickListEnum pickListType, int? selectedItem = null, bool includeEmptySelect = true, bool includeInactive = false)
        {
            PickListGridMV gridModelView = this.GetPickListValues(pickListType);
            ICollection<PickListDto> allPicklistData = gridModelView?.PickLists ?? new List<PickListDto>();
            ICollection<PickListDto> dataToUse = allPicklistData.Where(x => includeInactive || x.IsActive).ToList();

            if (selectedItem.HasValue && !dataToUse.Any(x => x.Id == selectedItem.Value) && allPicklistData.Any(x => x.Id == selectedItem.Value))
            {
                dataToUse.Add(allPicklistData.First(x => x.Id == selectedItem.Value));
            }

            // Show " (InActive)" only when explicitly retrieving them
            List<SelectListItem> result = dataToUse.OrderBy(p => p.Text).Select(x => new SelectListItem() { Text = x.IsActive || !includeInactive ? x.Text : x.Text + " (InActive)", Value = x.Id.ToString() }).OrderBy(x => x.Text).ToList();
            if (includeEmptySelect)
            {
                result.Insert(0, new SelectListItem() { Text = "Select " + pickListType.GetDescription(), Value = string.Empty });
            }

            return result;
        }

        /// <summary>
        /// Save pick lists
        /// </summary>
        /// <param name="pickListType">Type of Picklist</param>
        /// <param name="dataToSave">Items to save</param>
        public void SavePickList(PickListEnum pickListType, ICollection<PickListDto> dataToSave)
        {
            if (dataToSave == null || !dataToSave.Any())
            {
                throw new ArgumentNullException(nameof(dataToSave));
            }

            IPickListLoader loader = this.GetPickListSettings(pickListType);

            if (loader != null)
            {
                foreach (PickListDto pickList in dataToSave)
                {
                    loader.Save(pickList);
                }
            }
        }

        /// <summary>
        /// Gets loader for the specified pick list type
        /// </summary>
        /// <param name="pickListType">Pick list that we want to work on</param>
        /// <returns>The correct pick list</returns>
        protected abstract IPickListLoader GetPickListSettings(PickListEnum pickListType);

        /// <summary>
        /// Determines whether a pick list value is active.
        /// </summary>
        /// <param name="pickList">The pick list.</param>
        /// <param name="pickListId">The pick list identifier.</param>
        /// <returns>
        ///   <c>true</c> if pick list is active; otherwise, <c>false</c>.
        /// </returns>
        public bool IsPickListActive(PickListEnum pickList, int pickListId)
        {
            bool result = false;
            PickListDto dto = this.GetById(pickList, pickListId);
            if (dto != null)
            {
                return dto.IsActive;
            }

            return result;
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="contractType">Type of pick list for the children.</param>
        /// <param name="parentId">The parent id of the children.</param>
        /// <returns></returns>
        public ICollection<PickListDto> GetChildren(PickListEnum pickList, int parentId)
        {
            ICollection<PickListDto> result = new List<PickListDto>();
            PickListGridMV gridMV = this.GetPickListValues(pickList);

            if (gridMV != null)
            {
                result = gridMV.PickLists.Where(p => p.ParentIds.Contains(parentId)).ToList();
            }

            return result;
        }

        /// <summary>
        /// Gets the picklist by identifier.
        /// </summary>
        /// <param name="pickList">The pick list.</param>
        /// <param name="pickListId">The pick list identifier.</param>
        /// <returns>PickList DTO or null if not found.</returns>
        public PickListDto GetById(PickListEnum pickList, int pickListId)
        {
            PickListDto dto = null;

            PickListGridMV gridMV = this.GetPickListValues(pickList);

            if (gridMV != null)
            {
                dto = gridMV.PickLists.FirstOrDefault(p => p.Id == pickListId);
            }

            return dto;
        }
    }
}
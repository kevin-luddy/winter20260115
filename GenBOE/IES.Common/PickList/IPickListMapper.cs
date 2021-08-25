// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.PickList
{
    using System.Collections.Generic;
    using System.Web.Mvc;
    using IES.Common;

    /// <summary>
    /// Interface for pick list mapper
    /// </summary>
    public interface IPickListMapper
    {
        /// <summary>
        /// Get pick lists for the specified type
        /// </summary>
        /// <param name="pickListType">Pick List Type</param>
        /// <param name="loadChildren">if set to <c>true</c> [load children].</param>
        /// <returns>Corresponding Pick List</returns>
        PickListGridMV GetPickListValues(PickListEnum pickListType, bool loadChildren = false);

        /// <summary>
        /// Save pick lists
        /// </summary>
        /// <param name="pickListType">Type of Picklist</param>
        /// <param name="dataToSave">Items to save</param>
        void SavePickList(PickListEnum pickListType, ICollection<PickListDto> dataToSave);

        /// <summary>
        /// Gets the Pick List values in a select list type
        /// </summary>
        /// <param name="pickListType">Pick List Type</param>
        /// <param name="selectedItem">Currently Selected Item</param>
        /// <param name="includeEmptySelect">If true, include the "Select PickList type" in the items.</param>
        /// <param name="includeInactive">If true, include all inactive items in the dropdown.</param>
        /// <returns>Dropdown selection</returns>
        ICollection<SelectListItem> GetSelectListPickList(PickListEnum pickListType, int? selectedItem = null, bool includeEmptySelect = true, bool includeInactive = false);

        /// <summary>
        /// Determines whether a pick list value is active.
        /// </summary>
        /// <param name="pickList">The pick list.</param>
        /// <param name="pickListId">The pick list identifier.</param>
        /// <returns>
        ///   <c>true</c> if pick list is active; otherwise, <c>false</c>.
        /// </returns>
        bool IsPickListActive(PickListEnum pickList, int pickListId);

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="contractType">Type of pick list for the children.</param>
        /// <param name="parentId">The parent id of the children.</param>
        /// <returns></returns>
        ICollection<PickListDto> GetChildren(PickListEnum pickList, int parentId);

        /// <summary>
        /// Gets the picklist by identifier.
        /// </summary>
        /// <param name="pickList">The pick list.</param>
        /// <param name="pickListId">The pick list identifier.</param>
        /// <returns>PickList DTO or null if not found.</returns>
        PickListDto GetById(PickListEnum pickList, int pickListId);
    }
}

// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Standard.PickList
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using IES.Standard;

    /// <summary>
    /// Pick lists abstract loader class
    /// </summary>
    public abstract class PickListLoader : DataLoader<PickListDto>, IPickListLoader
    {
        /// <summary>
        /// Gets the PickList items by Id.
        /// </summary>
        /// <param name="ids">Ids</param>
        /// <returns>The picklists that match the IDs.</returns>
        public override ICollection<PickListDto> GetByIds(ICollection<int> ids)
        {
            return this.GetPickListValues().Where(p => ids.Contains(p.Id)).ToList();
        }

        /// <summary>
        /// Abstract method for retrieving Pick List Values
        /// </summary>
        /// <returns>Pick list values</returns>
        abstract public ICollection<PickListDto> GetPickListValues();

        /// <summary>
        /// Save pick lists
        /// </summary>
        /// <param name="picklistsToSave">Items to save</param>
        public virtual void SavePickList(ICollection<PickListDto> picklistsToSave)
        {
            if (picklistsToSave == null)
            {
                throw new ArgumentNullException(nameof(picklistsToSave));
            }

            foreach (PickListDto item in picklistsToSave)
            {
                this.Save(item);
            }
        }

        /// <summary>
        /// Gets the parent pick list type, if applicable.
        /// </summary>
        public virtual PickListEnum? ParentPickList
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the children pick list.
        /// </summary>
        public virtual PickListEnum? ChildrenPickList
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this Pick List allows multiple parents.
        /// </summary>
        public virtual bool AllowsMultipleParents
        {
            get
            {
                return false;
            }
        }
    }
}
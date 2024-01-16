// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.PickList
{
	using System.Collections.Generic;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Loaders;

	/// <summary>
	/// Pick lists loader interface, keeping for Unit Tests
	/// </summary>
	public interface IPickListLoader : IDataLoader<PickListDto>
    {
        /// <summary>
        /// Get pick lists for the specified type
        /// </summary>
        /// <returns>Corresponding Pick List</returns>
        ICollection<PickListDto> GetPickListValues();

        /// <summary>
        /// Gets the parent pick list type, if applicable.
        /// </summary>
        PickListEnum? ParentPickList { get; }

        /// <summary>
        /// Gets the children pick list.
        /// </summary>
        PickListEnum? ChildrenPickList { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this Pick List allows multiple parents.
        /// </summary>
        bool AllowsMultipleParents { get; }
    }
}

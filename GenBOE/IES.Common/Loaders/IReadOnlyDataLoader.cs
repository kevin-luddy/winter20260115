// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface for the base class ReadOnlyDataLoader
    /// </summary>
    public interface IReadOnlyDataLoader<TDtoType> where TDtoType : IUpdateableDTO
    {
        /// <summary>
        /// Get item by Id
        /// </summary>
        /// <param name="id">Item Id</param>
        /// <returns>Item by the primary key</returns>
        TDtoType GetById(int id);

        /// <summary>
        /// Gets all objects based on the IDs that were passed in
        /// </summary>
        /// <param name="ids">IDs</param>
        /// <returns>Corresponding Data</returns>
        ICollection<TDtoType> GetByIds(ICollection<int> ids);
    }
}

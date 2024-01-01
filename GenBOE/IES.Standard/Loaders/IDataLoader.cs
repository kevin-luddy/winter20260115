// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Standard
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface for the base class DataLoader
    /// </summary>
    public interface IDataLoader<TDtoType> : IReadOnlyDataLoader<TDtoType> where TDtoType : IUpdateableDTO
    {
        /// <summary>
        /// Saves the specified Dto into the database, either deleting or upserting it.
        /// </summary>
        /// <param name="dtoToSave">Dto to save.</param>
        /// <returns>Wbs Id for the saved item, or null if save failed.</returns>
        int? Save(TDtoType dtoToSave);

        /// <summary>
        /// Save method to process collection of dtos to save
        /// </summary>
        /// <param name="dtosToSave">collection of Dtos</param>
        /// <returns>Dictionary of saved object ids</returns>
        Dictionary<int, int> Save(ICollection<TDtoType> dtosToSave);
    }
}

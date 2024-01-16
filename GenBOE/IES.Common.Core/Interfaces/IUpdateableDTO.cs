// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common.Core.Interfaces
{
    using System;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Models;

    /// <summary>
    /// Base interface for the Updateable DTOs
    /// </summary>
    public interface IUpdateableDTO
    {
        /// <summary>
        /// Unique Id for the DTO
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// Last Update Date
        /// </summary>
        DateTime UpdateDate { get; set; }

        /// <summary>
        /// Update Type
        /// </summary>
        UpdateType Updateable { get; set; }

        /// <summary>
        /// Updates the id (if its a new id) and the update date (if provided). Updateable is reset to none. 
        /// </summary>
        /// <param name="Id">New id value</param>
        /// <param name="newUpdateDate">new update date</param>
        /// <exception cref="ArgumentException">Thrown if the newOrExistingId is less than 0 if it's new or does not match the dto's existing id.</exception>
        /// <exception cref="InvalidOperationException">Thrown if update type is not 'Upsert'</exception>
        void Update(int newOrExistingId, DateTime? newUpdateDate);
    }
}

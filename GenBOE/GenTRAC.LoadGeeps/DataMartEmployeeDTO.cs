// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.LoadGeeps
{
    using System;
    using GenTRAC.Models;
    using IES.Common;

    public partial class DataMartEmployeeDTO : DataMartEmployee, IUpdateableDTO
    {
        /// <summary>
        /// Hard-coded to -1 so that this is always an Insert.
        /// </summary>
        public int Id { get { return -1; } set { } }

        /// <summary>
        /// Not used since this will always be a new Insert.
        /// </summary>
        public DateTime UpdateDate { get { return DateTime.Now; } set { } }

        /// <summary>
        /// Hard-coded to Upsert since this will always be a new Insert.
        /// </summary>
        public UpdateType Updateable { get { return UpdateType.Upsert; } set { } }

        /// <summary>
        /// Updates the id (if its a new id) and the update date (if provided). Updateable is reset to none. 
        /// </summary>
        /// <param name="Id">New id value</param>
        /// <param name="newUpdateDate">new update date</param>
        public void Update(int newOrExistingId, DateTime? newUpdateDate)
        {
        }
    }
}

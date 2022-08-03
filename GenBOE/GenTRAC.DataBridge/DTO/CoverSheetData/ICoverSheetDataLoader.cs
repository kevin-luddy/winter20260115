// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using IES.Common;

    /// <summary>
    /// Cover Sheet Data Loader Interface
    /// </summary>
    public interface ICoverSheetDataLoader
    {
        /// <summary>
        /// Gets Cover Sheet data by a given proposal id.
        /// </summary>
        /// <param name="id">Proposal Id</param>
        /// <returns>Cover Sheet DTO</returns>
        CoverSheetDataDto GetCoverSheetDataById(int id);
    }
}

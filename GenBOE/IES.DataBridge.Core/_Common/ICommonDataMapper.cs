// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.Common
{
    using System.Collections.Generic;
    using IES.Core;

    /// <summary>
    /// Interface for the Common Data Mappers
    /// </summary>
    public interface ICommonDataMapper
    {
        /// <summary>
        /// Gets all Resource Classes as an options list.
        /// </summary>
        /// <param name="revisionId">Revision Id</param>
        /// <returns>Resource Class options list.</returns>
        ICollection<OptionModelView> GetResourceClassOptions(int revisionId);
    }
}

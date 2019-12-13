// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.Loaders
{
    using System.Collections.Generic;
    using IES.Common;
    using IES.DataBridge.ModelViews;

    /// <summary>
    /// Interface for Cobra Years Loader
    /// </summary>
    public interface ICobraYearsLoader : IDataLoader<CobraYearGridModelView>
    {
        /// <summary>
        /// Get All Cobra Year Configuration Data
        /// </summary>
        /// <returns>All Cobra Year Data</returns>
        ICollection<CobraYearGridModelView> GetAll();

        /// <summary>
        /// GetCobraExportRows
        /// </summary>
        /// <param name="id">Revision Id</param>
        /// <param name="startYear">Start Year</param>
        /// <returns>All COBRA rates for Start Year and beyond.</returns>
        ICollection<CobraExportRowModelView> GetCobraExportRows(int id, int startYear);
    }
}

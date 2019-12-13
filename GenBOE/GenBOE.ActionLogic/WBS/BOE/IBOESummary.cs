// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.WBS.BOE
{
    using System.Collections.Generic;
    using GenBOE.ActionLogic.IO.Export.BOE;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;

    public interface IBOESummary
    {
        /// <summary>
        /// Get a collection of all Summary Grid Model Views for the given BOE.
        /// </summary>
        /// <param name="boe">The BOE</param>
        /// <param name="exportInputs">The export inputs.</param>
        /// <param name="isSubcontractorUser">Whether the user is a subcontractor</param>
        /// <returns>
        /// All summary grid model views for the BOE
        /// </returns>
        ICollection<BOESummaryGridModelView> GetBOESummaryGridModelViews(BoeDTO boe, BOEExportInputs exportInputs, bool isSubcontractorUser);        
    }
}

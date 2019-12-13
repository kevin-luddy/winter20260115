// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenTRAC.DataBridge.DTO
{
    using System.Collections.Generic;
    using IES.Common.PickList;

    /// <summary>
    /// IOrgStructureDataLoader
    /// </summary>
    public interface IOrgStructureDataMapper
    {
        /// <summary>
        /// Get All Lines of Business
        /// </summary>
        /// <returns>Collection of LineOfBusiness DTOs</returns>
        ICollection<PickListDto> GetAllLinesOfBusiness();

        /// <summary>
        /// Get Line of Business by Id
        /// </summary>
        /// <param name="id">Line of Business Id</param>
        /// <returns>LineOfBusinessDTO with data</returns>
        PickListDto GetLineOfBusinessById(int id);

        /// <summary>
        /// Get Lines of Business by IDs
        /// </summary>
        /// <param name="ids">Collection of Lines of Business</param>
        /// <returns>Collection of Line of Business Dtos</returns>
        ICollection<PickListDto> GetLineOfBusinessById(ICollection<int> ids);

        /// <summary>
        /// Get All Program Areas
        /// </summary>
        /// <returns>Collection of ProgramArea DTOs</returns>
        ICollection<PickListDto> GetAllProgramAreas();

        /// <summary>
        /// Get Program Area by Id
        /// </summary>
        /// <param name="id">Program Area Id</param>
        /// <returns>ProgramAreaDTO with data</returns>
        PickListDto GetProgramAreaById(int id);

        /// <summary>
        /// Get Program Areas by IDs
        /// </summary>
        /// <param name="ids">Collection of Program Areas</param>
        /// <returns>Collection of ProgramArea Dtos</returns>
        ICollection<PickListDto> GetProgramAreaById(ICollection<int> ids);

        /// <summary>
        /// Return available Program Areas as HTML select items for selected line of business
        /// </summary>
        /// <param name="lineOfBusinessId">Line of Business to filter</param>
        /// <param name="programAreaId">Optional Program Area that is selected.</param>
        /// <param name="includeDefaultSelect">Should the "Select Program Area" option be included</param>
        /// <returns>Program Area</returns>
        string GetProgramAreaHtmlOptionsForLineOfBusiness(int? lineOfBusinessId, int? programAreaId, bool includeDefaultSelect = true);

        /// <summary>
        /// Create the dynamic Program Area help text for active and inactive Program Areas
        /// </summary>
        /// <returns>String to display as help text for the Program Area field</returns>
        string GetProgramAreaDynamicHelpText();
    }
}

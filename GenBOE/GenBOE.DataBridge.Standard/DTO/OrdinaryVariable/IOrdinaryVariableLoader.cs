// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using System.Collections.Generic;
    using GenBOE.Dtos;
    using IES.Standard;

    public interface IOrdinaryVariableLoader : IBulkDataLoader<OrdinaryVariableDto>
    {
        ICollection<int> GetIdsByTaskElementId(int taskElementId);

        /// <summary>
        /// Gets Ordinary Variables By Task Element Ids
        /// </summary>
        /// <param name="taskElementIds">task element Ids</param>
        /// <returns>Corresponding data</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        ICollection<OrdinaryVariableDto> GetByTaskElementIds(HashSet<int?> taskElementIds);
    }
}

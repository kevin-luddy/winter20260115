// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.DTO
{
    using GenBOE.Dtos;

    public interface IMileReimbursementRateDTOLoader
    {
        void SaveMileReimbursementRateDTO(MileReimbursementRateDTO inMileReimbursementRateDTO);

        MileReimbursementRateDTO GetMileReimbursementRateDTO();
    }
}

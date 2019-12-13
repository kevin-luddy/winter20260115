// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.WBS
{
    using System.Collections.ObjectModel;
    using GenBOE.Dtos;

    public interface INestedWBSUtilities
    {
        void AdjustLevels(Collection<WbsDTO> WbsDTOs);

        void SetParentsAndChildrenInUse(Collection<WbsDTO> WbsDTOs);

        void SetParentsInUse(Collection<WbsDTO> WbsDTOs);

        void SetChildrenInUse(Collection<WbsDTO> WbsDTOs);

        Collection<string> GetParentsWBSNumByChildWBS(string inWBSNumber);

        Collection<string> GetParentsWBSNumByChildWBS(WbsDTO inWBS);
    }
}

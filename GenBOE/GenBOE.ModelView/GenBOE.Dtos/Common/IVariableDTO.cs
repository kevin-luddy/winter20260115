// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.Generic;
using IES.Common;

namespace GenBOE.Dtos
{
    public interface IVariableDTO
    {
        int Id { get; }
        ICollection<SelectBOEsToSum> SelectedBOEsToSum { get; }
        ICollection<int> TaskElementIds { get; }
        VariableType VariableType { get; }
        UpdateType Updateable { get; }
    }
}

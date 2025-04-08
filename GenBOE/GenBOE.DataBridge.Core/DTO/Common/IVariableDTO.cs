// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------
using System.Collections.Generic;
using IES.Common;
using IES.Common.Core.Enums;

namespace GenBOE.DataBridge.Core.DTO.Common
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

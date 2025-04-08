// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.DTO
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Diagnostics.CodeAnalysis;
	using GenBOE.DataBridge.Core.DTO.Common;
	using IES.Common;
	using IES.Common.Core.Enums;
	using IES.Common.Core.Models;

	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class OrdinaryVariableDto : UpdateableDTO, IBOEMembership, IVariableDTO
	{
		public OrdinaryVariableDto()
		{
			Id = 0;
			OrdinaryVariableName = string.Empty;
			OrdinaryVariableValue = 0;
			ValueType = VarValueType.Discrete;
			SortBOEBy = VarSortBOEBy.CLIN;
			SelectedBOEsToSum = new Collection<SelectBOEsToSum>();
			IsPercentage = false;
			SumVariableResourceTypeIDs = new Collection<int>();
			DefaultSize = string.Empty;
		}

		public VariableType VariableType { get { return VariableType.Task; } }

		public decimal? OrdinaryVariableValue { get; set; }
		public string OrdinaryVariableName { get; set; }
		public VarValueType ValueType { get; set; }
		public VarSortBOEBy SortBOEBy { get; set; }
		public int BoeID { get; set; }


		// If VarValueType is SumOfBOEs, then we want to save Selected BOEs to Sum
		public ICollection<SelectBOEsToSum> SelectedBOEsToSum { get; set; }

		public ICollection<int> SumVariableResourceTypeIDs { get; set; }
		public bool? IsPercentage { get; set; }
		public int TaskElementId { get; set; }

		public ICollection<int> TaskElementIds { get { return new List<int> { TaskElementId }; } }

		/// <summary>
		/// The size of the variable when/if metric was imported.
		/// </summary>
		public string DefaultSize { get; set; }

		#region UpdateableDTO

		/// <summary>
		/// Propagates the new 'parent' DTO Id to all first level 'child' DTOs in collections.
		/// </summary>
		/// <param name="newParentId">new id of the parent DTO</param>
		override protected void PropagateNewParentIdToChildDTOs(int newParentId)
		{
			//// now update the child DTOs in each collection on this element
			if (SelectedBOEsToSum != null && SelectedBOEsToSum.Count > 0)
			{
				foreach (SelectBOEsToSum dto in SelectedBOEsToSum)
				{
					dto.OrdinaryVariableID = newParentId;
				}
			}
		}
		#endregion UpdateableDTO
	}
}

// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using IES.Standard;

    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class OrdinaryVariableDto : UpdateableDTO, IBOEMembership, IVariableDTO
    {
        public OrdinaryVariableDto()
        {
            Id = 0;
            OrdinaryVariableName = String.Empty;
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


        /// <summary>
        /// These are used for data load.. During the load the data is stored here temporarily, then it's placed into the public property and cleared out
        /// </summary>
        internal IEnumerable<SelectBOEsToSum> SelectedBOEsToSumIEnum { get; set; }

        // If VarValueType is SumOfBOEs, then we want to save Selected BOEs to Sum
        public ICollection<SelectBOEsToSum> SelectedBOEsToSum { get; set; }

        /// <summary>
        /// These are used for data load.. During the load the data is stored here temporarily, then it's placed into the public property and cleared out
        /// </summary>
        internal IEnumerable<int> SumVariableResourceTypeIDsIEnum { get; set; }

        public ICollection<int> SumVariableResourceTypeIDs { get; set; }
        public bool? IsPercentage { get; set; }
        public int TaskElementId { get; set; }

        public ICollection<int> TaskElementIds { get { return new List<int> { this.TaskElementId }; } }

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
            if (this.SelectedBOEsToSum != null && this.SelectedBOEsToSum.Count > 0)
            {
                foreach (SelectBOEsToSum dto in this.SelectedBOEsToSum)
                {
                    dto.OrdinaryVariableID = newParentId;
                }
            }
        }
        #endregion UpdateableDTO
    }
}

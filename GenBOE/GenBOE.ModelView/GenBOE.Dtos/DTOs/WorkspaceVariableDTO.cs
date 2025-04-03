using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using IES.Common;
using Newtonsoft.Json;

namespace GenBOE.Dtos
{
    [Serializable()]
    [ExcludeFromCodeCoverage]
    public class WorkspaceVariableDTO : UpdateableDTO, IWorkspaceMembership, IVariableDTO
    {
        public WorkspaceVariableDTO()
        {
            Id = -1;
            WorkspaceVariableName = string.Empty;
            WorkspaceVariableValue = 0.00m;
            WorkspaceID = -1;
            ValueType = VarValueType.Discrete;
            SortBOEBy = VarSortBOEBy.CLIN;
            InUse = false;
            SelectedBOEsToSum = new Collection<SelectBOEsToSum>();
            IsPercentage = false;
            SumVariableResourceTypeIDs = new Collection<int>();
            TaskElementIds = new Collection<int>();
        }

        public VariableType VariableType { get { return VariableType.Workspace; } }

        public string WorkspaceVariableName { get; set; }
        public decimal WorkspaceVariableValue { get; set; }
        public int WorkspaceID { get; set; }
        public VarValueType ValueType { get; set; }
        public VarSortBOEBy SortBOEBy { get; set; }

        // If a WorkspaceVarValueType is SumOfBOEs, then we want to save Selected BOEs to Sum
        public ICollection<SelectBOEsToSum> SelectedBOEsToSum { get; set; }
        
        public bool InUse { get; set; }
        public bool? IsPercentage { get; set; }
        public Collection<int> SumVariableResourceTypeIDs { get; set; }

        public ICollection<int> TaskElementIds { get; set; }

        /// <summary>
        /// This is an internal object, for use only when making DB calls
        /// </summary>
        internal IEnumerable<int> TaskElementIdsIEnum { get; set; }

        /// <summary>
        /// This is an internal object, for use only when making DB calls
        /// </summary>
        internal IEnumerable<int> SumVariableResourceTypeIDsIEnum { get; set; }

        /// <summary>
        /// This is an internal object, for use only when making DB calls
        /// </summary>
        [JsonIgnore]
		internal IEnumerable<SelectBOEsToSum> SelectedBOEsToSumIEnum { get; set; }
    }
}

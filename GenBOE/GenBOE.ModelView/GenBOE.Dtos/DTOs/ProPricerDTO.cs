using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using IES.Common;

namespace GenBOE.Dtos
{
    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class ProPricerDTO : UpdateableDTO
    {
        public ProPricerDTO()
        {
            ExportID = -1;
            Scope = ProPricerScope.System;
            WorkspaceID = null;
            ProPricerTasks = new Collection<ProPricerTasks>();
            ProPricerResources = new Collection<ProPricerResources>();
            FormatName = string.Empty;
        }

        public int ExportID { get; set; }
        public ProPricerScope Scope { get; set; }
        public string FormatName { get; set; }


        /// <summary>
        /// These are used for data load.. During the load the data is stored here temporarily, then it's placed into the public property and cleared out
        /// </summary>
        internal IEnumerable<ProPricerTasks> ProPricerTasksIEnum { get; set; }

        public ICollection<ProPricerTasks> ProPricerTasks { get; set; }

        /// <summary>
        /// These are used for data load.. During the load the data is stored here temporarily, then it's placed into the public property and cleared out
        /// </summary>
        internal IEnumerable<ProPricerResources> ProPricerResourcesIEnum { get; set; }

        public ICollection<ProPricerResources> ProPricerResources { get; set; }

        // the workspaceID will be null if Scope is System
        public int? WorkspaceID { get; set; }

        /// <summary>
        /// Set to true if workspace is project map type
        /// </summary>
        public bool IsProjectMapTemplate { get; set; }
    }

    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class ProPricerTasks
    {
        public ProPricerTasks()
        {
            Selection = ProPricerCustomFieldSelection.None;       
        }

        public int ListOrder { get; set; }
        public ProPricerField_Task Task { get; set;}

        /// <summary>
        /// Used for Workspace scope.
        /// </summary>
        public int? CustomFieldID { get; set; }

        /// <summary>
        /// Used for System scope.
        /// </summary>
        public string CustomFieldName { get; set; }
        public ProPricerCustomFieldSelection Selection { get; set; }
        public bool IsProjectMapField { get; set; }
    }

    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class ProPricerResources
    {
        public ProPricerResources()
        {
            Selection = ProPricerCustomFieldSelection.None;
        }

        /// <summary>
        /// Order of this resource.
        /// </summary>
        public int ListOrder { get; set; }

        /// <summary>
        /// Resource Type. CLIN Number, CLIN Title, DISCRETE, etc.
        /// </summary>
        public ProPricerField_Resources Resource { get; set; }

        /// <summary>
        /// Custom field Id if this is a custom field, used for Workspace Scope.
        /// </summary>
        public int? CustomFieldID { get; set; }

        /// <summary>
        /// Custom field Id if this is a custom field, used for System scope.
        /// </summary>
        public string CustomFieldName { get; set; }
        
        /// <summary>
        /// Custom field, Not a custom field or Custom Field description.
        /// </summary>
        public ProPricerCustomFieldSelection Selection { get; set; }

        /// <summary>
        /// Set to true if ProPricer field is custom project map type
        /// </summary>
        public bool IsProjectMapField { get; set; }
    }


}

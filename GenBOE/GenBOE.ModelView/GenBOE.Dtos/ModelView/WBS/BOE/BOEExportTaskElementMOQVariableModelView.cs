// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using System.Collections.ObjectModel;

    public class BOEExportTaskElementMOQVariableModelView
    {
        public BOEExportTaskElementMOQVariableModelView()
        {
            OrdinaryVariableName = String.Empty;
            Total = 0;
            ReferencedBOEs = new Collection<BOEExportTaskElementMOQVariableBOEModelView>();
        }
        public String OrdinaryVariableName { get; set; }
        public decimal Total { get; set; }
        public Collection<BOEExportTaskElementMOQVariableBOEModelView> ReferencedBOEs { get; set; }
    }

    public class BOEExportTaskElementMOQVariableBOEModelView
    {
        public BOEExportTaskElementMOQVariableBOEModelView()
        {
            WBSNumber = String.Empty;
            WBSTitle = String.Empty;
        }
        public String WBSNumber { get; set; }
        public String WBSTitle { get; set; }
        public String ClinNumber { get; set; }
        public decimal Total { get; set; }
    }
}

// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core.DTO.Export.BOE
{
	using System;
	using System.Collections.ObjectModel;

	public class BOEExportTaskElementMOQVariableModelView
	{
		public BOEExportTaskElementMOQVariableModelView()
		{
			OrdinaryVariableName = string.Empty;
			Total = 0;
			ReferencedBOEs = new Collection<BOEExportTaskElementMOQVariableBOEModelView>();
		}
		public string OrdinaryVariableName { get; set; }
		public decimal Total { get; set; }
		public Collection<BOEExportTaskElementMOQVariableBOEModelView> ReferencedBOEs { get; set; }
	}

	public class BOEExportTaskElementMOQVariableBOEModelView
	{
		public BOEExportTaskElementMOQVariableBOEModelView()
		{
			WBSNumber = string.Empty;
			WBSTitle = string.Empty;
		}
		public string WBSNumber { get; set; }
		public string WBSTitle { get; set; }
		public string ClinNumber { get; set; }
		public decimal Total { get; set; }
	}
}

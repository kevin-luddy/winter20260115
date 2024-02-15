// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2022 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.IO.Export
{
	using System.Collections.Generic;
	using GenBOE.ActionLogic.ModelView;
	using GenBOE.Objects;

	/// <summary>
	/// Interface for TraceTableExporter
	/// </summary>
	public interface ITraceTableExporter
	{
		/// <summary>
		/// Export Trace Table Data
		/// </summary>
		/// <param name="workspace">Full Workspace</param>
		/// <param name="settingsData">Trace Table Settings Data</param>
		/// <returns>Trace Table Data</returns>
		ICollection<TraceTableBoeData> ExportTraceTableData(FullWorkspace workspace, TraceTableSettingsData settingsData);
        ICollection<TraceTableBoeDataGroup> ExportTraceTableDataGroup(FullWorkspace workspace, TraceTableSettingsData settingsData);
    }
}
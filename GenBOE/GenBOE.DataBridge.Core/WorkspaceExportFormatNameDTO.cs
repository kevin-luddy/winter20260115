// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.DataBridge.Core
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using IES.Common.Core.Models;

	/// <summary>
	/// Workspace Export Format Meta Data (Name/Description).  This class does not include the File itself.
	/// </summary>
	[ExcludeFromCodeCoverage]
    [Serializable()]
    public class WorkspaceExportFormatNameDTO
    {
		/// <summary>
		/// #ctor
		/// </summary>
        public WorkspaceExportFormatNameDTO()
        {
            ExportFormat = new ExcelReportTemplate();
            ExportFormatName = string.Empty;
            ExportFormatDescription = string.Empty;
            IsAvailableToAllWorkspaces = false;
			Id = -1;
        }

		/// <summary>
		/// Id of the Export Template
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Last Update Date
		/// </summary>
		public DateTime UpdateDate { get; set; }

		/// <summary>
		/// The unique ID for this DTO
		/// </summary>
		public ExcelReportTemplate ExportFormat { get; set; }

        /// <summary>
        /// The description for the export format (i.e. 'Portrait', 'Landscape', ...)
        /// </summary>
        public string ExportFormatName { get; set; }

        /// <summary>
        /// A longer description for this template
        /// </summary>
        public string ExportFormatDescription { get; set; }

        /// <summary>
        /// Whether or not this is an active template.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets/Sets whether template is available to all workspaces
        /// </summary>
        public bool IsAvailableToAllWorkspaces { get; set; }
    }
}

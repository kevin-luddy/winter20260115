using System;
using System.Diagnostics.CodeAnalysis;
using IES.Common;
using System.Web;
using System.IO;
using IES.Common.Core.Models;
using IES.Common.Core.Enums;

namespace GenBOE.DataBridge.Core.WorkspaceExportFormat
{
	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class WorkspaceExportFormatDTO : UpdateableDTO
	{
		public WorkspaceExportFormatDTO()
		{
			ExportFormat = new ExcelReportTemplate();
			ExportFormatName = string.Empty;
			ExportFormatDescription = string.Empty;
			FileData = null;
			IsAvailableToAllWorkspaces = false;
		}

		public WorkspaceExportFormatDTO(string templateFilePath)
			: this()
		{
			this.templateFilePath = templateFilePath;
		}

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
		/// The file/template itself
		/// </summary>
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public byte[] FileData { get; set; }

		/// <summary>
		/// Whether or not this is an active template.
		/// </summary>
		public bool IsActive { get; set; }

		/// <summary>
		/// Gets/Sets whether template is available to all workspaces
		/// </summary>
		public bool IsAvailableToAllWorkspaces { get; set; }

		/// <summary>
		/// Location of the template file on disk
		/// </summary>
		private string templateFilePath = null;

		///// <summary>
		///// Get the location of the file on disk.  This is a temporary copy that is flushed whenever there is a build
		///// but it allows copying to take place for exports, etc.
		///// </summary>
		//public string PhysicalFilePathCache
		//{
		//	get
		//	{
		//		// check to see if the temporary file already exists
		//		string physicalFileLocation = templateFilePath == null ? HttpContext.Current.Server.MapPath("~/Templates/Export/Temp/" + ExportFormatName +
		//			(ExportFormat.TemplateType == ExcelReportTemplateType.MASTER ||
		//			  ExportFormat.TemplateType == ExcelReportTemplateType.LMSI_STANDARD_LANDSCAPE
		//			  ? ".docx" : ".xslx")) : templateFilePath;

		//		//added back in for 2.4, will be updated to not create temp files in 2.5
		//		//currently required for templates uploaded via UI
		//		if (!File.Exists(physicalFileLocation))
		//		{
		//			// not found, create it
		//			File.WriteAllBytes(physicalFileLocation, FileData);
		//		}

		//		return physicalFileLocation;
		//	}
		//}
	}
}

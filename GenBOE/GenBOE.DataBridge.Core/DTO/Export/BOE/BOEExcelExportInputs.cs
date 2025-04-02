namespace GenBOE.DataBridge.Core.DTO.Export.BOE
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using GenTRAC.DataBridge.Core.DTO.User;

	public class BOEExcelExportInputs
	{
		/// <summary>
		/// Gets all of the boes in the workspace.
		/// </summary>
		public ICollection<BoeDTO> Boes { get; }

		/// <summary>
		/// Gets the clins.
		/// </summary>
		public ICollection<ClinDTO> Clins { get; set; }

		/// <summary>
		/// Gets the WBS elements.
		/// </summary>
		public ICollection<WbsDTO> WbsElements { get; set; }

		/// <summary>
		/// Authors for BOEs
		/// </summary>
		public ICollection<UserDTO> Authors { get; set; }

		/// <summary>
		/// Subcontractor Authors for BOEs
		/// </summary>
		public ICollection<UserDTO> SubcontractorAuthors { get; set; }

		/// <summary>
		/// Approvers for BOEs
		/// </summary>
		public ICollection<UserDTO> Approvers { get; set; }

		/// <summary>
		/// BOE Permissions
		/// </summary>
		public ICollection<PermissionsDTO> BOEPermissions { get; set; }

		/// <summary>
		/// BOE Users based off BOE Permissions
		/// </summary>
		public ICollection<UserDTO> BOEUsers { get; set; }

		/// <summary>
		/// Should this export to a blank template
		/// </summary>
		public bool IsBlankTemplate { get; set; }

		/// <summary>
		/// The BOE Template File Location
		/// </summary>
		public string TemplateFileLocation { get; set; }

		/// <summary>
		/// The Workspace Name
		/// </summary>
		public string WorkspaceName { get; set; }
	}
}

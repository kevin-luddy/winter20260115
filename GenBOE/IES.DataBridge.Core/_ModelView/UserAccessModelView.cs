// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2025 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.DataBridge.Core._ModelView
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	/// <summary>
	/// View Model for User Access
	/// </summary>
	public class UserAccessModelView
	{
		/// <summary>
		/// Gets or sets the flag whether the user is RDM Admin.
		/// </summary>
		public bool IsRdmAdminUser { get; set; }

		/// <summary>
		/// Gets or sets the flag whether the user is RDM Cobra Admin.
		/// </summary>
		public bool IsRdmCobraAdminUser { get; set; }
	}
}

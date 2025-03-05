using System;
using System.Diagnostics.CodeAnalysis;

namespace GenBOE.DataBridge.Core.ModelView
{
	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class RoleModelView
	{
		public RoleModelView()
		{
			RoleID = 0;
			RoleName = string.Empty;
		}

		public int RoleID { get; set; }

		public string RoleName { get; set; }
	}
}

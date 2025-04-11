using System.Diagnostics.CodeAnalysis;
using System;

namespace GenBOE.DataBridge.Core.ModelView
{
	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class ExportFormatModelView
	{
		public ExportFormatModelView()
		{
			ExportFormatID = 0;
			ExportFormatName = string.Empty;
		}

		public int ExportFormatID { get; set; }

		public string ExportFormatName { get; set; }
	}
}
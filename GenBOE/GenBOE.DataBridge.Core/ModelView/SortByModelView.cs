using System;
using System.Diagnostics.CodeAnalysis;

namespace GenBOE.DataBridge.Core.ModelView
{
	[ExcludeFromCodeCoverage]
	[Serializable()]

	public class SortByModelView
	{
		public SortByModelView()
		{
			SortByID = 0;
			SortBy = string.Empty;
		}

		public int SortByID { get; set; }

		public string SortBy { get; set; }
	}
}

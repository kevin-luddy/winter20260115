using System;
using System.Diagnostics.CodeAnalysis;

namespace GenBOE.DataBridge.Core.ModelView
{
	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class EnumTypeModelView
	{
		public EnumTypeModelView()
		{
			EnumTypeID = 0;
			EnumTypeName = string.Empty;
		}

		public int EnumTypeID { get; set; }

		public string EnumTypeName { get; set; }
	}
}

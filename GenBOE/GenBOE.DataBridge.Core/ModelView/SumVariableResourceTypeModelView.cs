using System;
using System.Diagnostics.CodeAnalysis;

namespace GenBOE.DataBridge.Core.ModelView
{
	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class SumVariableResourceTypeModelView
	{
		public SumVariableResourceTypeModelView()
		{
			SumVariableResourceTypeID = 0;
			SumVariableResourceTypeName = string.Empty;
		}

		public int SumVariableResourceTypeID { get; set; }
		public string SumVariableResourceTypeName { get; set; }

	}
}

namespace GenBOE.DataBridge.Core.ModelView
{
	using System;
	using System.Diagnostics.CodeAnalysis;

	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class RateTypeModelView
	{
		public RateTypeModelView()
		{
			RateTypeID = 0;
			RateTypeName = string.Empty;
		}

		public int RateTypeID { get; set; }
		public string RateTypeName { get; set; }
	}
}

using System;
using System.Diagnostics.CodeAnalysis;
using IES.Common;
using IES.Common.Core.Enums;

namespace GenBOE.DataBridge.Core.ModelView
{
	[ExcludeFromCodeCoverage]
	[Serializable()]
	public class OtherDirectCostSpreadCurveModelView
	{
		public OtherDirectCostSpreadCurveModelView()
		{
			SpreadCurveID = SpreadCurves.DiscreteHours;
			SpreadCurveName = string.Empty;
		}

		public SpreadCurves SpreadCurveID { get; set; }

		public string SpreadCurveName { get; set; }

	}
}

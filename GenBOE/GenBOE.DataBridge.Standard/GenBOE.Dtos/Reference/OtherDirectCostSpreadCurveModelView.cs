using System;
using System.Diagnostics.CodeAnalysis;
using IES.Standard;

namespace GenBOE.Dtos
{
    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class OtherDirectCostSpreadCurveModelView
    {
        public OtherDirectCostSpreadCurveModelView()
        {
            SpreadCurveID = SpreadCurves.DiscreteHours;
            SpreadCurveName = String.Empty;
        }

        public SpreadCurves SpreadCurveID { get; set; }

        public string SpreadCurveName { get; set; }

    }
}

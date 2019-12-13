using System;
using System.Diagnostics.CodeAnalysis;
using IES.Common;

namespace GenBOE.Dtos
{
    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class SpreadCurveModelView
    {
        public SpreadCurveModelView()
        {
            SpreadCurveID = SpreadCurves.DiscreteHours;
            SpreadCurveName = String.Empty;
        }

        public SpreadCurves SpreadCurveID { get; set; }

        public string SpreadCurveName { get; set; }

    }
}
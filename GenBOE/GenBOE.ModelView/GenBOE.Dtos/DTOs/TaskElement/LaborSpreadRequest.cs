// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using IES.Common;

    public class LaborSpreadRequest
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LaborSpreadRequest()
        {
            CurveID = null;
            StartDate = DateTime.MinValue;
            EndDate = DateTime.MinValue;
            HourSpread = 0;
            this.ForceZeroPrecisionForSpread = false;
        }

        /// <summary>
        /// Constructor. Sets all the properties
        /// </summary>
        /// <param name="inCurveID">The <see cref="SpreadCurves"/> to use for the spread request</param>
        /// <param name="inStartDate">The start date of the spread request</param>
        /// <param name="inEndDate">The end date of the spread request</param>
        /// <param name="inHourSpread">The hours to spread</param>
        public LaborSpreadRequest(SpreadCurves? inCurveID, DateTime inStartDate, DateTime inEndDate, decimal inHourSpread)
        {
            CurveID = inCurveID;
            StartDate = inStartDate;
            EndDate = inEndDate;
            HourSpread = inHourSpread;
            this.ForceZeroPrecisionForSpread = false;
        }

        /// <summary>
        /// The <see cref="SpreadCurves"/> to use for the spread request
        /// </summary>
        public SpreadCurves? CurveID { get; set;}

        /// <summary>
        /// The start date of the spread request
        /// </summary>
        public DateTime StartDate { get; set;}

        /// <summary>
        /// The end date of the spread request
        /// </summary>
        public DateTime EndDate { get; set;}

        /// <summary>
        /// The hours to spread
        /// </summary>
        public decimal HourSpread { get; set; }

        /// <summary>
        /// If true, it will ignore the setting in the workspace; example usage in ODC
        /// </summary>
        public bool ForceZeroPrecisionForSpread { get; set; }
    }
}

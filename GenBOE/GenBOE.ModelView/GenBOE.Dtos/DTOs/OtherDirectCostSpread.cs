// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Dtos
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using IES.Common;

    [ExcludeFromCodeCoverage]
    [Serializable()]
    public class OtherDirectCostSpread : UpdateableDTO, IBOEMembership
    {
        public OtherDirectCostSpread()
        {
            ODCSpreadID = -1;
            ODCSpreadDate = DateTime.MinValue;
            CostSpreadValue = 0;
        }

        // the ODC Spread ID
        public int? ODCSpreadID { get; set; }

        // the month associated with the ODC spread
        public DateTime? ODCSpreadDate { get; set; }

        // the ODC spread cost
        public long? CostSpreadValue { get; set; }
        public int BoeID { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using IES.Common;
using GenBOE.Dtos;

namespace GenBOE.Web.ModelView
{
    public class ODCSpreadModelView
    {
        public ODCSpreadModelView()
        {
            ODCSpreadID = -1;
            ODCSpreadDate = DateTime.MinValue;
            CostSpreadValue = 0;
        }

        public ODCSpreadModelView(OtherDirectCostSpread inBoeLaborSpread)
            : this()
        {
            if (inBoeLaborSpread != null)
            {
                ODCSpreadID = inBoeLaborSpread.ODCSpreadID;
                ODCSpreadDate = inBoeLaborSpread.ODCSpreadDate.HasValue ? GenBOEUtilities.AdjustDateTimePrecision(inBoeLaborSpread.ODCSpreadDate.Value, DateTimePrecision.Month) : inBoeLaborSpread.ODCSpreadDate;
                CostSpreadValue = (decimal)inBoeLaborSpread.CostSpreadValue/100;
            }
        }
        public int? ODCSpreadID { get; set; }

        public DateTime? ODCSpreadDate { get; set; }

        [RegularExpression(@"(^[+-]?\d{0,10}([.]\d{1,2})?$)", ErrorMessage = "Cost Spread must be between -9,999,999,999.99 and 9,999,999,999.99 and contain only 2 decimal places.")]
        public decimal CostSpreadValue { get; set; }

        public bool Deleted { get; set; }

    }
}

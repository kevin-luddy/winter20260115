// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
    using System.ComponentModel.DataAnnotations;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.Dtos;
    using IES.Common;
    public class ODCTypesGridModelView : PersistedDataModelView
    {
        public ODCTypesGridModelView()
        {
            ODCTypeID = -1;
            ResourceDescription = string.Empty;
            ResourceID = -1;
            PerformingOrgName = string.Empty;
            StartDate = "01/1970";
            NumberOfMonths = 0;
            EndDate = "01/1970";
            ODCSpreadCurveID = SpreadCurves.DiscreteHours;
            Cost = 0;
            UpdateUserID = 0;
            Deleted = false;
            BOESummaryText = string.Empty;
        }

        public ODCTypesGridModelView(OtherDirectCostType inODCTypeDTO)
            : this()
        {
            if (inODCTypeDTO != null)
            {
                ODCTypeID = inODCTypeDTO.ODCTypeID;
                ResourceID = inODCTypeDTO.ResourceID;
                PerformingOrgID = inODCTypeDTO.PerformingOrgID;
                ODCSpreadCurveID = inODCTypeDTO.SpreadCurve;
                ResourceDescription = string.Empty;
                if (inODCTypeDTO.Cost.HasValue)
                {
                    Cost = inODCTypeDTO.Cost.Value;
                }
                StartDate = inODCTypeDTO.StartDate.Value.ToString("MM/yyyy");
                EndDate = inODCTypeDTO.EndDate.Value.ToString("MM/yyyy");
                UpdateDate = inODCTypeDTO.UpdateDate;

            }
        }

        public int? ODCTypeID { get; set; }

        public int? ResourceID { get; set; }

        [RequiredIf("Deleted", false, ErrorMessage = "Resource is required or is not valid.")]
        public string ResourceDescription { get; set; }

        [RequiredIf("Deleted", false, ErrorMessage = "Performing organization is required or is not valid.")]
        [Range(1, int.MaxValue, ErrorMessage = "Performing Org is invalid.")]
        public int? PerformingOrgID { get; set; }

        public string PerformingOrgName { get; set; }

        [RequiredIf("Deleted", false, ErrorMessage = "Spread curve is required.")]
        public SpreadCurves ODCSpreadCurveID { get; set; }

        [RequiredIf("Deleted", false, ErrorMessage = "Start Date is required.")]
        [RegularExpression(ValidationConstants.DATE_MONTH_YEAR, ErrorMessage = "Start Date must be in MM/YYYY format.")]
        [DisplayFormat(DataFormatString = "{0:MM/yyyy}", ApplyFormatInEditMode = true)]
        public string StartDate { get; set; }

        public int NumberOfMonths { get; set; }

        [RequiredIf("Deleted", false, ErrorMessage = "End Date is required.")]
        [RegularExpression(ValidationConstants.DATE_MONTH_YEAR, ErrorMessage = "End Date must be in MM/YYYY format.")]
        [DisplayFormat(DataFormatString = "{0:MM/yyyy}", ApplyFormatInEditMode = true)]
        public string EndDate { get; set; }

        [RegularExpression(@"(^[+-]?\d{0,10}([.]\d{1,2})?$)", ErrorMessage = "Cost Spread must be between -9,999,999,999.99 and 9,999,999,999.99 and contain only 2 decimal places.")]
        public decimal Cost { get; set; }

        public int UpdateUserID { get; set; }

        public bool Deleted { get; set; }

        public string BOESummaryText { get; set; }


    }
}

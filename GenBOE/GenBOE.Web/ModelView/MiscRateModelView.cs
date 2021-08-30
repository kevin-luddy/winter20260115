// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using GenBOE.ActionLogic.ModelView;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.Dtos;

    public class MiscRateModelView : PersistedDataModelView
    {
        public MiscRateModelView()
        {
            MiscTravelRateID = -1;
            MiscTravelRateMode = string.Empty;
            MiscTravelRate = 0;
            SortCode = 0;
            MiscTravelRateTwoDecimals = string.Empty;
        }

        public MiscRateModelView(MiscTravelRateDTO inMiscTravelRateDTO)
        {
            if (inMiscTravelRateDTO == null)
            {
                throw new ArgumentNullException(nameof(inMiscTravelRateDTO));
            }

            MiscTravelRateID = inMiscTravelRateDTO.Id;
            MiscTravelRateMode = inMiscTravelRateDTO.MiscTravelRateMode;
            MiscTravelRate = inMiscTravelRateDTO.MiscTravelRate;
            SortCode = inMiscTravelRateDTO.SortCode;
            inUse = inMiscTravelRateDTO.inUse;
            MiscTravelRateTwoDecimals = (Math.Truncate(100 * MiscTravelRate) / 100).ToString();
            if (!MiscTravelRateTwoDecimals.Contains("."))
            {
                MiscTravelRateTwoDecimals += ".00";
            }
        }

        public int MiscTravelRateID { get; set; }

        [Required(ErrorMessage = "Mode is required.")]
        public string MiscTravelRateMode { get; set; }

        [Required(ErrorMessage = "Rate is required.")]
        [RegularExpression(ValidationConstants.MISC_RATE_DECIMAL, ErrorMessage = "Rate must be in the form of nnnnn.nn where n is a numeric number.")]
        public decimal MiscTravelRate { get; set; }

        public string MiscTravelRateTwoDecimals { get; set; }

        [Required(ErrorMessage = "Sort Code is required.")]
        public int SortCode { get; set; }
        public bool inUse { get; set; }
        public bool Deleted { get; set; }

    }
}